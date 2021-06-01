using System;
using System.IO;
using System.Windows;
using System.Xml.Serialization;
using DbAccess;
using DbAccess.Structures;
using RtfConverterBusiness;
using RtfConverterWpf.Structures;
using Utils;

namespace RtfConverterWpf.Models {
    public class MainWindowModel : NotifyPropertyChangedBase{
        #region Constructor
        public MainWindowModel(Window owner) {
            DbConfig = new DbConfig("SQLServer"){Hostname = "edna", Username = "sa", Password = "avendata"};
            Owner = owner;
            RtfConverter = new RtfConverter();
            LoadConfigSettings();
            LoadTableSettings();
        }

        #endregion Constructor

        #region Properties
        private string _tablesFileName =
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "RtfConverter",
                         "tables.xml");
        private string _configFileName =
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "RtfConverter",
                         "config.xml");
        public DbConfig DbConfig { get; set; }
        public Window Owner { get; private set; }

        #region RtfConverter
        private RtfConverter _rtfConverter;

        public RtfConverter RtfConverter {
            get { return _rtfConverter; }
            set {
                if (_rtfConverter != value) {
                    _rtfConverter = value;
                    OnPropertyChanged("RtfConverter");
                    OnPropertyChanged("Tables");
                }
            }
        }
        #endregion RtfConverter

        public ObservableCollectionAsync<Table> Tables { get { return RtfConverter.Tables; } }

        #region TargetDbName
        private string _targetDbName;

        public string TargetDbName {
            get { return _targetDbName; }
            set {
                if (_targetDbName != value) {
                    _targetDbName = value;
                    OnPropertyChanged("TargetDbName");
                }
            }
        }
        #endregion TargetDbName

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
            RtfConverter.FetchTables(DbConfig);
            CheckIsCheckedHeaderState();
            //foreach (var table in Tables) {
            //    table.PropertyChanged += table_PropertyChanged;
            //}
            
        }

        void table_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e) {
            if (e.PropertyName == "IsChecked") {
                Table table = sender as Table;
                CheckIsCheckedHeaderState();
            }

        }

        private void CheckIsCheckedHeaderState() {
            if (Tables.Count > 0) {
                bool state = Tables[0].IsChecked;
                foreach (var table in Tables) {
                    _isCheckedHeaderState = state;
                    if (state != table.IsChecked) {
                        _isCheckedHeaderState = null;
                        break;
                    }
                }
                
                OnPropertyChanged("IsCheckedHeaderState");
            }
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
            //foreach (var table in Tables) {
            //    if(table.IsChecked && string.IsNullOrEmpty(table.ColumnString))
            //}

            SaveSettings();
            ProgressDialogHelper helper = new ProgressDialogHelper(Owner);
            helper.SuccessMessage = "Export erfolgreich.";
            helper.StartJob(calculator => RtfConverter.StartConversion(calculator, DbConfig, TargetDbName));
            SaveSettings();
        }

        public void LoadTableSettings() {
            if (File.Exists(_tablesFileName)) {
                
                XmlSerializer ser = new XmlSerializer(typeof (RtfConverter));
                using (StreamReader fs = new StreamReader(_tablesFileName)) {
                    try {
                        //string oldOutputPath = TableOperations.OutputDir;
                        RtfConverter = (RtfConverter) ser.Deserialize(fs);
                        //TableOperations = (TableOperations)ser.Deserialize(fs);
                        //TableOperations.OutputDir = oldOutputPath;
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
                        TargetDbName = config.TargetDbName;
                        //TableOperations.OutputDir = config.OutputDir;
                    } catch (Exception) {
                        
                    }
                }
            }
        }

        private void SaveSettings() {
            Directory.CreateDirectory(new FileInfo(_tablesFileName).DirectoryName);
            XmlSerializer ser = new XmlSerializer(typeof(RtfConverter));

            if(File.Exists(_tablesFileName))
                File.Delete(_tablesFileName);
            using (FileStream fs = new FileStream(_tablesFileName, FileMode.OpenOrCreate)) {
                ser.Serialize(fs, RtfConverter);
            }

            Config config = new Config() {
                DbName = DbConfig.DbName,
                Hostname = DbConfig.Hostname,
                Password = DbConfig.Password,
                Username = DbConfig.Username,
                TargetDbName = TargetDbName
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
