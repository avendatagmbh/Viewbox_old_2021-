using System.Collections.ObjectModel;
using System.ComponentModel;
using DbAccess.Structures;
using Utils;

namespace DbConsolidate.Models {
    public class DestinationDatabaseModel : NotifyPropertyChangedBase{
        #region Constructor
        public DestinationDatabaseModel(DbConfig dbConfig) {
            _dbConfig = dbConfig;
        }
        #endregion

        #region Properties

        #region DbConfig
        private DbConfig _dbConfig;

        public DbConfig DbConfig {
            get { return _dbConfig; }
            set {
                if (_dbConfig != value) {
                    _dbConfig = value;
                    OnPropertyChanged("DbConfig");
                }
            }
        }
        #endregion DbConfig
        #endregion
    }
}
