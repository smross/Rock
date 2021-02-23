using System;

namespace Rock.Update.Exceptions
{
    public class PackageNotFoundException : Exception
    {
        public PackageNotFoundException( string message ) : base( message ) { }
    }
}
