// --------------------------------------------------------------------------------
// author: Solueman Hussain
// since:  2011-11-03
// copyright 2011 AvenDATA GmbH
// --------------------------------------------------------------------------------
namespace eBalanceKitBusiness.Structures {
    public class UserInfoActiveDirectory : Utils.NotifyPropertyChangedBase {
        public string Name { get; set; }
        public string LoginName { get; set; }

        #region IsChecked
        private bool _isChecked;

        public bool IsChecked {
            get { return _isChecked; }
            set {
                _isChecked = value;
                OnPropertyChanged("IsChecked");
            }
        }
        #endregion
        
        #region IsSelected
        private bool _isSelected;

        public bool IsSelected {
            get { return _isSelected; }
            set {
                _isSelected = value;
                OnPropertyChanged("IsSelected");
            }
        }
        #endregion
    }
}
