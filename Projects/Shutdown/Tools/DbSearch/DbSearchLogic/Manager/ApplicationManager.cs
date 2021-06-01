using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Text;
using System.Windows.Threading;
using System.Xml;
using DbSearchBase.Interfaces;
using DbSearchDatabase.Config;
using DbSearchDatabase.Interfaces;
using DbSearchLogic.Config;
using ApplicationConfig = DbSearchLogic.Config.ApplicationConfig;

namespace DbSearchLogic.Manager {
    public static class ApplicationManager {
        static ApplicationManager() {
            ApplicationConfig = new ApplicationConfig();
        }

        #region properties
        private static string _configPassword = "j54z8dj237S8357fOJse2093DZmXU31";

        public static IApplicationConfig ApplicationConfig { get; private set; }

        private static string ConfigDirectory {
            get {
                string path = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\AvenDATA\\DbSearch";
                if (!Directory.Exists(path)) Directory.CreateDirectory(path);
                return path;
            }
        }

        private static string ConfigFileName {
            get { return "DbSearch.xml"; }
        }

        #endregion properties

        #region methods

        /// <summary>
        /// Loads the application config.
        /// </summary>
        public static void Load(ObservableCollection<Profile> profiles, Dispatcher dispatcher) {

            if (!File.Exists(ConfigDirectory + "\\" + ConfigFileName)) return;
            profiles.Clear();
            string error = string.Empty;

            XmlDocument doc = new XmlDocument();
            doc.Load(ConfigDirectory + "\\" + ConfigFileName);
            XmlNode root = doc.DocumentElement;
            foreach (XmlNode node in root.ChildNodes) {
                switch (node.Name) {
                    case "Profile":
                        try {
                            Profile profile = LoadProfile(node);
                            ProfileManager.AddProfile(profile, true);
                        } catch (Exception ex) {
                            error += ex.Message + Environment.NewLine;
                        }
                        break;
                        //case "LastUser":
                        //    ApplicationConfig.LastUser = node.InnerText.Trim();
                        //    break;
                    case "LastQueryTable":
                        ApplicationConfig.LastQueryTable = node.InnerText;
                        break;

                    case "LastProfile":
                        ApplicationConfig.LastProfile = node.InnerText;
                        break;

                        //case "LastTableMapping":
                        //    ApplicationConfig.LastTableMapping = node.InnerText;
                        //    break;

                        //case "ConfigLocation":
                        //    ApplicationConfig.ConfigLocation = (ConfigLocation)Enum.Parse(typeof(ConfigLocation), node.InnerText.Trim());
                        //    break;

                        //case "ConfigDirectory":
                        //    ApplicationConfig.ConfigDirectory = node.InnerText.Trim();
                        //    break;

                        //case "LastExcelFile":
                        //    ApplicationConfig.LastExcelFile = node.InnerText.Trim();
                        //    break;

                }
            }
            if(!string.IsNullOrEmpty(error)) throw new Exception(error);
        }

        private static Profile LoadProfile(XmlNode parentNode) {
            Profile profile = ProfileManager.CreateNewProfile();
            if (parentNode.Attributes != null) {
                profile.Name = parentNode.Attributes["Name"].InnerText;
                if (parentNode.Attributes["DbHostname"] != null)
                    profile.DbConfigView.Hostname = parentNode.Attributes["DbHostname"].InnerText;
                if (parentNode.Attributes["DbUsername"] != null)
                    profile.DbConfigView.Username = parentNode.Attributes["DbUsername"].InnerText;
                if (parentNode.Attributes["DbPassword"] != null)
                    profile.DbConfigView.Password = Utils.StringUtils.DecryptString(parentNode.Attributes["DbPassword"].InnerText, _configPassword); ;
                if (parentNode.Attributes["DbDatabase"] != null)
                    profile.DbConfigView.DbName = parentNode.Attributes["DbDatabase"].InnerText;
                if (parentNode.Attributes["DbPort"] != null) {
                    int port = 3306;
                    if (!String.IsNullOrEmpty(parentNode.Attributes["DbPort"].InnerText)) {
                        port = Convert.ToInt32(parentNode.Attributes["DbPort"].InnerText);
                        profile.DbConfigView.Port = port;
                    }
                    
                }
                    

            }
            return profile;
        }

        public static void Save() {

            XmlTextWriter writer = null;
            try {

                // save xml file
                writer = new XmlTextWriter(ConfigDirectory + "\\" + ConfigFileName, Encoding.UTF8);
                writer.Formatting = Formatting.Indented;
                writer.Indentation = 4;

                writer.WriteStartDocument();

                writer.WriteStartElement("DbSearchConfig");

                foreach (var profile in ProfileManager.Profiles) {
                    writer.WriteStartElement("Profile");
                    writer.WriteAttributeString("Name", profile.Name);
                    writer.WriteAttributeString("DbHostname", profile.DbConfigView.Hostname);
                    writer.WriteAttributeString("DbUsername", profile.DbConfigView.Username);
                    writer.WriteAttributeString("DbPassword", Utils.StringUtils.EncryptString(profile.DbConfigView.Password, _configPassword)); ;
                    writer.WriteAttributeString("DbDatabase", profile.DbConfigView.DbName);
                    writer.WriteAttributeString("DbPort", profile.DbConfigView.Port.ToString());
                    //writer.WriteAttributeString("Path", profile.Path);
                    writer.WriteEndElement();
                }
                //writer.WriteElementString("LastUser", ApplicationConfig.LastUser);
                writer.WriteElementString("LastProfile", ApplicationConfig.LastProfile);
                writer.WriteElementString("LastQueryTable", ApplicationConfig.LastQueryTable);
                //writer.WriteElementString("LastTableMapping", ApplicationConfig.LastTableMapping);
                //writer.WriteElementString("ConfigLocation", Enum.GetName(typeof(ConfigLocation), ApplicationConfig.ConfigLocation));                
                //writer.WriteElementString("ConfigDirectory", ApplicationConfig.ConfigDirectory);
                //writer.WriteElementString("LastExcelFile", ApplicationConfig.LastExcelFile);
                ////writer.WriteElementString("AutomaticLogin", ApplicationConfig.AutomaticLogin.ToString());

                writer.WriteEndElement();

                writer.WriteEndDocument();
                writer.Close();

            } finally {
                if (writer != null) writer.Close();
            }
        }

        #endregion methods
    }
}
