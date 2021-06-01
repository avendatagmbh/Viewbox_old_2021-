using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Utils;
using eBalanceKitResources.Localisation;

namespace eBalanceKitManagement.Structures {
    public class LoginUser : NotifyPropertyChangedBase {
        #region UserName
        public string UserName{get;set;}
        #endregion

        #region FullName
        public string FullName {get;set;}
        #endregion

        #region PasswordHash
        public string PasswordHash{get;set;}
        #endregion

        #region EncryptedKey
        /// <summary>
        /// Gets or sets the (encrypted) key. This key will be decrypted using the user password within 
        /// the Logon() method.
        /// </summary>
        /// <value>The key.</value>
        public string EncryptedKey { get; set; }
        #endregion

        #region IsActive
        public bool IsActive { get; set; }
        #endregion

        #region IsAdmin
        private bool _isAdmin;

        public bool IsAdmin {
            get { return _isAdmin; }
            set {
                if (_isAdmin != value) {
                    _isAdmin = value;
                    OnPropertyChanged("IsAdmin");
                }
            }
        }
        #endregion IsAdmin

        #region DisplayString
        public string DisplayString {
            get {
                string ds = this.UserName;
                if (string.IsNullOrEmpty(ds)) return ResourcesLogging.NoNameGiven;
                if (ds.Length == 0) {
                    ds = this.FullName;
                    if (string.IsNullOrEmpty(ds)) return ResourcesLogging.NoNameGiven;
                } else {
                    ds += " (" + this.FullName + ")";
                }
                return ds;
            }
        }
        #endregion
    }
}
