using System.Collections.ObjectModel;
using System.ComponentModel;
using DbAccess.Structures;

namespace ViewValidator.Models.Profile {
    public class DatabaseModel :INotifyPropertyChanged{
        #region events
        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged(string propertyName) { if (PropertyChanged != null) PropertyChanged(this, new PropertyChangedEventArgs(propertyName)); }
        #endregion

        #region Constructor
        public DatabaseModel(DbConfig configSource, DbConfig configDest) {
            this.ConfigSource = configSource;
            this.ConfigDest = configDest;
        }
        #endregion

        #region Properties
        #region TablesSource
        private ObservableCollection<string> _tablesSource = new ObservableCollection<string>();
        public ObservableCollection<string> TablesSource {
            get { return _tablesSource; }
            set { _tablesSource = value; }
        }
        #endregion

        #region TablesDestination
        private ObservableCollection<string> _tablesDestination = new ObservableCollection<string>();
        public ObservableCollection<string> TablesDestination {
            get { return _tablesDestination; }
            set { _tablesDestination = value; }
        }
        #endregion

        #region SelectedTableSource
        private string _selectedTableSource;
        public string SelectedTableSource {
            get { return _selectedTableSource; }
            set {
                if (_selectedTableSource != value) {
                    _selectedTableSource = value;
                    OnPropertyChanged("SelectedTableSource");
                }
            }
        }
        #endregion

        #region SelectedTableDestination
        private string _selectedTableDestination;
        public string SelectedTableDestination {
            get { return _selectedTableDestination; }
            set {
                if (_selectedTableDestination != value) {
                    _selectedTableDestination = value;
                    OnPropertyChanged("SelectedTableDestination");
                }
            }
        }
        #endregion

        public DbConfig ConfigSource { get; set; }
        public DbConfig ConfigDest { get; set; }
        #endregion
    }
}
