using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using SystemDb;
using SystemDb.Internal;
using ViewboxAdmin_ViewModel;

namespace ViewboxAdmin.ViewModels.Users
{
    class UserModel : NotifyBase,IDataErrorInfo
    {
        public UserModel(IUser user)  {
            
            UserName = user.UserName;
            Name = user.Name;
            Email = user.Email;
            Flags = user.Flags;
            IsADUser = user.IsADUser;
            DisplayRowCount = user.DisplayRowCount;
            Domain = user.Domain;
            Id = user.Id;
            


        }

        public UserModel Clone() { 
            var result = new UserModel();
            result.UserName = this.UserName;
            result.Name = this.Name;
            result.Email = this.Email;
            result.Flags = this.Flags;
            result.IsADUser = this.IsADUser;
            result.DisplayRowCount = this.DisplayRowCount;
            result.Domain = this.Domain;
            return result;
        } 

        public UserModel() {
            this.UserName = String.Empty;
            this.Name = String.Empty;
            this.Email = String.Empty;
            this.Flags = SpecialRights.None;
            this.IsADUser = false;
            this.DisplayRowCount = 0;
            this.Domain = string.Empty;
            this.Password = string.Empty;
        }

        #region UserName
        private string _username;

        public string UserName {
            get { return _username; }
            set {
                if (_username != value) {
                    _username = value;
                    OnPropertyChanged("UserName");
                }
            }
        }
        #endregion UserName

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

        #region Email
        private string _email;

        public string Email {
            get { return _email; }
            set {
                if (_email != value) {
                    _email = value;
                    OnPropertyChanged("Email");
                }
            }
        }
        #endregion Email

        #region Flags
        private SpecialRights _flags;

        public SpecialRights Flags {
            get { return _flags; }
            set {
                if (_flags != value) {
                    _flags = value;
                    OnPropertyChanged("Flags");
                }
            }
        }
        #endregion Flags

        public IEnumerable<SpecialRights> SpecialRightsValue {
            get {
                return Enum.GetValues(typeof(SpecialRights))
                    .Cast<SpecialRights>();
            }
        }

        #region IsADUser
        private bool _isADUser;

        public bool IsADUser {
            get { return _isADUser; }
            set {
                if (_isADUser != value) {
                    _isADUser = value;
                    OnPropertyChanged("IsADUser");
                }
            }
        }
        #endregion IsADUser

        #region DisplayRowCount
        private int _displayrowcount;

        public int DisplayRowCount {
            get { return _displayrowcount; }
            set {
                if (_displayrowcount != value) {
                    _displayrowcount = value;
                    OnPropertyChanged("DisplayRowCount");
                }
            }
        }
        #endregion DisplayRowCount

        #region Domain
        private string _domain;

        public string Domain {
            get { return _domain; }
            set {
                if (_domain != value) {
                    _domain = value;
                    OnPropertyChanged("Domain");
                }
            }
        }
        #endregion Domain

        #region Password
        private string _password;

        public string Password {
            get { return _password; }
            set {
                if (_password != value) {
                    _password = value;
                    OnPropertyChanged("Password");
                }
            }
        }
        #endregion Password

        #region Id
        private int _id;

        public int Id {
            get { return _id; }
            set {
                if (_id != value) {
                    _id = value;
                    OnPropertyChanged("Id");
                }
            }
        }
        #endregion Id

        public string Error {
            get { throw new NotImplementedException(); }
        }

        public string this[string columnName] {
            get {
                string result = null;
                switch(columnName) {
                    case ("UserName"): {
                        if(String.IsNullOrEmpty(UserName))
                            result = "Please provide a user name";
                        break;
                    }
                    case("Name"): {
                        if (String.IsNullOrEmpty(Name))
                            result = "Please provide a name";
                        break;
                    }
                }
                return result;

            }
        }
    }
}
