using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;

namespace eBalanceKitBase {
    public class ProxyConfig : INotifyPropertyChanged {

        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged(string property) {
            if (PropertyChanged != null) PropertyChanged(this, new PropertyChangedEventArgs(property));
        }

        private string _host;
        public string Host {
            get { return _host; }
            set {
                _host = value;
                OnPropertyChanged("Host");
            }
        }

        private string _port;
        public string Port {
            get { return _port; }
            set {
                _port = value;
                OnPropertyChanged("Port");
            }
        }

        private string _username;
        public string Username {
            get { return _username; }
            set {
                _username = value;
                OnPropertyChanged("Username");
            }
        }

        private string _password;
        public string Password {
            get { return _password; }
            set {
                _password = value;
                OnPropertyChanged("Password");
            }
        }
    }
}
