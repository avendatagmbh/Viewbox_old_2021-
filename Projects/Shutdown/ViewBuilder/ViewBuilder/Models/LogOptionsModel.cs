using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Utils;

namespace ViewBuilder.Models {
    public class LogOptionsModel : NotifyPropertyChangedBase{
        #region Constructor
        public LogOptionsModel() {

        }
        #endregion Constructor

        #region Properties

        private string _fileName;
        public string FileName {
            get { return _fileName; }
            set {
                if (_fileName != value) {
                    _fileName = value;
                    OnPropertyChanged("FilePath");
                }
            }
        }

        #endregion Properties

        #region Methods

        #endregion Methods

    }
}
