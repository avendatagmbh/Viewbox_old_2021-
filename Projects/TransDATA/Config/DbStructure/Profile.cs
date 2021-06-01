// --------------------------------------------------------------------------------
// author: Mirko Dibbert
// since: 2011-08-27
// copyright 2012 AvenDATA GmbH
// --------------------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Diagnostics;
using AV.Log;
using Base.Localisation;
using Config.Enums;
using Config.Interfaces.DbStructure;
using Config.Interfaces.Mail;
using Config.Manager;
using DbAccess;
using Utils;
using System.Linq;
using log4net;

namespace Config.DbStructure {
    /// <summary>
    /// This class represents an export profile.
    /// </summary>
    [DbTable("profiles")]
    internal class Profile : NotifyPropertyChangedBase, IProfile {
        internal ILog _log = LogHelper.GetLogger();

        public Profile() {
            MaxThreadCount = 1;
            IsDatabaseAnalysed = false;
        }

        #region Id
        [DbColumn("id", AllowDbNull = false)]
        [DbPrimaryKey]
        public long Id { get; set; }
        #endregion

        #region Name
        private string _name;

        [DbColumn("name", Length = 64)]
        public string Name {
            get { return _name; }
            set {
                if (_name != value) {
                    _name = value;
                    if (_name != null && _name.Length > 64) _name = _name.Substring(0, 64);
                    OnPropertyChanged("Name");
                    OnPropertyChanged("DisplayString");
                    Save(false);
                }
            }
        }
        #endregion Name

        #region InputConfig
        private IInputConfig _inputConfig = new InputConfig(InputConfigTypes.Database);

        public IInputConfig InputConfig {
            get { return _inputConfig; }
            set {
                _inputConfig = value;
                OnPropertyChanged("InputConfig");
            }
        }

        [DbColumn("input_config_id")]
        private InputConfig InputConfigDb { get { return (InputConfig) _inputConfig; } set { _inputConfig = value; } }
        #endregion InputConfig

        #region OutputConfig
        private IOutputConfig _outputConfig = new OutputConfig(OutputConfigTypes.Database);

        public IOutputConfig OutputConfig {
            get { return _outputConfig; }
            set {
                _outputConfig = value;
                OnPropertyChanged("OutputConfig");
            }
        }

        [DbColumn("output_config_id")]
        private OutputConfig OutputConfigDb { get { return (OutputConfig) _outputConfig; } set { _outputConfig = value; } }
        #endregion

        #region MailConfig
        private IMailConfig _mailConfig;

        //public IMailConfig MailConfig { get { return _mailConfig ?? (_mailConfig = MailManager.CreateMailConfig()); } }
        public IMailConfig MailConfig { get { return MailManager.GlobalMailConfig; } }

        [DbColumn("mail_config_id")]
        private MailConfig MailConfigDb { get { return (MailConfig) _mailConfig; } set { _mailConfig = value; } }
        #endregion InputConfig

        #region MaxThreadCount
        private int _maxThreadCount;

        [DbColumn("max_thread_count")]
        public int MaxThreadCount {
            get { return _maxThreadCount; }
            set {
                _maxThreadCount = value;
                if (_maxThreadCount < 1) _maxThreadCount = 1;
                OnPropertyChanged("MaxThreadCount");
                Save(false);
            }
        }
        #endregion MaxThreadCount

        #region IsDatabaseAnalysed
        private bool _isDatabaseAnalysed = true;

        [DbColumn("IsDatabaseAnalysed")]
        public bool IsDatabaseAnalysed {
            get { return _isDatabaseAnalysed; }
            set {
                _isDatabaseAnalysed = value;
                Save(false);
            }
        }
        #endregion IsDatabaseAnalysed

        #region LogPerformance
        private bool _logPerformance = true;

        [DbColumn("log_performance")]
        public bool LogPerformance {
            get { return _logPerformance; }
            set {
                _logPerformance = value;
                Save(false);
            }
        }
        #endregion LogPerformance

        #region Tables
        private ObservableCollectionAsync<Table> _tables;

        public IEnumerable<ITable> Tables { get { return TablesDbMapping; } }

        [DbCollection("ProfileDbMapping", LazyLoad = true, CascadeSave = false)]
        internal ICollection<Table> TablesDbMapping {
            get {
                if (_tables == null) LoadTables();
                return _tables;
            }
        }
        #endregion Tables

        #region Files
        private ObservableCollectionAsync<File> _files;

        public IEnumerable<IFile> Files { get { return FilesDbMapping; } }

        [DbCollection("ProfileDbMapping", LazyLoad = true, CascadeSave = false)]
        internal ICollection<File> FilesDbMapping {
            get {
                if (_files == null) LoadFiles();
                return _files;
            }
        }
        #endregion Tables

        #region DisplayString
        public string DisplayString { get { return string.IsNullOrEmpty(Name) ? "<" + Base.Localisation.ResourcesCommon.NoName + ">" : Name; } }
        #endregion

        #region IsSelected
        public bool IsSelected {
            get { return _isSelected; }
            set {
                if (_isSelected == value) return;
                _isSelected = value;
                OnPropertyChanged("IsSelected");
            }
        }

        private bool _isSelected;

        #region DataSourceAvailable
        private bool _dataSourceAvailable;

        public bool DataSourceAvailable {
            get { return _dataSourceAvailable; }
            set {
                if (_dataSourceAvailable != value) {
                    _dataSourceAvailable = value;
                    OnPropertyChanged("DataSourceAvailable");
                }
            }
        }
        #endregion DataSourceAvailable

        #region DataDestinationAvailable
        private bool _dataDestinationAvailable;

        public bool DataDestinationAvailable {
            get { return _dataDestinationAvailable; }
            set {
                if (_dataDestinationAvailable != value) {
                    _dataDestinationAvailable = value;
                    OnPropertyChanged("DataDestinationAvailable");
                }
            }
        }
        #endregion DataDestinationAvailable

        #region DataSourceTooltip
        private string _dataSourceTooltip;

        public string DataSourceTooltip {
            get { return _dataSourceTooltip; }
            set {
                if (_dataSourceTooltip != value) {
                    _dataSourceTooltip = value;
                    OnPropertyChanged("DataSourceTooltip");
                }
            }
        }
        #endregion DataSourceTooltip

        #region DataDestinationTooltip
        private string _dataDestinationTooltip;

        public string DataDestinationTooltip {
            get { return _dataDestinationTooltip; }
            set {
                if (_dataDestinationTooltip != value) {
                    _dataDestinationTooltip = value;
                    OnPropertyChanged("DataDestinationTooltip");
                }
            }
        }
        #endregion DataDestinationTooltip

        #endregion

        #region DoDbUpdate
        private bool _doDbUpdate;

        public bool DoDbUpdate {
            get { return _doDbUpdate; }
            set {
                _doDbUpdate = value;
                ((InputConfig) InputConfig).DoDbUpdate = value;
                ((OutputConfig) OutputConfig).DoDbUpdate = value;
                //((MailConfig) MailConfig).DoDbUpdate = value;
            }
        }
        #endregion

        #region methods

        #region LoadTables
        private void LoadTables() {
            _tables = new ObservableCollectionAsync<Table>();

            using (var conn = ConfigDb.GetConnection()) {
                foreach (var table in conn.DbMapping.LoadSorted<Table>(conn.Enquote("profile_id") + "=" + Id, "Name")) {
                    table.ProfileDbMapping = this;
                    _tables.Add(table);
                    table.DoDbUpdate = true;
                }
            }
        }
        #endregion

        #region LoadFiles
        private void LoadFiles() {
            _files = new ObservableCollectionAsync<File>();

            using (var conn = ConfigDb.GetConnection()) {
                foreach (var file in conn.DbMapping.LoadSorted<File>(conn.Enquote("profile_id") + "=" + Id, "Name")) {
                    file.ProfileDbMapping = this;
                    _files.Add(file);
                    file.DoDbUpdate = true;
                }
            }
        }
        #endregion

        #region Save
        public void Save(bool saveAll = true) {
            _log.ContextLog( LogLevelEnum.Debug,"DoDbUpdate:{0} saveAll:{1}", DoDbUpdate, saveAll);

            if (DoDbUpdate) ConfigDb.Save(this);
            if (saveAll) {
                InputConfig.Save();
                OutputConfig.Save();
                MailConfig.Save();
            }
        }
        #endregion Save

        #region SaveTables
        public void SaveTables() { if (DoDbUpdate) ConfigDb.Save(Tables); }

        public void SaveTable(ITable table) {
            using (var conn = ConfigDb.GetConnection()) {
                conn.DbMapping.Save(table);
            }
        }
        #endregion SaveTables

        #region SaveFiles
        public void SaveFiles() { if (DoDbUpdate) ConfigDb.Save(Files); }
        #endregion SaveFiles

        #region CreateTable
        public ITable CreateTable() { return new Table {ProfileDbMapping = this, DoDbUpdate = true}; }
        //public ITableWithParts CreateTableWithParts() { return new TableWithParts { ProfileDbMapping = this }; }
        #endregion

        #region CreateFile
        public IFile CreateFile() { return new File {ProfileDbMapping = this, DoDbUpdate = true}; }
        #endregion

        #region AddTable
        public void AddTable(ITable table) {
            if (_tables == null) LoadTables();

            Debug.Assert(table is Table, "Table object must be of type Config.DbStructure.Table!");
            Debug.Assert(_tables != null, "_tables != null");

            _tables.Add((Table) table);
        }
        #endregion

        #region AddFile
        public void AddFile(IFile file) {
            if (_files == null) LoadFiles();

            Debug.Assert(file is File, "File object must be of type Config.DbStructure.File!");
            Debug.Assert(_files != null, "_files != null");

            _files.Add((File) file);
        }
        #endregion

        #region ClearTransferEntities
        public void ClearTransferEntities() {
            using (var conn = ConfigDb.GetConnection()) {
                // delete assigned tables, columns and files
                foreach (var type in new List<Type> {typeof (Table), typeof (TableColumn), typeof (File)})
                    conn.ExecuteNonQuery(
                        "DELETE FROM " + conn.DbMapping.GetEnquotedTableName(type) +
                        " WHERE " + conn.Enquote("profile_id") + "=" + Id);

                if (_tables != null) _tables.Clear();
                if (_files != null) _files.Clear();
            }
        }
        #endregion

        public IProfile Clone() {
            var result = new Profile();
            result.InputConfig.Type = this.InputConfig.Type;
            ((InputConfig) result.InputConfig).Config = this.InputConfig.Config;
            result.OutputConfig.Type = this.OutputConfig.Type;
            ((OutputConfig) result.OutputConfig).Config = this.OutputConfig.Config;
            return result;
        }
        #endregion methods
    }
}