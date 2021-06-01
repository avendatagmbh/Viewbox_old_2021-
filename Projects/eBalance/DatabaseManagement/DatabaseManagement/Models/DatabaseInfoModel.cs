// --------------------------------------------------------------------------------
// author: Benjamin Held
// since:  2011-07-26
// copyright 2011 AvenDATA GmbH
// --------------------------------------------------------------------------------
using DatabaseManagement.DbUpgrade;
using DatabaseManagement.Manager;
using Utils;
using eBalanceKitBase;

namespace DatabaseManagement.Models {
    public class DatabaseInfoModel : NotifyPropertyChangedBase {

        #region Properties
        public string DbType { get { return DatabaseManager.DatabaseConfig.DbConfig.DbType; } }

        public string DbHostname { get { return DatabaseManager.DatabaseConfig.DbConfig.Hostname; } }

        public string DbName { get { return DatabaseManager.DatabaseConfig.DbConfig.DbName; } }

        public string DbVersion {
            get {
                var upgrader = new Upgrader(null);

                return upgrader.GetDatabaseVersion();
            }
        }

        public string CurrentDbVersion { get { return VersionInfo.Instance.CurrentDbVersion; } }

        public bool HasDatabaseUpgrade {
            get {
                var upgrader = new Upgrader(null);
                return upgrader.UpgradeExists();
            }
        }
        #endregion

        #region Methods
        internal void DatabaseMayHaveChanged() {
            OnPropertyChanged("DbType");
            OnPropertyChanged("DbHostname");
            OnPropertyChanged("DbName");
            OnPropertyChanged("DbVersion");
            OnPropertyChanged("HasDatabaseUpgrade");
        }
        #endregion
    }
}