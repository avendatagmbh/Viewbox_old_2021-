using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Utils;

namespace Business.Structures {
    public class FileView : NotifyPropertyChangedBase{
        #region Constructor
        public FileView(string filename) {
            FullFilePath = filename;
        }
        #endregion Constructor

        #region Properties
        public string FullFilePath { get; private set; }

        #region Folder
        private string _folder;

        public string Folder {
            get { return _folder; }
            set {
                if (_folder != value) {
                    _folder = value;
                    if(!string.IsNullOrEmpty(_folder) && !_folder.EndsWith("\\"))
                        _folder += "\\";
                    OnPropertyChanged("Folder");
                    OnPropertyChanged("DisplayString");
                }
            }
        }
        #endregion Folder

        #region DisplayString
        public string DisplayString {
            get {
                if (string.IsNullOrEmpty(FullFilePath))
                    return FullFilePath;
                return FullFilePath.Replace(Folder, "");
            }
        }
        #endregion DisplayString
        #endregion Properties

        #region Methods
        #endregion Methods
    }
}
