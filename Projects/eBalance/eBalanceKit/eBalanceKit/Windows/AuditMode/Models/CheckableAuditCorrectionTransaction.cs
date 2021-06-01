// --------------------------------------------------------------------------------
// author: Mirko Dibbert
// since: 2012-05-24
// copyright 2012 AvenDATA GmbH
// --------------------------------------------------------------------------------
using Utils;
using eBalanceKitBusiness.AuditCorrections;

namespace eBalanceKit.Windows.AuditMode.Models {
    public class CheckableAuditCorrectionTransaction : NotifyPropertyChangedBase {

        public CheckableAuditCorrectionTransaction(IAuditCorrectionTransaction transaction, CheckableAuditCorrection parent) {
            Transaction = transaction;
            Parent = parent;
        }

        public IAuditCorrectionTransaction Transaction { get; private set; }
        public CheckableAuditCorrection Parent { get; private set; }

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