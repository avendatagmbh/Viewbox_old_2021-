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

namespace Config.Config {
    public class DatabaseOutputConfig : ConfigBase, IDatabaseOutputConfig {

        public DatabaseOutputConfig(string xml) {
            XmlDocument doc = new XmlDocument();
            doc.LoadXml(xml);
            var root = doc.DocumentElement;
            foreach (XmlNode child in root.ChildNodes) {
                switch (child.Name) {
                    case "DbType":
                        DbType = child.InnerText;
                        break;

                    case "ConnectionString":
                        ConnectionString = child.InnerText;
                        break;

                    case "DbTemplateName":
                        DbTemplateName = child.InnerText;
                        break;

                    case "UseImportDatabases":
                        UseImportDatabases = bool.Parse(child.InnerText);
                        break;

                    case "UseDatabaseTablePrefix":
                        UseDatabaseTablePrefix = bool.Parse(child.InnerText);
                        break;

                    case "UseCompressDatabase":
                        UseCompressDatabase = bool.Parse(child.InnerText);
                        break;

                    case "IsMsql":
                        IsMsSql = bool.Parse(child.InnerText);
                        break;
                        
                    case "BatchSize":
                        BatchSize = int.Parse(child.InnerText);
                        break;
                }
            }
            QueuedInsertPackages = 10;
            CountInsertLines = 1000;
        }

        internal DatabaseOutputConfig() {
            QueuedInsertPackages = 10;
            CountInsertLines = 1000;
        }

        #region UseImportDatabases
        private bool _useImportDatabases;

        public bool UseImportDatabases {
            get { return _useImportDatabases; }
            set {
                if (_useImportDatabases != value) {
                    _useImportDatabases = value;
                    OnPropertyChanged("UseImportDatabases");
                }
            }
        }
        #endregion UseImportDatabases

        #region UseDatabaseTablePrefix
        private bool _useDatabaseTablePrefix;

        public bool UseDatabaseTablePrefix {
            get { return _useDatabaseTablePrefix; }
            set {
                if (_useDatabaseTablePrefix != value) {
                    _useDatabaseTablePrefix = value;
                    OnPropertyChanged("UseDatabaseTablePrefix");
                }
            }
        }
        #endregion UseDatabaseTablePrefix

        #region UseCompressDatabase
        private bool _useCompressDatabase;

        public bool UseCompressDatabase {
            get { return _useCompressDatabase; }
            set {
                if (_useCompressDatabase != value) {
                    _useCompressDatabase = value;
                    OnPropertyChanged("UseCompressDatabase");
                }
            }
        }
        #endregion UseCompressDatabase

        #region IsMsSql
        private bool _isMsSql;

        public bool IsMsSql
        {
            get { return _isMsSql; }
            set
            {
                if (_isMsSql != value)
                {
                    _isMsSql = value;
                    OnPropertyChanged("IsMsSql");
                }
            }
        }

      
        #endregion IsMsSql

        #region BathSize
        private int _batchSize=100;

        public int BatchSize
        {
            get { return _batchSize; }
            set
            {
                if (_batchSize != value)
                {
                    _batchSize = value;
                    OnPropertyChanged("BatchSize");
                }
            }
        }


        #endregion IsMsSql

        #region DbType
        private string _dbtype = "MySQL";

        public string DbType
        {
            get { return _dbtype; }
            set
            {
                _dbtype = value;
                OnPropertyChanged("DbType");
            }
        }
        #endregion DbType

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

        #region CountInsertLines
        public int CountInsertLines { get; set; }
        #endregion

        #region QueuedInsertPackages
        public int QueuedInsertPackages { get; set; }
        #endregion

        public DbConfig DbConfig {
            get { 
                DbConfig result = new DbConfig(DbType);
                result.ConnectionString = ConnectionString;
                return result;
            }
        }
        
        public string GetXmlRepresentation() {

            var result = new StringBuilder();
            var settings = new XmlWriterSettings { Indent = true, IndentChars = "  ", NewLineChars = Environment.NewLine };
            var writer = XmlWriter.Create(result, settings);
            
            writer.WriteStartDocument();
            writer.WriteStartElement("Config");

            writer.WriteElementString("DbType", DbType);
            writer.WriteElementString("DbTemplateName", DbTemplateName);
            writer.WriteElementString("ConnectionString", ConnectionString);
            writer.WriteElementString("UseImportDatabases", UseImportDatabases.ToString());
            writer.WriteElementString("UseDatabaseTablePrefix", UseDatabaseTablePrefix.ToString());
            writer.WriteElementString("UseCompressDatabase", UseCompressDatabase.ToString());
            writer.WriteElementString("IsMsql", IsMsSql.ToString());
            writer.WriteElementString("BatchSize", BatchSize.ToString());
            writer.WriteEndElement();
            writer.WriteEndDocument();
            
            writer.Close();

            return result.ToString();
        }

        public bool Validate(out string error) {
            if (string.IsNullOrEmpty(ConnectionString)) {
                error = "Es wurde keine Ausgabedatenbank ausgewählt";
                return false;
            }
            if (IsMsSql)
                DbType = "SQLServer";
            DbConfig dbConfig = (DbConfig) DbConfig.Clone();
            dbConfig.DbName = "";
            return ConnectionManager.TestDbConnection(dbConfig, out error);
        }
    }
}