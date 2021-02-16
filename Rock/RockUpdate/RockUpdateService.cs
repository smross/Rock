using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Web.Hosting;
using Microsoft.Web.XmlTransform;
using Newtonsoft.Json;
using RestSharp;
using Rock.Web.Cache;

namespace Rock.RockUpdate
{
    /// <summary>
    /// Rock Update Service
    /// </summary>
    public class RockUpdateService
    {
        private const string GET_RELEASE_LIST_URL = "http://localhost:57822/api/RockUpdate/GetReleasesList";
        private const string GET_RELEASE_LIST_SINCE_URL = "http://localhost:57822/api/RockUpdate/GetReleasesListSinceVersion";
        private const string EARLY_ACCESS_URL = "http://www.rockrms.com/api/RockUpdate/GetEarlyAccessStatus";
        private const string EARLY_ACCESS_REQUEST_URL = "http://www.rockrms.com/earlyaccessissues?RockInstanceId=";
        private const string LOCAL_ROCK_PACKAGE_FOLDER = "App_Data/RockShop";


        private readonly string _rootPath = HostingEnvironment.MapPath( "~/" );

        /// <summary>
        /// Gets the releases list from the rock server.
        /// </summary>
        /// <returns></returns>
        public List<RockRelease> GetReleasesList( Version version )
        {
            var request = new RestRequest( Method.GET );

            request.RequestFormat = DataFormat.Json;

            request.AddParameter( "rockInstanceId", Web.SystemSettings.GetRockInstanceId() );
            request.AddParameter( "releaseProgram", GetRockReleaseProgram().ToString().ToLower() );

            if ( version != null )
            {
                request.AddParameter( "sinceVersion", version.ToString() );
            }

            var client = new RestClient( version != null ? GET_RELEASE_LIST_SINCE_URL : GET_RELEASE_LIST_URL );
            var response = client.Execute( request );

            if ( response.StatusCode == HttpStatusCode.OK || response.StatusCode == HttpStatusCode.Accepted )
            {
                return JsonConvert.DeserializeObject<List<RockRelease>>( response.Content );
            }

            return new List<RockRelease>();
        }

        /// <summary>
        /// Checks the early access status of this organization.
        /// </summary>
        /// <returns></returns>
        public bool IsEarlyAccessInstance()
        {
            var client = new RestClient( EARLY_ACCESS_URL );
            var request = new RestRequest( Method.GET );
            request.RequestFormat = DataFormat.Json;

            request.AddParameter( "rockInstanceId", Web.SystemSettings.GetRockInstanceId() );
            IRestResponse response = client.Execute( request );
            if ( response.StatusCode == HttpStatusCode.OK || response.StatusCode == HttpStatusCode.Accepted )
            {
                return response.Content.AsBoolean();
            }

            return false;
        }

        /// <summary>
        /// Gets the rock early access request URL.
        /// </summary>
        /// <returns></returns>
        public string GetRockEarlyAccessRequestUrl()
        {
            return $"{EARLY_ACCESS_REQUEST_URL}{Web.SystemSettings.GetRockInstanceId()}";
        }

        /// <summary>
        /// Gets the rock release program.
        /// </summary>
        /// <returns></returns>
        public RockReleaseProgram GetRockReleaseProgram()
        {
            var releaseProgram = RockReleaseProgram.Production;

            var updateUrl = GlobalAttributesCache.Get().GetValue( "UpdateServerUrl" );
            if ( updateUrl.Contains( RockReleaseProgram.Alpha.ToString().ToLower() ) )
            {
                releaseProgram = RockReleaseProgram.Alpha;
            }
            else if ( updateUrl.Contains( RockReleaseProgram.Beta.ToString().ToLower() ) )
            {
                releaseProgram = RockReleaseProgram.Beta;
            }

            return releaseProgram;
        }

        public RockRelease InstallVersion( Version targetVersion, Version installedVersion )
        {
            var releases = GetReleasesList( installedVersion );
            var targetRelease = releases.Where( r => r.SemanticVersion == targetVersion.ToString() ).FirstOrDefault();

            if ( targetRelease == null )
            {
                throw new Exception( $"Target Release ${targetRelease} was not found." );
            }

            var targetPackagePath = DownloadPackage( targetRelease );

            InstallPackage( targetPackagePath );

            return targetRelease;
        }

        private void InstallPackage( string targetPackagePath )
        {
            try
            {
                using ( ZipArchive packageZip = ZipFile.OpenRead( targetPackagePath ) )
                {
                    ProcessTransformFiles( packageZip );
                    ProcessContentFiles( packageZip );
                    ProcessDeleteFiles( packageZip );
                }
            }
            catch
            {
                RestoreOriginal();
                throw;
            }
        }

        private void RestoreOriginal()
        {
            var backupLocation = Path.Combine( _rootPath, "app_data\\rockbackup" );
            var filesToRestore = Directory.GetFiles( backupLocation, "*.*", SearchOption.AllDirectories );

            foreach ( var file in filesToRestore )
            {
                var originalPath = file.Replace( "app_data\\rockbackup", string.Empty );

                RenameActiveFile( originalPath );

                var backupDirectory = Path.GetDirectoryName( originalPath );
                if ( !Directory.Exists( backupDirectory ) )
                {
                    Directory.CreateDirectory( backupDirectory );
                }

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
                        var deleteItemFullPath = Path.Combine( _rootPath, deleteItem );
                        if ( deleteItem.StartsWith( rockWeb ) )
                        {
                            deleteItemFullPath = Path.Combine( _rootPath, deleteItem.Substring( rockWeb.Length ) );
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
                string fullpath = Path.Combine( _rootPath, entry.FullName.Replace( "content/", string.Empty ) );
                string directory = Path.GetDirectoryName( fullpath ).Replace( "content/", string.Empty );

                // if entry is a directory ignore it
                if ( entry.Length != 0 )
                {
                    BackupFile( fullpath );

                    if ( !Directory.Exists( directory ) )
                    {
                        Directory.CreateDirectory( directory );
                    }

                    RenameFile( fullpath );

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

            var localRockPackageDirectory = Path.Combine( _rootPath, LOCAL_ROCK_PACKAGE_FOLDER );

            if ( !Directory.Exists( localRockPackageDirectory ) )
            {
                Directory.CreateDirectory( localRockPackageDirectory );
            }

            var localRockPackagePath = Path.Combine( localRockPackageDirectory, $"{release.SemanticVersion}.rockpkg" );
            RemoveFileIfExists( localRockPackagePath );

            try
            {
                var wc = new WebClient();
                wc.DownloadFile( release.PackageUri, localRockPackagePath );
            }
            catch
            {
                RemoveFileIfExists( localRockPackagePath );
                throw;
            }

            return localRockPackagePath;
        }

        private void RemoveFileIfExists( string filepath )
        {
            if ( File.Exists( filepath ) )
            {
                // guard against things like file is temporarily locked, wait then try delete, etc.
                try
                {
                    File.Delete( filepath );
                }
                catch
                {
                    RenameActiveFile( filepath );
                }
            }
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
                string transformTargetFile = Path.Combine( _rootPath, filename.Substring( 0, filename.LastIndexOf( transformFileSuffix ) ) );

                BackupFile( transformTargetFile );

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
            var relativeTargetPath = filepathToBackup.Replace( _rootPath, string.Empty );
            var backupLocation = Path.Combine( _rootPath, "app_data\\rockbackup" );

            if ( !Directory.Exists( backupLocation ) )
            {
                Directory.CreateDirectory( backupLocation );
            }

            return Path.Combine( backupLocation, relativeTargetPath );
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

                RemoveFileIfExists( backupFilePath );
                File.Copy( filepathToBackup, backupFilePath );
            }
        }

        private string GetRenameFileName( string physicalFile )
        {
            var fileCount = 1;
            var fileToDelete = $"{physicalFile}.{fileCount}.rdelete";

            // generate a unique *.#.rdelete filename
            while ( File.Exists( fileToDelete ) )
            {
                fileCount++;
                fileToDelete = $"{physicalFile}.{fileCount}.rdelete";
            }

            return fileToDelete;
        }

        private void RenameFile( string filepathToRename )
        {
            bool dllFileNotInBin = filepathToRename.EndsWith( ".dll" ) && !filepathToRename.Contains( @"\bin\" );
            bool roslynAssembly = ( filepathToRename.EndsWith( ".dll" ) || filepathToRename.EndsWith( ".exe" ) ) && filepathToRename.Contains( @"\roslyn\" );

            // If this a roslyn assembly or a dll file from the Content files, rename it so that we don't have problems with it being locks
            if ( roslynAssembly || dllFileNotInBin )
            {
                string physicalFile;
                if ( roslynAssembly )
                {
                    physicalFile = filepathToRename;
                }
                else
                {
                    physicalFile = Path.Combine( _rootPath, filepathToRename );
                }

                RenameActiveFile( physicalFile );
            }
        }

        private void RenameActiveFile( string physicalFile )
        {
            if ( File.Exists( physicalFile ) )
            {
                File.Move( physicalFile, GetRenameFileName( physicalFile ) );
            }
        }
    }
}
