// --------------------------------------------------------------------------------
// author: Mirko Dibbert
// since:  2011-07-26
// copyright 2011 AvenDATA GmbH
// --------------------------------------------------------------------------------
using System.Windows;
using DatabaseManagement.Manager;
using Utils;

namespace DatabaseManagement.Models {
    public class MainWindowModel : NotifyPropertyChangedBase {
        public MainWindowModel(Window owner) {
            Owner = owner;
            BackupConfig = new BackupConfig(Owner, DatabaseManager.DatabaseConfig.BackupDirectory);
        }

        #region BackupConfig
        public BackupConfig BackupConfig { get; set; }
        #endregion

        #region DatabaseInfoModel
        private DatabaseInfoModel _databaseInfoModel = new DatabaseInfoModel();

        public DatabaseInfoModel DatabaseInfoModel { get { return _databaseInfoModel; } set { _databaseInfoModel = value; } }
        #endregion

        public Window Owner { get; set; }

        internal void DatabaseMayHaveChanged() { DatabaseInfoModel.DatabaseMayHaveChanged(); }
    }
}