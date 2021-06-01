using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SystemDb;
using Utils;
using ViewboxAdmin_ViewModel.Structures.Config;

namespace ViewboxAdmin.Models.ViewboxDb {
    public class TableObjectsModel : NotifyPropertyChangedBase{
        public TableObjectsModel(IProfile profile) {
            Profile = profile;
            profile.PropertyChanged += profile_PropertyChanged;
        }

        void profile_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e) {
            if (e.PropertyName == "Loaded") {
                OnPropertyChanged("TableObjects");
            }
        }

        public ITableObjectCollection TableObjects { get { return Profile.Loaded ? Profile.SystemDb.Objects : null; } }
        private IProfile Profile { get; set; }
    }
}
