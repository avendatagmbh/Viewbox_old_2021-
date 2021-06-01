// --------------------------------------------------------------------------------
// author: Marcus Gerlach
// since: 2012-01-20
// copyright 2012 AvenDATA GmbH
// --------------------------------------------------------------------------------
using System;
using System.Text;
using System.Xml;
using Config.Interfaces.Config;

namespace Config.Config {
    public class CsvOutputConfig : ConfigBase, ICsvOutputConfig {

        public CsvOutputConfig(string xml) {
            XmlDocument doc = new XmlDocument();
            doc.LoadXml(xml);
            var root = doc.DocumentElement;
            foreach (XmlNode child in root.ChildNodes) {
                switch (child.Name) {
                    case "Folder":
                        Folder = child.InnerText;
                        break;

                    case "CompressAfterExport":
                        CompressAfterExport = bool.Parse(child.InnerText);
                        break;

                    case "FieldSeperator":
                        FieldSeperator = child.InnerText;
                        break;

                    case  "LineEndSeperator":
                        LineEndSeperator = child.InnerText;
                        break;

                    case  "DoAnalyzation":
                        DoAnalyzation = bool.Parse(child.InnerText);
                        break;

                    case "FileEncoding":
                        FileEncoding = child.InnerText;
                        break;

                    case "DoFileSplit":
                        DoFileSplit = bool.Parse(child.InnerText);
                        break;

                    case "FileSplitSize":
                        FileSplitSize = int.Parse(child.InnerText);
                        break;
                }
           }
        }

        internal CsvOutputConfig() {
            FieldSeperator = "<<DIV>>";
            LineEndSeperator = "<<EOL>>";
            FileEncoding = "utf-8";
            DoFileSplit = false;
            FileSplitSize = 100000;
        }

        #region CompressAfterExport
        private bool _compressAfterExport;

        public bool CompressAfterExport {
            get { return _compressAfterExport; }
            set {
                _compressAfterExport = value;
                OnPropertyChanged("CompressAfterExport");
            }
        }
        #endregion CompressAfterExport

        #region AutoCreateProtocol
        private bool _autoCreateDocument;

        public bool AutoCreateProtocol
        {
            get { return _autoCreateDocument; }
            set
            {
                _autoCreateDocument = value;
                OnPropertyChanged("AutoCreateProtocol");
            }
        }
        #endregion AutoCreateProtocol

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

        #region FileEncoding
        private string _fileEncoding;

        public string FileEncoding {
            get { return _fileEncoding; }
            set {
                _fileEncoding = value;
                OnPropertyChanged("FileEncoding");
            }
        }
        #endregion FileEncoding

        #region FieldSeperator
        private string _fieldSeperator;
        
        public string FieldSeperator {
            get { return _fieldSeperator; }
            set {
                _fieldSeperator = value;
                OnPropertyChanged("FieldSeperator");
            }
        }
        #endregion FieldSeperator

        #region LineEndSeperator
        private string _lineEndSeperator;

        public string LineEndSeperator {
            get { return _lineEndSeperator; }
            set {
                _lineEndSeperator = value;
                OnPropertyChanged("LineEndSeperator");
            }
        }
        #endregion LineEndSeperator

        #region DoAnalyzation
        private bool _doAnalyzation;

        public bool DoAnalyzation {
            get { return _doAnalyzation; }
            set {
                _doAnalyzation = value;
                OnPropertyChanged("DoAnalyzation");
            }
        }
        #endregion DoAnalyzation

        #region DoFileSplit
        private bool _doFileSplit;

        public bool DoFileSplit {
            get { return _doFileSplit; }
            set {
                _doFileSplit = value;
                OnPropertyChanged("DoFileSplit");
            }
        }
        #endregion DoFileSplit

        #region FileSplitSize
        private int _fileSplitSize;

        public int FileSplitSize {
            get { return _fileSplitSize; }
            set {
                _fileSplitSize = value;
                OnPropertyChanged("FileSplitSize");
            }
        }
        #endregion FileSplitSize

        public string GetXmlRepresentation() {
            var result = new StringBuilder();
            var settings = new XmlWriterSettings { Indent = true, IndentChars = "  ", NewLineChars = Environment.NewLine };
            var writer = XmlWriter.Create(result, settings);

            writer.WriteStartDocument();
            writer.WriteStartElement("Config");

            writer.WriteElementString("Folder", Folder);
            writer.WriteElementString("CompressAfterExport", CompressAfterExport.ToString());
            writer.WriteElementString("AutoCreateProtocol", AutoCreateProtocol.ToString());
            writer.WriteElementString("FieldSeperator", FieldSeperator);
            writer.WriteElementString("LineEndSeperator", LineEndSeperator);
            writer.WriteElementString("DoAnalyzation", DoAnalyzation.ToString());
            writer.WriteElementString("FileEncoding", FileEncoding);
            writer.WriteElementString("DoFileSplit", DoFileSplit.ToString());
            writer.WriteElementString("FileSplitSize", FileSplitSize.ToString());

            writer.WriteEndElement();
            writer.WriteEndDocument();

            writer.Close();

            return result.ToString();
        }

        public bool Validate(out string error) {
            error = "";
            return true;
        }
    }
}