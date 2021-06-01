using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Utils;
using Config.Interfaces.Config;
using System.Xml;

namespace Config.Config {
    public class SqlExportConfig : ConfigBase, ISqlExportConfig {


        internal SqlExportConfig(string xml) {
            XmlDocument doc = new XmlDocument();
            doc.LoadXml(xml);
            var root = doc.DocumentElement;
            foreach (XmlNode child in root.ChildNodes) {
                switch (child.Name) {
                    case "Folder":
                        Folder = child.InnerText;
                        break;
                }
            }
        }

       public SqlExportConfig() { } 
       #region Folder
        private string _folder;    
        public string Folder { 
            get{
                return _folder;
                } 
            set{
                _folder = value;
                 OnPropertyChanged("Folder");
                }
       }
#endregion Folder
        public string GetXmlRepresentation() {
            var result = new StringBuilder();
            var settings = new XmlWriterSettings { Indent = true, IndentChars = "  ", NewLineChars = Environment.NewLine };
            var writer = XmlWriter.Create(result, settings);

            writer.WriteStartDocument();
            writer.WriteStartElement("Config");

            
            writer.WriteElementString("Folder", Folder);

            writer.WriteEndElement();
            writer.WriteEndDocument();

            writer.Close();

            return result.ToString();
        }

        public bool Validate(out string error) {
            error = string.Empty;
            return true;
        }
    }
}
