// <copyright>
// Copyright by the Spark Development Network
//
// Licensed under the Rock Community License (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.rockrms.com/license
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
// </copyright>

using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using Microsoft.Web.XmlTransform;
using Rock.Model;
using Rock.Update.Exceptions;
using Rock.Update.Helpers;
using Rock.Update.Interfaces;
using Rock.Update.Models;

namespace Rock.Update
{
    public class RockInstaller
    {
        private const string LOCAL_ROCK_PACKAGE_FOLDER = "App_Data\\RockShop";
        private const string BACKUP_FOLDER = "App_Data\\RockBackup";
        private const string TRANSFORM_FILE_SUFFIX = ".rock.xdt";
        private const string CONTENT_PATH = "content/";

        private readonly string _backupPath = Path.Combine( FileManagementHelper.ROOT_PATH, BACKUP_FOLDER );
        private readonly IRockUpdateService _rockUpdateService;
        private readonly string _versionBackupPath;
        private readonly Version _targetVersion;
        private readonly Version _installedVersion;

        public RockInstaller( IRockUpdateService rockUpdateService, Version targetVersion, Version installedVersion )
        {
            _rockUpdateService = rockUpdateService;
            _targetVersion = targetVersion;
            _installedVersion = installedVersion;
            _versionBackupPath = Path.Combine( _backupPath, installedVersion.ToString() );
        }

        public RockRelease InstallVersion()
        {
            VersionValidationHelper.ValidateVersionInstall( _targetVersion );

            var releases = _rockUpdateService.GetReleasesList( _installedVersion );
            var targetRelease = releases?.Where( r => r.SemanticVersion == _targetVersion.ToString() ).FirstOrDefault();

            if ( targetRelease == null )
            {
                throw new PackageNotFoundException( $"Target Release ${targetRelease} was not found." );
            }

            var targetPackagePath = DownloadPackage( targetRelease );

            InstallPackage( targetPackagePath );

            // Record the current version to the database
            Web.SystemSettings.SetValue( Rock.SystemKey.SystemSetting.ROCK_INSTANCE_ID, _targetVersion.ToString() );

            // register any new REST controllers
            try
            {
                RestControllerService.RegisterControllers();
            }
            catch ( Exception ex )
            {
                ExceptionLogService.LogException( ex );
            }

            return targetRelease;
        }

        private void InstallPackage( string targetPackagePath )
        {
            OfflinePageHelper.CreateOfflinePage();
            try
            {
                using ( ZipArchive packageZip = ZipFile.OpenRead( targetPackagePath ) )
                {
                    ClearPreviousBackups();
                    ProcessTransformFiles( packageZip );
                    ProcessContentFiles( packageZip );
                    ProcessDeleteFiles( packageZip );
                    FileManagementHelper.CleanUpDeletedFiles();
                }
            }
            catch
            {
                RestoreOriginal();
                throw;
            }
            finally
            {
                OfflinePageHelper.RemoveOfflinePage();
            }
        }

        private void ClearPreviousBackups()
        {
            if ( !Directory.Exists( _backupPath ) )
            {
                return;
            }
            try
            {
                Directory.Delete( _backupPath, true );
            }
            catch ( Exception ex )
            {
                // We're logging the exception but otherwise ignoring it because this will run again the next install.
                ExceptionLogService.LogException( ex );
            }
        }

        private void RestoreOriginal()
        {
            if ( !Directory.Exists( _versionBackupPath ) )
            {
                return;
            }

            var filesToRestore = Directory.GetFiles( _versionBackupPath, "*.*", SearchOption.AllDirectories );

            foreach ( var file in filesToRestore )
            {
                try
                {
                    var originalPath = Path.Combine( FileManagementHelper.ROOT_PATH, file.Replace( _versionBackupPath.EnsureTrailingBackslash(), string.Empty ) );

                    var backupDirectory = Path.GetDirectoryName( originalPath );
                    if ( !Directory.Exists( backupDirectory ) )
                    {
                        Directory.CreateDirectory( backupDirectory );
                    }

                    FileManagementHelper.RenameFile( originalPath );
                    File.Move( file, originalPath );
                }
                catch ( Exception ex )
                {
                    ExceptionLogService.LogException( ex );
                }
            }

            FileManagementHelper.CleanUpDeletedFiles();
        }

        private void ProcessDeleteFiles( ZipArchive packageZip )
        {
            // process deletefile.lst
            var deleteListEntry = packageZip.Entries.Where( e => e.FullName == "install\\deletefile.lst" ).FirstOrDefault();
            if ( deleteListEntry != null )
            {
                var deleteList = System.Text.Encoding.Default.GetString( deleteListEntry.Open().ReadBytesToEnd() );
                var itemsToDelete = deleteList.Split( new string[] { Environment.NewLine }, StringSplitOptions.None );

                foreach ( var deleteItem in itemsToDelete )
                {
                    if ( !string.IsNullOrWhiteSpace( deleteItem ) )
                    {
                        var rockWeb = "RockWeb\\";
                        var deleteItemFullPath = Path.Combine( FileManagementHelper.ROOT_PATH, deleteItem );
                        if ( deleteItem.StartsWith( rockWeb ) )
                        {
                            deleteItemFullPath = Path.Combine( FileManagementHelper.ROOT_PATH, deleteItem.Substring( rockWeb.Length ) );
                        }

                        var backupFilePath = GetBackupFileLocation( deleteItemFullPath );

                        if ( Directory.Exists( deleteItemFullPath ) )
                        {
                            // Don't actually delete just move the directory to the backup folder.
                            Directory.Move( deleteItemFullPath, backupFilePath );
                        }

                        if ( File.Exists( deleteItemFullPath ) )
                        {
                            var backupDirectory = Path.GetDirectoryName( backupFilePath );
                            if ( !Directory.Exists( backupDirectory ) )
                            {
                                Directory.CreateDirectory( backupDirectory );
                            }

                            File.Move( deleteItemFullPath, backupFilePath );
                        }
                    }
                }

            }
        }

        private void ProcessContentFiles( ZipArchive packageZip )
        {
            var contentFilesToProcess = packageZip
                .Entries
                .Where( e => e.FullName.StartsWith( CONTENT_PATH, StringComparison.OrdinalIgnoreCase ) )
                .Where( e => !e.FullName.EndsWith( TRANSFORM_FILE_SUFFIX, StringComparison.OrdinalIgnoreCase ) );

            // unzip content folder and process xdts
            foreach ( ZipArchiveEntry entry in contentFilesToProcess )
            {
                // process all content files
                string fullpath = Path.Combine( FileManagementHelper.ROOT_PATH, entry.FullName.Replace( CONTENT_PATH, string.Empty ) );
                string directory = Path.GetDirectoryName( fullpath ).Replace( CONTENT_PATH, string.Empty );

                ThrowTestExceptions( Path.GetFileNameWithoutExtension( entry.FullName ) );

                // if entry is a directory ignore it
                if ( entry.Length != 0 )
                {
                    BackupFile( fullpath );

                    if ( !Directory.Exists( directory ) )
                    {
                        Directory.CreateDirectory( directory );
                    }

                    FileManagementHelper.RenameActiveFile( fullpath );

                    entry.ExtractToFile( fullpath, true );
                }
            }
        }

        private void ThrowTestExceptions( string exception )
        {
            var exceptionList = new Dictionary<string, Exception>
            {
                {"exception", new Exception("Test Exception") },
                {"ioexception", new IOException("Test IO Exception") },
                {"outofmemoryexception", new OutOfMemoryException("Test Out of Memory Exception") },
                {"versionvalidationexception", new Exception("Test Version Validation Exception") },
                {"xmlexception", new Exception("XML Exception") },

            };

            exception = exception.ToLower();
            if ( exceptionList.ContainsKey( exception ) )
            {
                throw exceptionList[exception];
            }

        }
        private string DownloadPackage( RockRelease release )
        {
            if ( release.PackageUri.IsNullOrWhiteSpace() )
            {
                throw new Exception( $"Target Release ${release} doesn't have a Package URI specified." );
            }

            var localRockPackageDirectory = Path.Combine( FileManagementHelper.ROOT_PATH, LOCAL_ROCK_PACKAGE_FOLDER );

            if ( !Directory.Exists( localRockPackageDirectory ) )
            {
                Directory.CreateDirectory( localRockPackageDirectory );
            }

            var localRockPackagePath = Path.Combine( localRockPackageDirectory, $"{release.SemanticVersion}.rockpkg" );
            FileManagementHelper.DeleteOrRename( localRockPackagePath );

            try
            {
                var wc = new WebClient();
                wc.DownloadFile( release.PackageUri, localRockPackagePath );
            }
            catch
            {
                FileManagementHelper.DeleteOrRename( localRockPackagePath );
                throw;
            }

            return localRockPackagePath;
        }

        private void ProcessTransformFiles( ZipArchive packageZip )
        {
            var transformFilesToProcess = packageZip
                .Entries
                .Where( e => e.FullName.StartsWith( CONTENT_PATH, StringComparison.OrdinalIgnoreCase ) )
                .Where( e => e.FullName.EndsWith( TRANSFORM_FILE_SUFFIX, StringComparison.OrdinalIgnoreCase ) );

            foreach ( ZipArchiveEntry entry in transformFilesToProcess )
            {
                // process xdt
                string filename = entry.FullName.Replace( CONTENT_PATH, "" );
                string transformTargetFile = Path.Combine( FileManagementHelper.ROOT_PATH, filename.Substring( 0, filename.LastIndexOf( TRANSFORM_FILE_SUFFIX ) ) );

                // process transform
                using ( XmlTransformableDocument document = new XmlTransformableDocument() )
                {
                    document.PreserveWhitespace = true;
                    document.Load( transformTargetFile );

                    using ( XmlTransformation transform = new XmlTransformation( entry.Open(), null ) )
                    {
                        if ( transform.Apply( document ) )
                        {
                            BackupFile( transformTargetFile );
                            document.Save( transformTargetFile );
                        }
                    }
                }
            }
        }

        private string GetBackupFileLocation( string filepathToBackup )
        {
            var relativeTargetPath = filepathToBackup.Replace( FileManagementHelper.ROOT_PATH.EnsureTrailingBackslash(), string.Empty );

            if ( !Directory.Exists( _versionBackupPath ) )
            {
                Directory.CreateDirectory( _versionBackupPath );
            }

            return Path.Combine( _versionBackupPath, relativeTargetPath );
        }

        private void BackupFile( string filepathToBackup )
        {
            if ( File.Exists( filepathToBackup ) )
            {
                var backupFilePath = GetBackupFileLocation( filepathToBackup );
                var backupDirectory = Path.GetDirectoryName( backupFilePath );
                if ( !Directory.Exists( backupDirectory ) )
                {
                    Directory.CreateDirectory( backupDirectory );
                }

                FileManagementHelper.DeleteOrRename( backupFilePath );
                File.Copy( filepathToBackup, backupFilePath );
            }
        }
    }
}
