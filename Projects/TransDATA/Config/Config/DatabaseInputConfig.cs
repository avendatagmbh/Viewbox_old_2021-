// --------------------------------------------------------------------------------
// author: Mirko Dibbert
// since: 2012-01-09
// copyright 2012 AvenDATA GmbH
// --------------------------------------------------------------------------------
using System.Globalization;
using Config.Interfaces.Config;
using DbAccess;
using DbAccess.Structures;
using Utils;
using System.Xml;
using System.Text;
using System;
using System.IO;

namespace Config.Config {
    internal class DatabaseInputConfig : ConfigBase, IDatabaseInputConfig {

        public DatabaseInputConfig(string xml) {
            XmlDocument doc = new XmlDocument();
            doc.LoadXml(xml);
            var root = doc.DocumentElement;
            foreach (XmlNode child in root.ChildNodes) {
                switch (child.Name) {
                    case "ConnectionString":
                        ConnectionString = child.InnerText;
                        break;

                    case "DbTemplateName":
                        DbTemplateName = child.InnerText;
                        break;

                    case "TableWhitelist":
                        TableWhitelist = child.InnerText;
                        break;

                    case "DatabaseWhitelist":
                        DatabaseWhitelist = child.InnerText;
                        break;

                    case "ProcessTables":
                        ProcessTables = bool.Parse(child.InnerText);
                        break;

                    case "ProcessViews":
                        ProcessViews = bool.Parse(child.InnerText);
                        break;

                    case "UseAdo":
                        UseAdo = bool.Parse(child.InnerText);
                        break;

                    case "UseCatalog":
                        UseCatalog = bool.Parse(child.InnerText);
                        break;

                    case "UseSchema":
                        UseSchema = bool.Parse(child.InnerText);
                        break;
                }
           }
        }

        internal DatabaseInputConfig() {
            UseSchema = true;
            UseCatalog = true;
        }

        #region UseAdo
        private bool _useAdo;

        public bool UseAdo {
            get { return _useAdo; }
            set {
                if (_useAdo != value) {
                    _useAdo = value;
                    OnPropertyChanged("UseAdo");
                }
            }
        }
        #endregion UseAdo

        #region UseCatalog
        private bool _useCatalog;

        public bool UseCatalog {
            get { return _useCatalog; }
            set {
                if (_useCatalog != value) {
                    _useCatalog = value;
                    OnPropertyChanged("UseCatalog");
                }
            }
        }
        #endregion UseCatalog

        #region UseSchema
        private bool _useSchema;

        public bool UseSchema {
            get { return _useSchema; }
            set {
                if (_useSchema != value) {
                    _useSchema = value;
                    OnPropertyChanged("UseSchema");
                }
            }
        }
        #endregion UseSchema

        #region TableWhitelist
        private string _tableWhitelist;

        public string TableWhitelist {
            get { return _tableWhitelist; }
            set {
                if (_tableWhitelist != value) {
                    _tableWhitelist = value;
                    OnPropertyChanged("TableWhitelist");
                }
            }
        }
        #endregion TableWhitelist

        #region DatabaseWhitelist
        private string _databaseWhitelist;

        public string DatabaseWhitelist {
            get { return _databaseWhitelist; }
            set {
                if (_databaseWhitelist != value) {
                    _databaseWhitelist = value;
                    OnPropertyChanged("DatabaseWhitelist");
                }
            }
        }
        #endregion DatabaseWhitelist

        #region DbTemplateName
        private string _dbTemplatname;
        public string DbTemplateName
        {
            get
            {
                return _dbTemplatname;
            }
            set
            {
                _dbTemplatname = value;
                OnPropertyChanged("DbTemplateName");
            }
        }
        #endregion DbTemplateName

        #region ConnectionString
        private string _connectionString;

        public string ConnectionString {
            get { return _connectionString; }
            set {
                _connectionString = value;
                OnPropertyChanged("ConnectionString");
            }
        }
        #endregion ConnectionString

        #region ProcessTables
        private bool _processTables = true;

        public bool ProcessTables {
            get { return _processTables; }
            set {
                _processTables = value;
                OnPropertyChanged("ProcessTables");
            }
        }
        #endregion ProcessTables

        #region ProcessViews
        private bool _processViews;

        public bool ProcessViews {
            get { return _processViews; }
            set {
                _processViews = value;
                OnPropertyChanged("ProcessViews");
            }
        }
        #endregion ProcessViews
        
        public string GetXmlRepresentation() {
            
            var result = new StringBuilder();
            var settings = new XmlWriterSettings { Indent = true, IndentChars = "  ", NewLineChars = Environment.NewLine };
            var writer = XmlWriter.Create(result, settings);
            
            writer.WriteStartDocument();
            writer.WriteStartElement("Config");

            writer.WriteElementString("DbTemplateName", DbTemplateName);
            writer.WriteElementString("TableWhitelist", TableWhitelist);
            writer.WriteElementString("DatabaseWhitelist", DatabaseWhitelist);
            writer.WriteElementString("ConnectionString", ConnectionString);
            writer.WriteElementString("ProcessTables", ProcessTables.ToString(CultureInfo.InvariantCulture));
            writer.WriteElementString("ProcessViews", ProcessViews.ToString(CultureInfo.InvariantCulture));
            writer.WriteElementString("UseAdo", UseAdo.ToString(CultureInfo.InvariantCulture));
            writer.WriteElementString("UseCatalog", UseCatalog.ToString(CultureInfo.InvariantCulture));
            writer.WriteElementString("UseSchema", UseSchema.ToString(CultureInfo.InvariantCulture));

            writer.WriteEndElement();
            writer.WriteEndDocument();
            
            writer.Close();

            return result.ToString();
        }

        public bool Validate(out string error) {
            if (string.IsNullOrEmpty(ConnectionString)) {
                error = "Es wurde keine Eingabedatenbank ausgewählt";
                return false;
            }
            return ConnectionManager.TestDbConnection(new DbConfig("GenericODBC"){ConnectionString = ConnectionString}, out error);
        }
    }
}