using System;
using System.IO;
using System.Text;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Primitives;

namespace Rock.Lava
{
    /// <summary>
    /// An implementation of a Lava File System for the Fluid framework.
    /// </summary>
    public class FluidFileSystem : IFileProvider, ILavaFileSystem
    {
        private ILavaFileSystem _fileSystem = null;

        public FluidFileSystem( ILavaFileSystem fileSystem )
        {
            _fileSystem = fileSystem;
        }

        public bool FileExists( string filePath )
        {
            return _fileSystem.FileExists( filePath );
        }

        public IDirectoryContents GetDirectoryContents( string subpath )
        {
            // Directory listing is not supported.
            return null;

        }

        public IFileInfo GetFileInfo( string subpath )
        {
            // The Fluid framework appends a ".liquid" extension to the file path if it does not exist.
            // If the file cannot be found, remove the appended extension and try again.
            bool exists = false;

            if ( subpath.EndsWith(".liquid") )
            {
                exists = _fileSystem.FileExists( subpath );

                if ( !exists )
                {
                    subpath = subpath.Substring( 0, subpath.Length - 7 );

                    exists = _fileSystem.FileExists( subpath );
                }
            }

            var text = exists ? _fileSystem.ReadTemplateFile( null, subpath ) : string.Empty;

            var fileInfo = new LavaFileInfo( subpath, text, exists );

            if ( !exists )
            {
                throw new LavaException( "File Load Failed. File \"{0}\" could not be accessed.", subpath );
            }

            return fileInfo;
        }

        public string ReadTemplateFile( ILavaContext context, string templateName )
        {
            throw new NotImplementedException();
        }

        public IChangeToken Watch( string filter )
        {
            // File system monitoring is not supported.
            return null;
        }
    }

    public class LavaFileInfo : IFileInfo
    {
        public LavaFileInfo( string name, string content, bool exists = true )
        {
            Name = name;
            Content = content;
            Exists = exists;
        }

        public string Content { get; set; }
        public bool Exists { get; }

        public bool IsDirectory => false;

        public DateTimeOffset LastModified => DateTimeOffset.MinValue;

        public long Length => -1;

        public string Name { get; }

        public string PhysicalPath => null;

        public Stream CreateReadStream()
        {
            var data = Encoding.UTF8.GetBytes( Content );
            return new MemoryStream( data );
        }
    }

}