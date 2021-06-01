using Utils;

namespace eBalanceKit.Models.Assistants {
    public class ProfileNameModel : NotifyPropertyChangedBase{
        #region ProfileName
        private string _profileName;

        public string ProfileName {
            get { return _profileName; }
            set {
                if (_profileName != value) {
                    if (value.Length > 30) {
                        value = value.Substring(0, 30);
                    }
                    _profileName = value;
                    OnPropertyChanged("ProfileName");
                }
            }
        }
        #endregion ProfileName

        public bool PressedOk { get; set; }
    }
}
