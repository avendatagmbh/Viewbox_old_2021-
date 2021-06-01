// --------------------------------------------------------------------------------
// author: Mirko Dibbert
// since: 2012-01-12
// copyright 2012 AvenDATA GmbH
// --------------------------------------------------------------------------------
using System;
using System.Diagnostics;
using System.Text;
using System.Xml;
using Config.Interfaces.Config;
using Utils;

namespace Config.Config {
    public class BinaryConfig : ConfigBase, IBinaryConfig {
        internal BinaryConfig(string xml) {
            var doc = new XmlDocument();
            doc.LoadXml(xml);
            XmlElement root = doc.DocumentElement;
            Debug.Assert(root != null, "root != null");
            foreach (XmlNode child in root.ChildNodes) {
                switch (child.Name) {
                    case "Folder":
                        Folder = child.InnerText;
                        break;
                }
            }
        }

        public BinaryConfig() { }

        #region Folder
        private string _folder;

        public string Folder {
            get { return _folder; }
            set {
                _folder = value;
                OnPropertyChanged("Folder");
            }
        }
        #endregion Folder

        #region IBinaryConfig Members
        public string GetXmlRepresentation() {
            var result = new StringBuilder();
            var settings = new XmlWriterSettings {Indent = true, IndentChars = "  ", NewLineChars = Environment.NewLine};
            XmlWriter writer = XmlWriter.Create(result, settings);

            writer.WriteStartDocument();
            writer.WriteStartElement("Config");


            writer.WriteElementString("Folder", Folder);

            writer.WriteEndElement();
            writer.WriteEndDocument();

            writer.Close();

            return result.ToString();
        }

        public bool Validate(out string error) {
            error = "";
            return true;
        }
        #endregion
    }
}