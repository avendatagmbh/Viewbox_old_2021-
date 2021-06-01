using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Utils;

namespace eBalanceKitBase.Structures.Backup {
    public class Week : NotifyPropertyChangedBase{
        #region Constructor
        internal Week() {
        }
        #endregion Constructor

        #region Properties
        private bool []_day = new bool[7];
        #endregion Properties

        #region Methods
        public bool this[int index] {
            get { return _day[index]; }
            set { 
                _day[index] = value;
                OnPropertyChanged("Item[]");
            }
        }
        #endregion Methods
    }
}
