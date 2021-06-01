using System;
using Base.Localisation;

namespace Base.Exceptions {
    /// <summary>
    /// This exception is thrown, if an error occured during the config database shutdown process.
    /// </summary>
    /// <author>Mirko Dibbert</author>
    /// <since>2011-08-27</since>
    public class ConfigDbShutdownException : Exception {
        public ConfigDbShutdownException(string detailMessage, Exception ex) :
            base(string.Format(ExceptionMessages.ConfigDbShutdownException, detailMessage), ex) {}
    }
}