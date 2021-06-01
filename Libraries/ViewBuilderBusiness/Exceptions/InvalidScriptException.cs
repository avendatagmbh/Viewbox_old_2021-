using System;

namespace ViewBuilderBusiness.Exceptions
{
    /// <summary>
    ///   This exception is shown on viescript parsing errors.
    /// </summary>
    public class InvalidScriptException : Exception
    {
        /// <summary>
        ///   Initializes a new instance of the <see cref="InvalidScriptException" /> class.
        /// </summary>
        /// <param name="message"> The message. </param>
        public InvalidScriptException(string message) : base(message)
        {
        }
    }
}