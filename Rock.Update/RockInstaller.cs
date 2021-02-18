using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using Microsoft.Web.XmlTransform;
using Newtonsoft.Json;
using RestSharp;
using Rock.Data;
using Rock.Model;
using Rock.Update.Interfaces;
using Rock.Utility;
using Rock.Web.Cache;
using Rock.Web.Utilities;

namespace Rock.Update
{
    public class RockInstaller
    {
        private const string LOCAL_ROCK_PACKAGE_FOLDER = "App_Data\\RockShop";
        private const string BACKUP_FOLDER = "App_Data\\RockBackup";

        private readonly string _backupPath = Path.Combine( FileManagementHelper.ROOT_PATH, BACKUP_FOLDER );
        private readonly IRockUpdateService _rockUpdateService;
        private readonly string _versionBackupPath;
        private readonly Version _targetVersion;
        private readonly Version _installedVersion;

        public RockInstaller(IRockUpdateService rockUpdateService, Version targetVersion, Version installedVersion )
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
            var targetRelease = releases.Where( r => r.SemanticVersion == _targetVersion.ToString() ).FirstOrDefault();

            if ( targetRelease == null )
            {
                throw new Exception( $"Target Release ${targetRelease} was not found." );
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
                var originalPath = file.Replace( BACKUP_FOLDER, string.Empty );

                var backupDirectory = Path.GetDirectoryName( originalPath );
                if ( !Directory.Exists( backupDirectory ) )
                {
                    Directory.CreateDirectory( backupDirectory );
                }

                FileManagementHelper.RenameFile( originalPath );
                File.Move( file, originalPath );
            }
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
            var transformFileSuffix = ".rock.xdt";
            var contentFilesToProcess = packageZip
                .Entries
                .Where( e => e.FullName.StartsWith( "content/", StringComparison.OrdinalIgnoreCase ) )
                .Where( e => !e.FullName.EndsWith( transformFileSuffix, StringComparison.OrdinalIgnoreCase ) );

            // unzip content folder and process xdts
            foreach ( ZipArchiveEntry entry in contentFilesToProcess )
            {
                // process all content files
                string fullpath = Path.Combine( FileManagementHelper.ROOT_PATH, entry.FullName.Replace( "content/", string.Empty ) );
                string directory = Path.GetDirectoryName( fullpath ).Replace( "content/", string.Empty );

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
            var transformFileSuffix = ".rock.xdt";
            var transformFilesToProcess = packageZip
                .Entries
                .Where( e => e.FullName.StartsWith( "content/", StringComparison.OrdinalIgnoreCase ) )
                .Where( e => e.FullName.EndsWith( transformFileSuffix, StringComparison.OrdinalIgnoreCase ) );

            foreach ( ZipArchiveEntry entry in transformFilesToProcess )
            {
                // process xdt
                string filename = entry.FullName.Replace( "content/", "" );
                string transformTargetFile = Path.Combine( FileManagementHelper.ROOT_PATH, filename.Substring( 0, filename.LastIndexOf( transformFileSuffix ) ) );

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
            var relativeTargetPath = filepathToBackup.Replace( FileManagementHelper.ROOT_PATH, string.Empty );

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

                if ( !Directory.Exists( Path.GetDirectoryName( backupFilePath ) ) )
                {
                    Directory.CreateDirectory( Path.GetDirectoryName( backupFilePath ) );
                }

                FileManagementHelper.DeleteOrRename( backupFilePath );
                File.Copy( filepathToBackup, backupFilePath );
            }
        }
    }
}
