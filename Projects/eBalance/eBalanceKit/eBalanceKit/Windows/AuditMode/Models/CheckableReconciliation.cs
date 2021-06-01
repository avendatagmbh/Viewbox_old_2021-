// --------------------------------------------------------------------------------
// author: Mirko Dibbert
// since: 2012-05-23
// copyright 2012 AvenDATA GmbH
// --------------------------------------------------------------------------------
using System.Collections.Generic;
using System.Linq;
using Utils;
using eBalanceKitBusiness.Reconciliation.Interfaces.ReconciliationTypes;

namespace eBalanceKit.Windows.AuditMode.Models {
    public class CheckableReconciliation : NotifyPropertyChangedBase {
        public CheckableReconciliation(IReconciliation reconciliation) {
            Reconciliation = reconciliation;
            Transactions = new List<CheckableReconciliationTransaction>();
        }

        public IReconciliation Reconciliation { get; private set; }
        public List<CheckableReconciliationTransaction> Transactions { get; private set; }

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

        #region IsChecked
        private bool? _isChecked = false;

        public bool? IsChecked {
            get { return _isChecked; }
            set {
                if (_isChecked == value) return;
                _isChecked = value;
                OnPropertyChanged("IsChecked");

                if (!_isChecked.HasValue) return;
                bool val = _isChecked.Value;
                foreach (var transaction in Transactions)
                    transaction.IsChecked = val;
            }
        }
        #endregion // IsChecked

        public void UpdateIsChecked() {
            if (Transactions.All(t => t.IsChecked)) _isChecked = true;
            else if (Transactions.All(t => !t.IsChecked)) _isChecked = false;
            else _isChecked = null;
            OnPropertyChanged("IsChecked");
        }
    }
        
}