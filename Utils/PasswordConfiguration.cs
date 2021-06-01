using System;
using System.Configuration;
using System.IO;

namespace Utils
{
	public class PasswordConfiguration : ConfigurationSection
	{
		[ConfigurationProperty("passwordPolicy", IsRequired = false, IsKey = false, IsDefaultCollection = true)]
		public PasswordPolicy PasswordPolicy
		{
			get
			{
				return (PasswordPolicy)base["passwordPolicy"];
			}
			set
			{
				base["passwordPolicy"] = value;
			}
		}

		public static PasswordPolicy GetPasswordPolicy()
		{
			string configPath = AppDomain.CurrentDomain.BaseDirectory + "password.config";
			if (File.Exists(configPath))
			{
				PasswordConfiguration pwdSection = ConfigurationManager.OpenMappedExeConfiguration(new ExeConfigurationFileMap
				{
					ExeConfigFilename = configPath
				}, ConfigurationUserLevel.None).GetSection("passwordSection") as PasswordConfiguration;
				if (pwdSection != null && pwdSection.PasswordPolicy != null && pwdSection.PasswordPolicy.EnablePolicy)
				{
					return pwdSection.PasswordPolicy;
				}
			}
			return null;
		}
	}
}
