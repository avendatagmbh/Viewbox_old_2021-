/**************************************************************************************************************
 * author               date            comment
 * ------------------------------------------------------------------------------------------------------------
 * Mirko Dibbert        2010-10-16      initial implementation
 *************************************************************************************************************/

using System;
using System.IO;
using System.Text;
using System.Xml;
using ViewValidatorLogic.Config;

namespace ViewValidatorLogic.Manager {

    /// <summary>
    /// This class provides functionality to persist and restore the application state.
    /// </summary>
    public static class ApplicationManager {

        /// <summary>
        /// Initializes the <see cref="ApplicationManager"/> class.
        /// </summary>
        static ApplicationManager() {
            ApplicationConfig = new ApplicationConfig();
        }

        /*****************************************************************************************************/

        #region properties

        /// <summary>
        /// Gets or sets the application config.
        /// </summary>
        /// <value>The application config.</value>
        public static ApplicationConfig ApplicationConfig { get; private set; }

        /// <summary>
        /// Gets the config path.
        /// </summary>
        /// <value>The config path.</value>
        private static string ConfigDirectory{
            get {
                string path = System.Environment.GetFolderPath(System.Environment.SpecialFolder.ApplicationData) + "\\AvenDATA\\ViewValidator";
                if (!Directory.Exists(path)) Directory.CreateDirectory(path);
                return path;
            }
        }

        /// <summary>
        /// Gets the name of the config file.
        /// </summary>
        /// <value>The name of the config file.</value>
        private static string ConfigFileName {
            get { return "viewvalidator.xml"; }
        }

        #endregion properties

        /*****************************************************************************************************/

        #region methods

        /// <summary>
        /// Loads the application config.
        /// </summary>
        public static void Load() {

            if (!File.Exists(ConfigDirectory + "\\" + ConfigFileName)) return;

            XmlDocument doc = new XmlDocument();
            doc.Load(ConfigDirectory + "\\" + ConfigFileName);
            XmlNode root = doc.DocumentElement;
            foreach (XmlNode node in root.ChildNodes) {
                switch (node.Name) {
                    case "LastUser":
                        ApplicationConfig.LastUser = node.InnerText.Trim();
                        break;

                    case "LastProfile":
                        ApplicationConfig.LastProfile = node.InnerText;
                        break;
                    case "LastTableMapping":
                        ApplicationConfig.LastTableMapping = node.InnerText;
                        break;

                    case "ConfigLocation":
                        ApplicationConfig.ConfigLocation = (ConfigLocation)Enum.Parse(typeof(ConfigLocation), node.InnerText.Trim());
                        break;

                    case "ConfigDirectory":
                        ApplicationConfig.ConfigDirectory = node.InnerText.Trim();
                        break;

                    case "LastExcelFile":
                        ApplicationConfig.LastExcelFile = node.InnerText.Trim();
                        break;

                        //case "AutomaticLogin":
                        //    ApplicationConfig.AutomaticLogin = Convert.ToBoolean(node.InnerText);
                        //    break;
                }
            }
        }

        /// <summary>
        /// Saves the application config.
        /// </summary>
        public static void Save() {

            XmlTextWriter writer = null;
            try {
                
                // save xml file
                writer = new XmlTextWriter(ConfigDirectory + "\\" + ConfigFileName, Encoding.UTF8);
                writer.Formatting = Formatting.Indented;
                writer.Indentation = 4;

                writer.WriteStartDocument();

                writer.WriteStartElement("ViewValidatorConfig");

                writer.WriteElementString("LastUser", ApplicationConfig.LastUser);
                writer.WriteElementString("LastProfile", ApplicationConfig.LastProfile);
                writer.WriteElementString("LastTableMapping", ApplicationConfig.LastTableMapping);
                writer.WriteElementString("ConfigLocation", Enum.GetName(typeof(ConfigLocation), ApplicationConfig.ConfigLocation));                
                writer.WriteElementString("ConfigDirectory", ApplicationConfig.ConfigDirectory);
                writer.WriteElementString("LastExcelFile", ApplicationConfig.LastExcelFile);
                //writer.WriteElementString("AutomaticLogin", ApplicationConfig.AutomaticLogin.ToString());
                
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
