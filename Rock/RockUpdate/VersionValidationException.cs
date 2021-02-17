using System;

namespace Rock.RockUpdate
{
    /// <summary>
    /// 
    /// </summary>
    /// <seealso cref="System.Exception" />
    public class VersionValidationException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="VersionValidationException"/> class.
        /// </summary>
        /// <param name="message">The message that describes the error.</param>
        public VersionValidationException( string message ) : base( message ) { }
    }
}
