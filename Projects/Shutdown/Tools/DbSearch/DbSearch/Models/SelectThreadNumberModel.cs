using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Utils;

namespace DbSearch.Models {
    public class SelectThreadNumberModel : NotifyPropertyChangedBase{
        #region Constructor
        public SelectThreadNumberModel() { ThreadCount = 4; }
        #endregion Constructor

        #region Properties
        private int _threadCount;
        public int ThreadCount { 
            get { return _threadCount; } 
            set {
                if (_threadCount != value) {
                    _threadCount = value;
                    OnPropertyChanged("ThreadCount");
                }
            } 
        }

        #endregion Properties

        #region Methods
        #endregion Methods

        
    }
}
