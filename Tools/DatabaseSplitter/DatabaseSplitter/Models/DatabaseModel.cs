using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Utils;

namespace DatabaseSplitter.Models {
    public class DatabaseModel : NotifyPropertyChangedBase {
        #region User
        private string _user;

        public string User {
            get { return _user; }
            set {
                if (_user != value) {
                    _user = value;
                    OnPropertyChanged("User");
                }
            }
        }
        #endregion User

        #region Host
        private string _host;

        public string Host {
            get { return _host; }
            set {
                if (_host != value) {
                    _host = value;
                    OnPropertyChanged("Host");
                }
            }
        }
        #endregion Host

        #region Database
        private string _database;

        public string Database {
            get { return _database; }
            set {
                if (_database != value) {
                    _database = value;
                    OnPropertyChanged("Database");
                }
            }
        }
        #endregion Database 

        #region DatabaseAnalyser
        private string _databaseAnalyser;

        public string DatabaseAnalyser {
            get { return _databaseAnalyser; }
            set {
                if (_databaseAnalyser != value) {
                    _databaseAnalyser = value;
                    OnPropertyChanged("DatabaseAnalyser");
                }
            }
        }
        #endregion DatabaseAnalyser

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
    }
}
