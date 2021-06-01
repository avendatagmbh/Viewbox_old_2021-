using System;
using System.Configuration;
using System.IO;

namespace Utils.Password
{
    public class PasswordConfiguration : ConfigurationSection
    {
        #region Class members

        /// <summary>
        ///   Returns the password policy found in password.config if enablePolicy is set to true.
        /// </summary>
        /// <returns> </returns>
        public static PasswordPolicy GetPasswordPolicy()
        {
            var configPath = AppDomain.CurrentDomain.BaseDirectory + "password.config";
            if (File.Exists(configPath))
            {
                var configFileMap = new ExeConfigurationFileMap {ExeConfigFilename = configPath};

                // Get the mapped configuration file.
                var passwordConfig = ConfigurationManager.OpenMappedExeConfiguration(configFileMap,
                                                                                     ConfigurationUserLevel.None);
                var pwdSection = passwordConfig.GetSection("passwordSection") as PasswordConfiguration;

                if (pwdSection != null && pwdSection.PasswordPolicy != null && pwdSection.PasswordPolicy.EnablePolicy)
                    return pwdSection.PasswordPolicy;
            }
            return null;
        }

        #endregion

        [ConfigurationProperty("passwordPolicy", IsRequired = false, IsKey = false, IsDefaultCollection = true)]
        public PasswordPolicy PasswordPolicy
        {
            get { return ((PasswordPolicy) (base["passwordPolicy"])); }
            set { base["passwordPolicy"] = value; }
        }
    }

    public class PasswordPolicy : ConfigurationElement
    {
        public bool HistoryEnabled
        {
            get { return PasswordHistoryCount > 0; }
        }

        [ConfigurationProperty("passwordHistoryCount")]
        public int PasswordHistoryCount
        {
            get
            {
                int result = 0;
                int.TryParse(base["passwordHistoryCount"].ToString(), out result);
                return result;
            }
            set { base["passwordHistoryCount"] = value; }
        }

        /// <summary>
        ///   the time in days when the password is valid, 0 means always valid
        /// </summary>
        [ConfigurationProperty("validityPeriod")]
        public int ValidityPeriod
        {
            get
            {
                int result = 0;
                int.TryParse(base["validityPeriod"].ToString(), out result);
                return result;
            }
            set { base["validityPeriod"] = value; }
        }

        [ConfigurationProperty("passwordPattern")]
        public string PasswordPattern
        {
            get { return base["passwordPattern"].ToString(); }
            set { base["passwordPattern"] = value; }
        }

        [ConfigurationProperty("passwordDescription_EN")]
        public string PasswordDescription_EN
        {
            get { return base["passwordDescription_EN"].ToString(); }
            set { base["passwordDescription_EN"] = value; }
        }

        [ConfigurationProperty("passwordDescription_DE")]
        public string PasswordDescription_DE
        {
            get { return base["passwordDescription_DE"].ToString(); }
            set { base["passwordDescription_DE"] = value; }
        }

        [ConfigurationProperty("passwordDescription_FR")]
        public string PasswordDescription_FR
        {
            get { return base["passwordDescription_FR"].ToString(); }
            set { base["passwordDescription_FR"] = value; }
        }

        [ConfigurationProperty("passwordDescription_ES")]
        public string PasswordDescription_ES
        {
            get { return base["passwordDescription_ES"].ToString(); }
            set { base["passwordDescription_ES"] = value; }
        }

        [ConfigurationProperty("passwordDescription_IT")]
        public string PasswordDescription_IT
        {
            get { return base["passwordDescription_IT"].ToString(); }
            set { base["passwordDescription_IT"] = value; }
        }

        /// <summary>
        ///   If the policy is enabled or not.
        /// </summary>
        [ConfigurationProperty("enablePolicy")]
        public bool EnablePolicy
        {
            get
            {
                var result = false;
                bool.TryParse(base["enablePolicy"].ToString(), out result);
                return result;
            }
            set { base["enablePolicy"] = value; }
        }
    }
}