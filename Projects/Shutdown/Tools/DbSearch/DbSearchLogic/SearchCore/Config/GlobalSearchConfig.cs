using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Utils;

namespace DbSearchLogic.SearchCore.Config {
    public class GlobalSearchConfig :NotifyPropertyChangedBase {
        #region Properties
        #region MaxStringLength
        private int _maxStringLength;

        public int MaxStringLength {
            get { return _maxStringLength; }
            set {
                if (_maxStringLength != value) {
                    _maxStringLength = value;
                    OnPropertyChanged("MaxStringLength");
                }
            }
        }
        #endregion MaxStringLength
        #endregion Properties

        #region Methods
        #endregion Methods
    }
}
