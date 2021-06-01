// (C)opyright 2011 AvenDATA GmbH
// ----------------------------------------
// author: Mirko Dibbert
// since : 2011-08-27

using Config.Interfaces.DbStructure;
using DbAccess;
using Utils;

namespace Config.DbStructure {
    /// <summary>
    /// This class represents a User.
    /// </summary>
    [DbTable("users")]
    internal class User : NotifyPropertyChangedBase, IUser {
        #region Id
        [DbColumn("id", AllowDbNull = false)]
        [DbPrimaryKey]
        public int Id { get; set; }
        #endregion

        #region UserName
        private string _userName;

        [DbColumn("username", Length = 64)]
        public string UserName {
            get { return _userName; }
            set {
                if (_userName == value) return;
                _userName = value;
                OnPropertyChanged("UserName");
                OnPropertyChanged("DisplayString");
            }
        }
        #endregion

        #region FullName
        private string _fullName;

        [DbColumn("fullname", AllowDbNull = true, Length = 256)]
        public string FullName {
            get { return _fullName; }
            set {
                if (_fullName == value) return;
                _fullName = value;
                OnPropertyChanged("FullName");
                OnPropertyChanged("DisplayString");
            }
        }
        #endregion

        #region Salt
        [DbColumn("salt", AllowDbNull = true, Length = 32)]
        public string Salt { get; set; }
        #endregion

        #region IsAdmin
        private bool _isAdmin;

        [DbColumn("is_admin", AllowDbNull = false)]
        public bool IsAdmin {
            get { return _isAdmin; }
            set {
                _isAdmin = value;
                OnPropertyChanged("IsAdmin");
            }
        }
        #endregion

        #region IsInitialized
        private bool _isInitialized;

        [DbColumn("is_initialized", AllowDbNull = false)]
        public bool IsInitialized {
            get { return _isInitialized; }
            set {
                _isInitialized = value;
                OnPropertyChanged("IsInitialized");
            }
        }
        #endregion

        #region PasswordHash
        [DbColumn("password", AllowDbNull = false, Length = 1024)]
        public string PasswordHash { get; set; }
        #endregion

        #region DisplayString
        public string DisplayString {
            get {
                var ds = UserName;
                if (string.IsNullOrEmpty(ds)) return "<Noch kein Name eingegeben>";
                if (ds.Length == 0) {
                    ds = FullName;
                    if (string.IsNullOrEmpty(ds)) return "<Noch kein Name eingegeben>";
                }
                else if (!string.IsNullOrEmpty(FullName)) ds += " (" + FullName + ")";
                return ds;
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