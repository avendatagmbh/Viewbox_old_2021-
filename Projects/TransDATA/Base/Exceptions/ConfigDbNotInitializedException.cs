using System;
using Base.Localisation;

namespace Base.Exceptions {
    /// <summary>
    /// This exception is thrown, if an uninitialized configuration database has been accessed.
    /// </summary>
    /// <author>Mirko Dibbert</author>
    /// <since>2011-08-27</since>
    public class ConfigDbNotInitializedException : Exception {
        public ConfigDbNotInitializedException() : base(ExceptionMessages.ConfigDbNotInitializedException) {}
    }
}