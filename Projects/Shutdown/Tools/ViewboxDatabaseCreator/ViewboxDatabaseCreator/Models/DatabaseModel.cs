using System.Collections.ObjectModel;
using System.ComponentModel;
using DbAccess.Structures;

namespace ViewboxDatabaseCreator.Models {
    public class DatabaseModel :INotifyPropertyChanged{
        #region events
        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged(string propertyName) { if (PropertyChanged != null) PropertyChanged(this, new PropertyChangedEventArgs(propertyName)); }
        #endregion

        #region Constructor
        public DatabaseModel(string databaseName) {
            DbConfig = new DbConfig("MySQL"){Hostname = "localhost", Username = "root", Password = "avendata"};
            DatabaseName = databaseName;
        }
        #endregion

        #region Properties
        public DbConfig DbConfig { get; set; }
        public string DatabaseName { get; set; }
        #endregion
    }
}
