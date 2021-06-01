// --------------------------------------------------------------------------------
// author: Mirko Dibbert
// since: 2012-05-23
// copyright 2012 AvenDATA GmbH
// --------------------------------------------------------------------------------
using Utils;

namespace eBalanceKit.Windows.AuditMode.Models {
    public class SelectNextYearNodeBase : NotifyPropertyChangedBase, ISelectNextYearNode {

        #region IsExpanded
        private bool _isExpanded = true;

        public bool IsExpanded {
            get { return _isExpanded; }
            set {
                if (_isExpanded == value) return;
                _isExpanded = value;
                OnPropertyChanged("IsExpanded");
            }
        }
        #endregion // IsExpanded

        #region IsSelected
        private bool _isSelected;

        public bool IsSelected {
            get { return _isSelected; }
            set {
                if (_isSelected == value) return;
                _isSelected = value;
                OnPropertyChanged("IsSelected");
            }
        }
        #endregion // IsSelected
    }
}