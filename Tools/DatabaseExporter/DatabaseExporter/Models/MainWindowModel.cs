using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Xml.Serialization;
using DatabaseExporter.Structures;
using DatabaseExporterBusiness;
using DbAccess;
using DbAccess.Structures;
using Utils;

namespace DatabaseExporter.Models {
    public class MainWindowModel : NotifyPropertyChangedBase{
        #region Constructor
        public MainWindowModel(Window owner) {
            DbConfig = new DbConfig("SQLServer"){Hostname = "edna", Username = "sa", Password = "avendata", DbName = "krh_viewbox"};
            Owner = owner;
            TableOperations = new TableOperations();
            LoadConfigSettings();
            LoadTableSettings();
        }

        #endregion Constructor

        #region Properties
        private string _tablesFileName =
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "DatabaseExporter",
                         "tables.xml");
        private string _configFileName =
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "DatabaseExporter",
                         "config.xml");
        public DbConfig DbConfig { get; set; }
        public Window Owner { get; private set; }

        #region TableOperations
        private TableOperations _tableOperations;

        public TableOperations TableOperations {
            get { return _tableOperations; }
            set {
                if (_tableOperations != value) {
                    _tableOperations = value;
                    OnPropertyChanged("TableOperations");
                    OnPropertyChanged("Tables");
                }
            }
        }
        #endregion TableOperations
        //public TableOperations TableOperations { get; private set; }
        public ObservableCollectionAsync<Table> Tables { get { return TableOperations.Tables; } }

        #region IsCheckedHeaderState
        private bool? _isCheckedHeaderState;

        public bool? IsCheckedHeaderState {
            get { return _isCheckedHeaderState; }
            set {
                if (_isCheckedHeaderState != value) {
                    _isCheckedHeaderState = value;
                    if (_isCheckedHeaderState.HasValue) {
                        foreach (var table in Tables)
                            table.IsChecked = _isCheckedHeaderState.Value;
                    }
                    OnPropertyChanged("IsCheckedHeaderState");
                }
            }
        }
        #endregion IsCheckedHeaderState

        #region SelectedTable
        private Table _selectedTable;

        public Table SelectedTable {
            get { return _selectedTable; }
            set {
                if (_selectedTable != value) {
                    _selectedTable = value;
                    OnPropertyChanged("SelectedTable");
                }
            }
        }
        #endregion SelectedTable
        #endregion Properties

        #region Methods
        public void LoadTables() {
            LoadTableSettings();
            TableOperations.FetchTables(DbConfig);
        }

        public bool ValidateConnection() {
            try {
                using (IDatabase conn = ConnectionManager.CreateConnection(DbConfig)) {
                    conn.Open();
                }
            } catch (Exception ex) {
                MessageBox.Show(Owner, "Fehler bei der Datenbankverbindung: " + Environment.NewLine + ex.Message, "",
                                MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
            return true;

        }
        #endregion Methods

        public void Start() {
            SaveSettings();
            ProgressDialogHelper helper = new ProgressDialogHelper(Owner);
            helper.SuccessMessage = "Export erfolgreich.";
            helper.StartJob(calculator => TableOperations.Start(calculator, DbConfig));
        }

        public void LoadTableSettings() {
            if (File.Exists(_tablesFileName)) {
                
                XmlSerializer ser = new XmlSerializer(typeof (TableOperations));
                using (StreamReader fs = new StreamReader(_tablesFileName)) {
                    try {
                        string oldOutputPath = TableOperations.OutputDir;
                        TableOperations = (TableOperations)ser.Deserialize(fs);
                        TableOperations.OutputDir = oldOutputPath;
                    } catch (Exception) {
                        
                    }
                }
            }
        }

        private void LoadConfigSettings() {
            if (File.Exists(_configFileName)) {
                XmlSerializer ser = new XmlSerializer(typeof(Config));
                using (StreamReader fs = new StreamReader(_configFileName)) {
                    try {
                        Config config = (Config)ser.Deserialize(fs);
                        DbConfig.Hostname = config.Hostname;
                        DbConfig.Username = config.Username;
                        DbConfig.Password = config.Password;
                        DbConfig.DbName = config.DbName;
                        TableOperations.OutputDir = config.OutputDir;
                    } catch (Exception) {
                        
                    }
                }
            }
        }

        private void SaveSettings() {
            Directory.CreateDirectory(new FileInfo(_tablesFileName).DirectoryName);
            XmlSerializer ser = new XmlSerializer(typeof(TableOperations));

            if(File.Exists(_tablesFileName))
                File.Delete(_tablesFileName);
            using (FileStream fs = new FileStream(_tablesFileName, FileMode.OpenOrCreate)) {
                ser.Serialize(fs, TableOperations);
            }

            Config config = new Config() {
                DbName = DbConfig.DbName,
                Hostname = DbConfig.Hostname,
                Password = DbConfig.Password,
                Username = DbConfig.Username,
                OutputDir = TableOperations.OutputDir
            };

            if(File.Exists(_configFileName))
                File.Delete(_configFileName);
            ser = new XmlSerializer(typeof(Config));
            using (FileStream fs = new FileStream(_configFileName, FileMode.OpenOrCreate)) {
                ser.Serialize(fs, config);
            }

        }
    }
}
