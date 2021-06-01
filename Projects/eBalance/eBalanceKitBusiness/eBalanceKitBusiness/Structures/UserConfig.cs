/**************************************************************************************************************
 * author               date            comment
 * ------------------------------------------------------------------------------------------------------------
 * Mirko Dibbert        2011-04-01      initial implementation
 *************************************************************************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Xml;
using eBalanceKitBusiness.Manager;

namespace eBalanceKitBusiness.Structures {
    
    public static class UserConfig {

        /// <summary>
        /// Initializes the <see cref="UserConfig"/> class.
        /// </summary>
        static UserConfig() {
            ConfigPath = System.Environment.GetFolderPath(
                System.Environment.SpecialFolder.ApplicationData) + "\\AvenDATA\\eBalanceKit";
            EnvironmentStates = new EnvironmentStateList();
        }

        /// <summary>
        /// Gets or sets the config path.
        /// </summary>
        /// <value>The config path.</value>
        private static string ConfigPath { get; set; }
        
        /// <summary>
        /// Name of the config file.
        /// </summary>
        private static string ConfigFile {
            get { return "eBalanceKit.cfg"; }
        }

        /// <summary>
        /// Gets or sets the last user.
        /// </summary>
        /// <value>The last user.</value>
        public static string LastUser { get; set; }

        /// <summary>
        /// Gets or sets the last language.
        /// </summary>
        /// <value>The last language.</value>
        public static string LastLanguage { get; set; }

        /// <summary>
        /// Gets or sets the passwordhash.
        /// </summary>
        /// <value>The Hash.</value>
        public static string PasswordHash { get; set; }

        /// <summary>
        /// Gets or sets the EnvironmentStates.
        /// </summary>
        /// <value>The environment states.</value>
        public static EnvironmentStateList EnvironmentStates { get; set; }

        /// <summary>
        /// Gets the EvironmentState of the current user if exists, otherwise null
        /// </summary>
        public static EnvironmentState UserEnvironmentState { get { return EnvironmentStates.FirstOrDefault(e => e.UserId == UserManager.Instance.CurrentUser.Id); } }

        /// <summary>
        /// Inits this instance.
        /// </summary>
        public static void Init() {

            if (!File.Exists(ConfigPath + "\\" + ConfigFile)) return;
            
            EnvironmentStates = new EnvironmentStateList();

            StreamReader reader = null;
            try {
                reader = new StreamReader(ConfigPath + "\\" + ConfigFile);
                while (!reader.EndOfStream) {
                    string line = reader.ReadLine().Trim();
                    if (line.Length == 0) continue;
                    if (!line.Contains("=")) continue;

                    string[] parts = new string[2];
                    parts[0] = line.Substring(0, line.IndexOf('='));
                    parts[1] = line.Substring(line.IndexOf('=') + 1, line.Length - line.IndexOf('=') - 1);
                    switch (parts[0].Trim().ToLower()) {

                        case "lastuser":
                            LastUser = parts[1].Trim();
                            break;

                        case "lastlanguage":
                            LastLanguage = parts[1].Trim();
                            break;

                        case "phash":
                            PasswordHash = parts[1].Trim();
                            break;

                        case "environment":
                            using (StringReader strigReader = new StringReader(parts[1].Trim())) {
                                using (XmlTextReader xmlReader = new XmlTextReader(strigReader)) {
                                    EnvironmentStates.ReadXml(xmlReader);
                                }
                            }
                            break;
                    }
                }
            } catch (Exception ex) {
                throw new Exception("User config could not be read: " + Environment.NewLine + ex.Message);
            
            } finally {
                if (reader != null) reader.Close();
            }
        }
        
        /// <summary>
        /// Saves this instance to the config file.
        /// </summary>
        public static void Save() {

            // create config directory
            try {
                if (!Directory.Exists(ConfigPath)) Directory.CreateDirectory(ConfigPath);
            } catch (Exception ex) {
                throw new Exception("Config directory '" + ConfigPath + "' could not be created: " + Environment.NewLine + ex.Message);
            }

            StreamWriter writer = null;
            try {
                writer = new StreamWriter(ConfigPath + "\\" + ConfigFile);
                writer.WriteLine("lastuser=" + LastUser);
                if (PasswordHash != null) {
                    writer.WriteLine("phash=" + PasswordHash);
                }
                if (LastLanguage != null) {
                    writer.WriteLine("lastlanguage=" + LastLanguage);
                }
                if (EnvironmentStates != null) {
                    string environment = string.Empty;
                    using (StringWriter str = new StringWriter()) {
                        using (XmlTextWriter xml = new XmlTextWriter(str)) {
                            EnvironmentStates.WriteXml(xml);
                            environment = str.ToString();
                        }
                    }
                    writer.WriteLine("environment=" + environment);
                }

            } catch (Exception ex) {
                throw new Exception("User config could not be saved: " + Environment.NewLine + ex.Message);
            
            } finally {
                if (writer != null) writer.Close();
            }
        }

    }
}
