using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Utils;
using System.Xml;
using Config.Interfaces.Config;

namespace Config.Config {
    public class GdpduOutputConfig : ConfigBase, IGdpduConfig {

        internal GdpduOutputConfig(string xml) {
            XmlDocument doc = new XmlDocument();
            doc.LoadXml(xml);
            var root = doc.DocumentElement;
            foreach (XmlNode child in root.ChildNodes) {
                switch (child.Name) {
                    case "XmlName":
                        XmlName = child.InnerText;
                        break;

                    case "XmlLocation":
                        XmlLocation = child.InnerText;
                        break;

                    case "XmlComment":
                        XmlComment = child.InnerText;
                        break;

                    case "Folder":
                        Folder = child.InnerText;
                        break;


                }
            }
        }

        public GdpduOutputConfig() { }

        #region XmlName
        private string _xmlName;
        public string XmlName {
            get { return _xmlName; }

            set {
                _xmlName = value;
                OnPropertyChanged("XmlName");
            }
        }
        #endregion XmlName

        #region XmlLocation
        private string _xmlLocation;
        public string XmlLocation {
            get { return _xmlLocation; }

            set {
                _xmlLocation = value;
                OnPropertyChanged("XmlLocation");
            }
        }
        #endregion XmlLocation

        #region XmlComment
        private string _xmlComment;
        public string XmlComment {
            get { return _xmlComment; }

            set {
                _xmlComment = value;
                OnPropertyChanged("XmlComment");
            }
        }
        #endregion XmlComment

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

        public string GetXmlRepresentation() {
            var result = new StringBuilder();
            var settings = new XmlWriterSettings { Indent = true, IndentChars = "  ", NewLineChars = Environment.NewLine };
            var writer = XmlWriter.Create(result, settings);

            writer.WriteStartDocument();
            writer.WriteStartElement("Config");

            writer.WriteElementString("XmlName", XmlName);
            writer.WriteElementString("XmlLocation", XmlLocation);
            writer.WriteElementString("XmlComment", XmlComment);
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
