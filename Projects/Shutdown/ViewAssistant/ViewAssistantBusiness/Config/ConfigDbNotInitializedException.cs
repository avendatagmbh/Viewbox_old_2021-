using System;
using Base.Localisation;

namespace ViewAssistantBusiness.Config
{
    public class ConfigDbNotInitializedException : Exception
    {
        public ConfigDbNotInitializedException() : base(ExceptionMessages.ConfigDbNotInitializedException) { }
    }
}
