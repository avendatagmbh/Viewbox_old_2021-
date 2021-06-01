// --------------------------------------------------------------------------------
// author: Mirko Dibbert
// since: 2012-05-23
// copyright 2012 AvenDATA GmbH
// --------------------------------------------------------------------------------
using Utils;
using eBalanceKitBusiness.Reconciliation.Interfaces;

namespace eBalanceKit.Windows.AuditMode.Models {
    public class CheckableReconciliationTransaction : NotifyPropertyChangedBase {

        public CheckableReconciliationTransaction(IReconciliationTransaction transaction, CheckableReconciliation parent) {
            Transaction = transaction;
            Parent = parent;
        }

        public IReconciliationTransaction Transaction { get; private set; }
        public CheckableReconciliation Parent { get; private set; }

        #region IsExpanded
        private bool _isExpanded;

        public bool IsExpanded {
            get { return _isExpanded; }
            set {
                if (_isExpanded == value) return;
                _isExpanded = value;
                OnPropertyChanged("IsExpanded");
            }
        }
        #endregion // IsExpanded

        #region IsChecked
        private bool _isChecked;

        public bool IsChecked {
            get { return _isChecked; }
            set {
                if (_isChecked == value) return;
                _isChecked = value;
                OnPropertyChanged("IsChecked");
                Parent.UpdateIsChecked();
            }
        }
        #endregion // IsChecked
    }
}