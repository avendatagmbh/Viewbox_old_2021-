// --------------------------------------------------------------------------------
// author: Mirko Dibbert
// since: 2011-11-25
// copyright 2011 AvenDATA GmbH
// --------------------------------------------------------------------------------

using System;
using System.ComponentModel;

namespace LineCounter {
    public class MainWindowModel : INotifyPropertyChanged {
        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged(string propertyName) {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
        }

        #region Folders
        private string _folders;

        public string Folders {
            get { return _folders; }
            set {
                _folders = value;
                OnPropertyChanged("Folders");
            }
        }
        #endregion

        #region Result
        private string _result;

        public string Result {
            get { return _result; }
            set {
                _result = value;
                OnPropertyChanged("Result");
            }
        }
        #endregion

        public void Log(string msg) { 
            if (!string.IsNullOrEmpty(Result)) Result += Environment.NewLine;
            Result += msg;
        }
    }
}