using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Hosting;
using Rock.Model;

namespace Rock.Utility
{
    /// <summary>
    /// Helper methods for accessing the file system.
    /// </summary>
    public static class FileManagementHelper
    {
        /// <summary>
        /// The root physical file system path of the web application.
        /// </summary>
        public static readonly string ROOT_PATH = HostingEnvironment.MapPath( "~/" );
        private static readonly string DELETE_FILE_EXTENSION = "rdelete";

        public static void TryDelete(string filePath, bool shouldBubbleException )
        {
            TryDelete( filePath, ( ex ) => ExceptionLogService.LogException( ex ), shouldBubbleException );
        }

        public static void TryDelete( string filePath, Action<Exception> catchMethod, bool shouldBubbleException )
        {
            try
            {
                File.Delete( filePath );
            }
            catch ( Exception ex )
            {
                catchMethod(ex);
                if ( shouldBubbleException )
                {
                    throw;
                }
            }
        }

        public static void DeleteOrRename( string filepath )
        {
            if ( File.Exists( filepath ) )
            {
                TryDelete( filepath, ( ex ) => RenameFile( filepath ), false );
            }
        }

        public static void RenameFile( string physicalFile )
        {
            if ( File.Exists( physicalFile ) )
            {
                File.Move( physicalFile, GetRenameFileName( physicalFile ) );
            }
        }

        public static void RenameActiveFile( string filepathToRename )
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
                    physicalFile = Path.Combine( ROOT_PATH, filepathToRename );
                }

                RenameFile( physicalFile );
            }
        }

        public static void CleanUpDeletedFiles()
        {
            var filesToDelete = Directory.GetFiles( ROOT_PATH, $"*.{DELETE_FILE_EXTENSION}", SearchOption.AllDirectories );
            foreach ( var file in filesToDelete )
            {
                FileManagementHelper.TryDelete( file, false );
            }
        }

        private static string GetRenameFileName( string physicalFile )
        {
            var fileCount = 1;
            var fileToDelete = $"{physicalFile}.{fileCount}.{DELETE_FILE_EXTENSION}";

            // generate a unique *.#.rdelete filename
            while ( File.Exists( fileToDelete ) )
            {
                fileCount++;
                fileToDelete = $"{physicalFile}.{fileCount}.{DELETE_FILE_EXTENSION}";
            }

            return fileToDelete;
        }
    }
}
