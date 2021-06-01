using System;
using System.Configuration;
using System.Reflection;

namespace ViewAssistant
{
    public class ApplicationConfigSection : ConfigurationSection
    {
        [ConfigurationProperty("VersionString")]
        public String VersionString
        {
            get
            {
                return string.Format(
                    Base.Localisation.ResourcesCommon.Version,
                    Assembly.GetEntryAssembly().GetName().Version.Major,
                    Assembly.GetEntryAssembly().GetName().Version.Minor,
                    Assembly.GetEntryAssembly().GetName().Version.Build,
                    Assembly.GetEntryAssembly().GetName().Version.Revision
                    );
            }
        }

        [ConfigurationProperty("ProgramArchitecture")]
        public String ProgramArchitecture
        {
            get
            {
                return string.Format(Base.Localisation.ResourcesCommon.ApplicationArchitecture, IntPtr.Size * 8);
            }
        }
    }
}
