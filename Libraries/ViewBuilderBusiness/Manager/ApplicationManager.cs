using System;
using System.IO;
using System.Text;
using System.Xml;
using Utils;
using ViewBuilderBusiness.Structures.Config;

namespace ViewBuilderBusiness.Manager
{
    /// <summary>
    ///   This class provides functionality to persist and restore the application state.
    /// </summary>
    public static class ApplicationManager
    {
        private const string PASSWORD = "A527V%enEpD&237aT734a#";

        /// <summary>
        ///   Initializes the <see cref="ApplicationManager" /> class.
        /// </summary>
        static ApplicationManager()
        {
            ApplicationConfig = new ApplicationConfig();
        }

        #region properties

        /// <summary>
        ///   Gets or sets the application config.
        /// </summary>
        /// <value> The application config. </value>
        public static ApplicationConfig ApplicationConfig { get; private set; }

        /// <summary>
        ///   Gets the config path.
        /// </summary>
        /// <value> The config path. </value>
        private static string ConfigDirectory
        {
            get
            {
                string path = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) +
                              "\\AvenDATA\\ViewBuilder";
                if (!Directory.Exists(path)) Directory.CreateDirectory(path);
                return path;
            }
        }

        /// <summary>
        ///   Gets the name of the config file.
        /// </summary>
        /// <value> The name of the config file. </value>
        private static string ConfigFileName
        {
            get { return "viewbuilder.xml"; }
        }

        #endregion properties

        #region methods

        /// <summary>
        ///   Loads the application config.
        /// </summary>
        public static void Load()
        {
            if (!File.Exists(ConfigDirectory + "\\" + ConfigFileName)) return;
            XmlDocument doc = new XmlDocument();
            doc.Load(ConfigDirectory + "\\" + ConfigFileName);
            XmlNode root = doc.DocumentElement;
            foreach (XmlNode node in root.ChildNodes)
            {
                switch (node.Name)
                {
                    case "LastUser":
                        ApplicationConfig.LastUser = node.InnerText.Trim();
                        break;
                    case "LastProfile":
                        ApplicationConfig.LastProfile = node.InnerText.Trim();
                        break;
                    case "ConfigLocation":
                        ApplicationConfig.ConfigLocation =
                            (ConfigLocation) Enum.Parse(typeof (ConfigLocation), node.InnerText.Trim());
                        break;
                    case "ConfigDirectory":
                        ApplicationConfig.ConfigDirectory = node.InnerText.Trim();
                        break;
                    case "ConfigDbConfig":
                        foreach (XmlAttribute attr in node.Attributes)
                        {
                            switch (attr.Name)
                            {
                                case "type":
                                    ApplicationConfig.ConfigDbConfig.DbType = attr.Value;
                                    break;
                                case "host":
                                    ApplicationConfig.ConfigDbConfig.Hostname = attr.Value;
                                    break;
                                case "user":
                                    ApplicationConfig.ConfigDbConfig.Username = attr.Value;
                                    break;
                                case "password":
                                    ApplicationConfig.ConfigDbConfig.Password = StringUtils.DecryptString(
                                        attr.Value,
                                        ApplicationConfig.ConfigDbConfig.Username + "@" +
                                        ApplicationConfig.ConfigDbConfig.Hostname,
                                        PASSWORD);
                                    break;
                                case "port":
                                    {
                                        int port;
                                        int.TryParse(attr.Value, out port);
                                        ApplicationConfig.ConfigDbConfig.Port = port;
                                    }
                                    break;
                                case "database":
                                    ApplicationConfig.ConfigDbConfig.DbName = attr.Value;
                                    break;
                            }
                        }
                        break;
                    case "ConfigSmtpServer":
                        foreach (XmlAttribute attr in node.Attributes)
                        {
                            switch (attr.Name)
                            {
                                case "sender":
                                    ApplicationConfig.SmtpServer.Sender = attr.Value;
                                    break;
                                case "user":
                                    ApplicationConfig.SmtpServer.User = attr.Value;
                                    break;
                                case "password":
                                    ApplicationConfig.SmtpServer.Password = StringUtils.DecryptString(
                                        attr.Value,
                                        ApplicationConfig.SmtpServer.User + "@" + ApplicationConfig.SmtpServer.Server,
                                        PASSWORD);
                                    break;
                                case "server":
                                    ApplicationConfig.SmtpServer.Server = attr.Value;
                                    break;
                                case "port":
                                    {
                                        int port;
                                        int.TryParse(attr.Value, out port);
                                        ApplicationConfig.SmtpServer.Port = port;
                                    }
                                    break;
                            }
                        }
                        break;
                }
            }
        }

        /// <summary>
        ///   Saves the application config.
        /// </summary>
        public static void Save()
        {
            XmlTextWriter writer = null;
            try
            {
                // save xml file
                writer = new XmlTextWriter(ConfigDirectory + "\\" + ConfigFileName, Encoding.UTF8);
                writer.Formatting = Formatting.Indented;
                writer.Indentation = 4;
                writer.WriteStartDocument();
                writer.WriteStartElement("ViewBuilderConfig");
                writer.WriteElementString("LastUser", ApplicationConfig.LastUser);
                writer.WriteElementString("LastProfile", ApplicationConfig.LastProfile);
                writer.WriteElementString("ConfigLocation",
                                          Enum.GetName(typeof (ConfigLocation), ApplicationConfig.ConfigLocation));
                writer.WriteElementString("ConfigDirectory", ApplicationConfig.ConfigDirectory);

                writer.WriteStartElement("ConfigDbConfig");
                writer.WriteAttributeString("type", ApplicationConfig.ConfigDbConfig.DbType);
                writer.WriteAttributeString("host", ApplicationConfig.ConfigDbConfig.Hostname);
                writer.WriteAttributeString("user", ApplicationConfig.ConfigDbConfig.Username);

                writer.WriteAttributeString("password", StringUtils.EncryptString(
                    ApplicationConfig.ConfigDbConfig.Password,
                    ApplicationConfig.ConfigDbConfig.Username + "@" + ApplicationConfig.ConfigDbConfig.Hostname,
                    PASSWORD));

                writer.WriteAttributeString("port", ApplicationConfig.ConfigDbConfig.Port.ToString());
                writer.WriteAttributeString("database", ApplicationConfig.ConfigDbConfig.DbName);
                writer.WriteEndElement();
                writer.WriteStartElement("ConfigSmtpServer");
                writer.WriteAttributeString("sender", ApplicationConfig.SmtpServer.Sender);
                writer.WriteAttributeString("user", ApplicationConfig.SmtpServer.User);
                writer.WriteAttributeString("password", StringUtils.EncryptString(
                    ApplicationConfig.SmtpServer.Password,
                    ApplicationConfig.SmtpServer.User + "@" + ApplicationConfig.SmtpServer.Server,
                    PASSWORD));
                writer.WriteAttributeString("server", ApplicationConfig.SmtpServer.Server);
                writer.WriteAttributeString("port", ApplicationConfig.SmtpServer.Port.ToString());
                writer.WriteEndElement();
                writer.WriteEndElement();
                writer.WriteEndDocument();
                writer.Close();
            }
            finally
            {
                if (writer != null) writer.Close();
            }
        }

        #endregion methods

        /*****************************************************************************************************/

        /*****************************************************************************************************/
    }
}