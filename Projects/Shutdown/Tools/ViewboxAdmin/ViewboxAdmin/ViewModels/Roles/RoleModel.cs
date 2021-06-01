using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SystemDb;
using ViewboxAdmin.ViewModels.Users;
using ViewboxAdmin_ViewModel;

namespace ViewboxAdmin.ViewModels.Roles
{
    class RoleModel : NotifyBase
    {
        public RoleModel() {
                this.Users = new TrulyObservableCollection<UserModel>();
        }

        #region Id
        private int _Id;

        public int Id {
            get { return _Id; }
            set {
                if (_Id != value) {
                    _Id = value;
                    OnPropertyChanged("Id");
                }
            }
        }
        #endregion Id

        #region Name
        private string _Name;

        public string Name {
            get { return _Name; }
            set {
                if (_Name != value) {
                    _Name = value;
                    OnPropertyChanged("Name");
                }
            }
        }
        #endregion Name

        #region Flags
        private SpecialRights _Flags;

        public SpecialRights Flags {
            get { return _Flags; }
            set {
                if (_Flags != value) {
                    _Flags = value;
                    OnPropertyChanged("Flags");
                }
            }
        }
        #endregion Flags

        public TrulyObservableCollection<UserModel> Users { get; private set;}

        public IEnumerable<SpecialRights> SpecialRightsValue {
            get {
                return Enum.GetValues(typeof(SpecialRights))
                    .Cast<SpecialRights>();
            }
        }

    }
}
