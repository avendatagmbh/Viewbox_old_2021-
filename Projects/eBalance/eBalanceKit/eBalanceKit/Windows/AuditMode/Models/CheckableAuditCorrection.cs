// --------------------------------------------------------------------------------
// author: Mirko Dibbert
// since: 2012-05-24
// copyright 2012 AvenDATA GmbH
// --------------------------------------------------------------------------------
using System.Collections.Generic;
using System.Linq;
using Utils;
using eBalanceKitBusiness.AuditCorrections;

namespace eBalanceKit.Windows.AuditMode.Models {
    public class CheckableAuditCorrection : NotifyPropertyChangedBase {
        
        public CheckableAuditCorrection(IAuditCorrection auditCorrection) {
            AuditCorrection = auditCorrection;
            Transactions = new List<CheckableAuditCorrectionTransaction>();
        }

        public IAuditCorrection AuditCorrection { get; private set; }
        public List<CheckableAuditCorrectionTransaction> Transactions { get; private set; }

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