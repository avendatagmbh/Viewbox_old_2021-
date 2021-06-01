/**************************************************************************************************************
 * author               date            comment
 * ------------------------------------------------------------------------------------------------------------
 * Attila Papp        2012-09-03      reuse of the legacy code from Mirko. Not tested.
 *************************************************************************************************************/
using System;
using System.Text;
using System.Xml;
using ViewboxAdmin_ViewModel.Structures.Config;

namespace ViewboxAdminBusiness.Manager
{
    public class XmlConfigManager : IXmlConfigManager
    {
        /// <summary>
        /// load / save the application configuration  info to file
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public IApplicationConfig LoadApplicationConfigurationDataFromXml(string path) {
            IApplicationConfig appconfig = new ApplicationConfig(new FileManager());
            XmlDocument doc = new XmlDocument();
            doc.Load(path);
            XmlNode root = doc.DocumentElement;
            foreach (XmlNode node in root.ChildNodes)
            {
                switch (node.Name)
                {
                    case "LastUser":
                        appconfig.LastUser = node.InnerText.Trim();
                        break;

                    case "LastProfile":
                        appconfig.LastProfile = node.InnerText;
                        break;

                    case "ConfigLocationType":
                        appconfig.ConfigLocationType = (ConfigLocation)Enum.Parse(typeof(ConfigLocation), node.InnerText.Trim());
                        break;

                    case "ConfigDirectory":
                        appconfig.ConfigDirectory = node.InnerText.Trim();
                        break;

                }
            }
            return appconfig;

        }

        public void SaveApplicationConfigurationDataToXml(IApplicationConfig appconfig, string path) {
            XmlTextWriter writer = null;
            try
            {

                // save xml file
                writer = new XmlTextWriter(path, Encoding.UTF8);
                writer.Formatting = Formatting.Indented;
                writer.Indentation = 4;

                writer.WriteStartDocument();

                writer.WriteStartElement("ViewboxAdminConfig");

                writer.WriteElementString("LastUser", appconfig.LastUser);
                writer.WriteElementString("LastProfile", appconfig.LastProfile);
                writer.WriteElementString("ConfigLocationType", Enum.GetName(typeof(ConfigLocation), appconfig.ConfigLocationType));
                writer.WriteElementString("ConfigDirectory", appconfig.ConfigDirectory);

                writer.WriteEndElement();

                writer.WriteEndDocument();
                writer.Close();

            }
            finally
            {
                if (writer != null) writer.Close();
            }
            
        }
    }
}
