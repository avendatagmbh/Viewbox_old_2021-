using System;
using Base.Localisation;

namespace ViewAssistantBusiness.Config
{
    public class ConfigDbInitializationException : Exception
    {
        public ConfigDbInitializationException(string detailMessage, Exception ex) :
            base(string.Format(ExceptionMessages.ConfigDbInitializationException, detailMessage), ex) { }
    }
}
