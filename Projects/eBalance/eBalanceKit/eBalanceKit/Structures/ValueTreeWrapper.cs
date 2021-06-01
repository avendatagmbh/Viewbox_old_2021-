// --------------------------------------------------------------------------------
// author: Mirko Dibbert
// since:  2011-07-14
// copyright 2011 AvenDATA GmbH
// --------------------------------------------------------------------------------
using System.ComponentModel;
using Utils;
using eBalanceKitBusiness.Structures.ValueTree;

namespace eBalanceKit.Structures {
    public class ValueTreeWrapper : NotifyPropertyChangedBase {
        private ValueTreeNode _valueTreeRoot;

        public ValueTreeNode ValueTreeRoot {
            get { return _valueTreeRoot; }
            set {
                if (_valueTreeRoot != value) {
                    _valueTreeRoot = value;
                    OnPropertyChanged("ValueTreeRoot");
                }
            }
        }
    }
}