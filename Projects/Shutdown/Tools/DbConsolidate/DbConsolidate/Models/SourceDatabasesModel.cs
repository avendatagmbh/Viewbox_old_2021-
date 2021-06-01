using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using DbAccess.Structures;
using DbConsolidateBusiness;
using Utils;

namespace DbConsolidate.Models {
    public class SourceDatabasesModel : NotifyPropertyChangedBase{
        #region Constructor
        public SourceDatabasesModel(List<SourceDatabase> sourceDatabases) {
            _sourceDatabases = sourceDatabases;
        }
        #endregion

        #region Properties

        #region SourceDatabases
        private List<SourceDatabase> _sourceDatabases;

        public List<SourceDatabase> SourceDatabases {
            get { return _sourceDatabases; }
            set {
                if (_sourceDatabases != value) {
                    _sourceDatabases = value;
                    OnPropertyChanged("SourceDatabases");
                }
            }
        }

        public void AddSourceDatabase(SourceDatabase sourceDatabase) {
            _sourceDatabases.Add(sourceDatabase);
            OnPropertyChanged("SourceDatabases");
        }
        #endregion DbConfig

        #endregion
    }
}
