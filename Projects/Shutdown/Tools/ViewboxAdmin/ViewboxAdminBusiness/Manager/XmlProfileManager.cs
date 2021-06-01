/**************************************************************************************************************
 * author               date            comment
 * ------------------------------------------------------------------------------------------------------------
 * Attila Papp        2012-09-04        Legacy code from Mirko, I hide it behind an interface, not unit tested
 *************************************************************************************************************/
using System;
using System.Text;
using System.Xml;
using DbAccess.Structures;
using ViewboxAdmin_ViewModel.Structures.Config;

namespace ViewboxAdminBusiness.Manager
{
    public class XmlProfileManager : IXmlProfileManager
    {
        public void LoadProfileFromFile(ref IProfile profile, string file) {
            XmlDocument doc = new XmlDocument();
            doc.Load(file);

            XmlNode root = doc.DocumentElement;

            foreach (XmlNode node in root.ChildNodes)
            {
                switch (node.Name)
                {
                    case "Name":
                        profile.Name = node.InnerText.Trim();
                        break;
                    case "Description":
                        profile.Description = node.InnerText.Trim();
                        break;
                    case "ViewboxDbConfig":
                        profile.DbConfig = ReadDbConfig(node.Attributes);
                        break;
                }
            }

        }

        private DbConfig ReadDbConfig(XmlAttributeCollection xmlAttributeCollection) {
            DbConfig dbConfig = new DbConfig(xmlAttributeCollection["type"].Value);
            foreach (XmlAttribute attr in xmlAttributeCollection)
            {
                switch (attr.Name)
                {
                    case "host":
                        dbConfig.Hostname = attr.Value;
                        break;

                    case "user":
                        dbConfig.Username = attr.Value;
                        break;

                    case "port":
                        dbConfig.Port = Convert.ToInt32(attr.Value);
                        break;

                    case "password":
                        dbConfig.Password = attr.Value;
                        break;

                    case "database":
                        dbConfig.DbName = attr.Value;
                        break;
                }
            }
            return dbConfig;
        }

        public void SaveProfileToXml(ViewboxAdmin_ViewModel.Structures.Config.IProfile profile, string path) {
            XmlTextWriter writer = new XmlTextWriter(path, Encoding.UTF8);
            writer.Formatting = Formatting.Indented;
            writer.Indentation = 4;

            writer.WriteStartDocument();

            writer.WriteStartElement("ViewboxAdminConfig");

            writer.WriteElementString("Name", profile.Name);
            writer.WriteElementString("Description", profile.Description);

            // write database config
            WriteDbConfig("ViewboxDbConfig", profile.DbConfig, writer);


            writer.WriteEndElement();
            writer.WriteEndDocument();
            writer.Close();
        }

        private void WriteDbConfig(string elemName, IDbConfig config, XmlWriter writer) {
            if (config != null)
            {
                writer.WriteStartElement(elemName);
                writer.WriteAttributeString("type", config.DbType);
                writer.WriteAttributeString("host", config.Hostname);
                writer.WriteAttributeString("user", config.Username);
                writer.WriteAttributeString("password", config.Password);
                writer.WriteAttributeString("port", config.Port.ToString());
                writer.WriteAttributeString("database", config.DbName);
                writer.WriteEndElement();
            }
        }
    }
}
