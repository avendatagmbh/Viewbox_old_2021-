using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Text;
using System.Windows.Threading;
using System.Xml;
using ScreenshotAnalyzerBusiness.Structures.Config;

namespace ScreenshotAnalyzerBusiness.Manager {
    public static class ApplicationManager {
        static ApplicationManager() {
            ApplicationConfig = new ApplicationConfig();
        }

        #region properties
        private static string _configPassword = "j54z8dj237S8357fOJse2093DZmXU31";

        public static IApplicationConfig ApplicationConfig { get; private set; }

        public static string ConfigDirectory {
            get {
                string path = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\AvenDATA\\ScreenshotAnalyzer";
                if (!Directory.Exists(path)) Directory.CreateDirectory(path);
                return path;
            }
        }

        private static string ConfigFileName {
            get { return "ScreenshotAnalyzer.xml"; }
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

                    case "LastProfile":
                        ApplicationConfig.LastProfile = node.InnerText;
                        break;
                }
            }
            if(!string.IsNullOrEmpty(error)) throw new Exception(error);
        }

        private static Profile LoadProfile(XmlNode parentNode) {
            Profile profile = ProfileManager.CreateNewProfile(parentNode.Attributes["Name"].InnerText);
            //if (parentNode.Attributes != null) {
            //    profile.Name = parentNode.Attributes["Name"].InnerText;
            //}
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

                writer.WriteStartElement("ScreenshotAnalyzer");

                foreach (var profile in ProfileManager.Profiles) {
                    writer.WriteStartElement("Profile");
                    writer.WriteAttributeString("Name", profile.Name);
                    writer.WriteAttributeString("DbHostname", profile.DbConfig.Hostname);
                    writer.WriteAttributeString("DbUsername", profile.DbConfig.Username);
                    writer.WriteAttributeString("DbPassword", Utils.StringUtils.EncryptString(profile.DbConfig.Password, _configPassword)); ;
                    writer.WriteAttributeString("DbDatabase", profile.DbConfig.DbName);
                    //writer.WriteAttributeString("Path", profile.Path);
                    writer.WriteEndElement();
                }
                //writer.WriteElementString("LastUser", ApplicationConfig.LastUser);
                writer.WriteElementString("LastProfile", ApplicationConfig.LastProfile);

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
