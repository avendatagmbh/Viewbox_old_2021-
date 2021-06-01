// --------------------------------------------------------------------------------
// author: Marcus Gerlach
// since: 2012-02-21
// copyright 2012 AvenDATA GmbH
// --------------------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using Config.Interfaces.Config;

namespace Config.Config {
    public class CsvInputConfig : ConfigBase, ICsvInputConfig {
        public CsvInputConfig(string xml) {
            TableCanHaveMultipleParts = true;

            var doc = new XmlDocument();
            doc.LoadXml(xml);
            var root = doc.DocumentElement;
            foreach (XmlNode child in root.ChildNodes) {
                switch (child.Name) {
                    case "Folder":
                        Folder = child.InnerText;
                        break;

                    case "FolderLog":
                        FolderLog = child.InnerText;
                        break;

                    case "FieldSeperator":
                        FieldSeperator = child.InnerText;
                        break;

                    case "LineEndSeperator":
                        LineEndSeperator = child.InnerText;
                        break;

                    case "FileEncoding":
                        FileEncoding = child.InnerText;
                        break;

                    case "ImportSubDirectories":
                        ImportSubDirectories = bool.Parse(child.InnerText);
                        break;

                    case "IsSapCsv":
                        IsSapCsv = bool.Parse(child.InnerText);
                        break;

                    case "IsBaanCsv":
                        IsBaanCsv = bool.Parse(child.InnerText);
                        break;

                    case "BaanCompanyIdLength":
                        BaanCompanyIdLength = int.Parse(child.InnerText);
                        break;

                    case "BaanCompanyIdField":
                        BaanCompanyIdField = child.InnerText;
                        break;

                    case "TableCanHaveMultipleParts":
                        TableCanHaveMultipleParts = bool.Parse(child.InnerText);
                        break;

                    case "HeadlineNoHeader":
                        HeadlineNoHeader = bool.Parse(child.InnerText);
                        break;

                    case "HeadlineInFirstLine":
                        HeadlineInFirstLine = bool.Parse(child.InnerText);
                        break;

                    case "HeadlineInEachFileFirstLine":
                        HeadlineInEachFileFirstLine = bool.Parse(child.InnerText);
                        break;

                    case "OptionallyEnclosedBy":
                        OptionallyEnclosedBy =  child.InnerText;
                        break;

                    case "IgnoreLines":
                        IgnoreLines = int.Parse(child.InnerText);
                        break;
                }
           }
        }

        public CsvInputConfig() {
            TableCanHaveMultipleParts = true;
            FieldSeperator = "<<DIV>>";
            LineEndSeperator = @"<<EOL>>\r\n";
            FileEncoding = "utf-8";
            ImportSubDirectories = false;
            OptionallyEnclosedBy = "\"";

            HeadlineNoHeader = false;
            HeadlineInFirstLine = true;
            HeadlineInEachFileFirstLine = false;
        }

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

        #region FolderLog
        private string _folderLog;

        public string FolderLog {
            get { return _folderLog; }
            set {
                if (_folderLog != value) {
                    _folderLog = value;
                    OnPropertyChanged("FolderLog");
                }
            }
        }
        #endregion FolderLog

        #region OptionallyEnclosedBy
        private string _optionallyEnclosedBy;

        public string OptionallyEnclosedBy {
            get { return _optionallyEnclosedBy; }
            set {
                _optionallyEnclosedBy = value;
                OnPropertyChanged("OptionallyEnclosedBy");
            }
        }
        #endregion OptionallyEnclosedBy

        #region TableCanHaveMultipleParts
        private bool _tableCanHaveMultipleParts;

        public bool TableCanHaveMultipleParts {
            get { return _tableCanHaveMultipleParts; }
            set {
                _tableCanHaveMultipleParts = value;
                OnPropertyChanged("TableCanHaveMultipleParts");
            }
        }
        #endregion TableCanHaveMultipleParts

        #region FileEncodingList
        public List<string> FileEncodingList
        {
            get {
                List<string> encs=new List<string>();
                encs.AddRange(Encoding.GetEncodings().Select(enc => enc.Name));
                return encs;
            }
        }
        #endregion FileEncodingList


        #region FileEncoding
        private string _fileEncoding;

        public string FileEncoding {
            get { return _fileEncoding; }
            set {
                _fileEncoding = value;
                if (_fileEncoding.ToLower() == "utf8" || _fileEncoding.Length == 0)
                    _fileEncoding = "utf-8";

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
        public string GetFieldSeperator() { return ReplaceEscapedChars(FieldSeperator); }
        #endregion FieldSeperator

        #region LineEndSeperator
        private string _lineEndSeperator;

        public string LineEndSeperator {
            get { return _lineEndSeperator; }
            set {
                _lineEndSeperator = ReplaceUnEscapedChars(value);
                OnPropertyChanged("LineEndSeperator");
            }
        }

        public string GetLineEndSeperator() { return ReplaceEscapedChars(LineEndSeperator); }
        #endregion LineEndSeperator

        #region HeadlineNoHeader
        private bool _headlineNoHeader;

        public bool HeadlineNoHeader
        {
            get { return _headlineNoHeader; }
            set
            {
                if (_headlineNoHeader != value)
                {
                    _headlineNoHeader = value;
                    _headlineInFirstLine = !value;
                    _headlineInEachFileFirstLine = !value;
                    OnPropertyChanged("HeadlineNoHeader");
                    OnPropertyChanged("HeadlineInFirstLine");
                    OnPropertyChanged("HeadlineInEachFileFirstLine");
                }
            }
        }
        #endregion HeadlineNoHeader



        #region HeadlineInFirstLine
        private bool _headlineInFirstLine;

        public bool HeadlineInFirstLine {
            get { return _headlineInFirstLine; }
            set {
                if (_headlineInFirstLine != value) {
                    _headlineNoHeader = !value;
                    _headlineInFirstLine = value;
                    _headlineInEachFileFirstLine = !value;
                    OnPropertyChanged("HeadlineNoHeader");
                    OnPropertyChanged("HeadlineInFirstLine");
                    OnPropertyChanged("HeadlineInEachFileFirstLine");
                }
            }
        }
        #endregion HeadlineInFirstLine
        

        #region HeadlineInEachFileFirstLine
        private bool _headlineInEachFileFirstLine;

        public bool HeadlineInEachFileFirstLine
        {
            get { return _headlineInEachFileFirstLine; }
            set
            {
                if (_headlineInEachFileFirstLine != value)
                {
                    _headlineNoHeader = !value;
                    _headlineInFirstLine = !value;
                    _headlineInEachFileFirstLine = value;
                    OnPropertyChanged("HeadlineNoHeader");
                    OnPropertyChanged("HeadlineInFirstLine");
                    OnPropertyChanged("HeadlineInEachFileFirstLine");
                }
            }
        }
        #endregion HeadlineInEachFileFirstLine



        #region IsNormalCsv
        private bool _IsNormalCsv=true;

        public bool IsNormalCsv
        {
            get { return _IsNormalCsv; }
            set
            {
                if (_IsNormalCsv != value)
                {
                    _IsNormalCsv = value;
                    _isSapCsv = !value;
                    _IsBaanCsv = !value;

                    OnPropertyChanged("IsNormalCsv");
                    OnPropertyChanged("IsSapCsv");
                    OnPropertyChanged("IsBaanCsv");
                }
            }
        }
        #endregion IsNormalCsv
        
        #region IsSAPCsv
        private bool _isSapCsv;

        public bool IsSapCsv {
            get { return _isSapCsv; }
            set {
                if (_isSapCsv != value) {
                    _IsNormalCsv = !value;
                    _isSapCsv = value;
                    _IsBaanCsv = !value;

                    OnPropertyChanged("IsNormalCsv");
                    OnPropertyChanged("IsSapCsv");
                    OnPropertyChanged("IsBaanCsv");
                }
            }
        }
        #endregion IsSAPCsv

        #region IsBaanCsv
        private bool _IsBaanCsv;

        public bool IsBaanCsv
        {
            get { return _IsBaanCsv; }
            set
            {
                if (_IsBaanCsv != value)
                {
                    _IsNormalCsv = !value;
                    _isSapCsv = !value;
                    _IsBaanCsv = value;

                    OnPropertyChanged("IsNormalCsv");
                    OnPropertyChanged("IsSapCsv");
                    OnPropertyChanged("IsBaanCsv");
                    
                    if (_IsBaanCsv) {
                        _headlineNoHeader = false;
                        _headlineInFirstLine = false;
                        _headlineInEachFileFirstLine = true;
                        _tableCanHaveMultipleParts = true;
                        OnPropertyChanged("HeadlineNoHeader");
                        OnPropertyChanged("HeadlineInFirstLine");
                        OnPropertyChanged("HeadlineInEachFileFirstLine");
                        OnPropertyChanged("TableCanHaveMultipleParts");
                    }
                }
            }
        }
        #endregion IsBaanCsv
        
        #region BaanCompanyIdLength
        private int _BaanCompanyIdLength=3;
        public int BaanCompanyIdLength
        {
            get { return _BaanCompanyIdLength; }
            set
            {
                if (_BaanCompanyIdLength != value)
                {
                    _BaanCompanyIdLength = value;
                    OnPropertyChanged("BaanCompanyIdLength");
                }
            }
        }
        #endregion BaanCompanyIdLength

        #region BaanCompanyIdField
        private string _BaanCompanyIdField = "#BAAN_CompanyID";
        public string BaanCompanyIdField
        {
            get { return _BaanCompanyIdField; }
            set
            {
                if (_BaanCompanyIdField != value)
                {
                    _BaanCompanyIdField = value;
                    OnPropertyChanged("BaanCompanyIdField");
                }
            }
        }
        #endregion

        #region IgnoreLines
        private int _ignoreLines;

        public int IgnoreLines {
            get { return _ignoreLines; }
            set {
                if (_ignoreLines != value) {
                    _ignoreLines = value;
                    OnPropertyChanged("IgnoreLines");
                }
            }
        }
        #endregion IgnoreLines

        private string ReplaceEscapedChars(string value) {
            return value
                .Replace("\\r", "\r")
                .Replace("\\n", "\n")
                .Replace("\\t", "\t");
        }

        private string ReplaceUnEscapedChars(string value) {
            return value
                .Replace("\r", "\\r")
                .Replace("\n", "\\n")
                .Replace("\t", "\\t");
        }

        #region ImportSubDirectories
        private bool _importSubDirectories;

        public bool ImportSubDirectories {
            get { return _importSubDirectories; }
            set {
                _importSubDirectories = value;
                OnPropertyChanged("ImportSubDirectories");
            }
        }
        #endregion ImportSubDirectories

        public string GetXmlRepresentation() {
            var result = new StringBuilder();
            var settings = new XmlWriterSettings { Indent = true, IndentChars = "  ", NewLineChars = Environment.NewLine };
            var writer = XmlWriter.Create(result, settings);

            writer.WriteStartDocument();
            writer.WriteStartElement("Config");

            writer.WriteElementString("Folder", Folder);
            writer.WriteElementString("FieldSeperator", FieldSeperator);
            writer.WriteElementString("LineEndSeperator", LineEndSeperator);
            writer.WriteElementString("FileEncoding", FileEncoding);
            writer.WriteElementString("ImportSubDirectories", ImportSubDirectories.ToString(CultureInfo.InvariantCulture));
            writer.WriteElementString("IsSapCsv", IsSapCsv.ToString(CultureInfo.InvariantCulture));
            writer.WriteElementString("IsBaanCsv", IsBaanCsv.ToString(CultureInfo.InvariantCulture));
            writer.WriteElementString("BaanCompanyIdLength", BaanCompanyIdLength.ToString(CultureInfo.InvariantCulture));
            writer.WriteElementString("BaanCompanyIdField", BaanCompanyIdField);
            writer.WriteElementString("TableCanHaveMultipleParts", TableCanHaveMultipleParts.ToString(CultureInfo.InvariantCulture));
            writer.WriteElementString("HeadlineNoHeader", HeadlineNoHeader.ToString(CultureInfo.InvariantCulture));
            writer.WriteElementString("HeadlineInFirstLine", HeadlineInFirstLine.ToString(CultureInfo.InvariantCulture));
            writer.WriteElementString("HeadlineInEachFileFirstLine", HeadlineInEachFileFirstLine.ToString(CultureInfo.InvariantCulture));
            writer.WriteElementString("OptionallyEnclosedBy", OptionallyEnclosedBy);
            writer.WriteElementString("IgnoreLines", IgnoreLines.ToString(CultureInfo.InvariantCulture));
            writer.WriteElementString("FolderLog", FolderLog);
            
            writer.WriteEndElement();
            writer.WriteEndDocument();

            writer.Close();

            return result.ToString();
        }

        public bool Validate(out string error) {
            if (!Directory.Exists(Folder)) {
                error = "Das Quellverzeichnis der CSV Dateien existiert nicht.";
                return false;
            }
            error = string.Empty;
            foreach (var file in Directory.EnumerateFiles(Folder, "*.*", ImportSubDirectories ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly).Where(
                                                                             s =>
                                                                             s.ToLower().EndsWith(".csv") ||
                                                                             s.ToLower().EndsWith(".txt") ||
                                                                             s.ToLower().EndsWith(".csv.zip")))
                return true;
            error = string.Format("Es sind keine CSV Dateien im Verzeichnis \"{0}\" vorhanden", Folder);
            return false;
        }

        public IEnumerable<string> GetCsvFilesInFolder() {
            return Directory.EnumerateFiles(Folder, "*.csv",
                                            ImportSubDirectories
                                                ? SearchOption.AllDirectories
                                                : SearchOption.TopDirectoryOnly);
        }
    }
}