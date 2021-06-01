using System;
using Base.Localisation;

namespace Base.Exceptions {
    /// <summary>
    /// This exception is thrown, if an error occured during the config database initialization process.
    /// </summary>
    /// <author>Mirko Dibbert</author>
    /// <since>2011-08-27</since>
    public class ConfigDbInitializationException : Exception {
        public ConfigDbInitializationException(string detailMessage, Exception ex) :
            base(string.Format(ExceptionMessages.ConfigDbInitializationException, detailMessage), ex) {}
    }
}