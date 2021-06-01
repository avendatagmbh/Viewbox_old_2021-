using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Utils;

namespace DbSearch.Models {
    public class AddEmptyQueryModel : NotifyPropertyChangedBase{

        #region Constructor
        public AddEmptyQueryModel(DbSearchLogic.Config.Profile profile) {
            Name = "Abfrage";
            _profile = profile;
        }
        #endregion Constructor

        #region Properties
        private DbSearchLogic.Config.Profile _profile;

        #region Name
        private string _name;

        public string Name {
            get { return _name; }
            set {
                if (_name != value) {
                    _name = value;
                    OnPropertyChanged("Name");
                }
            }
        }
        #endregion Name
        #endregion Properties

        #region Methods
        public void Save() {
            _profile.Queries.AddEmptyQuery(_profile, Name);
        }
        #endregion Methods
    }
}
