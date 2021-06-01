// --------------------------------------------------------------------------------
// author: Mirko Dibbert
// since: 2012-04-20
// copyright 2012 AvenDATA GmbH
// --------------------------------------------------------------------------------
using Utils;
namespace eBalanceKitBusiness.Reconciliation {
    public class TreeViewVisualOptions : NotifyPropertyChangedBase {

        #region EnterPreviousYearValues
        private bool _enterPreviousYearValues;

        public bool EnterPreviousYearValues {
            get { return _enterPreviousYearValues; }
            set {
                _enterPreviousYearValues = value;
                OnPropertyChanged("EnterPreviousYearValues");
            }
        }
        #endregion // EnterPreviousYearValues

        #region ShowCommercialBalanceValues
        private bool _showCommercialBalanceValues = true;

        public bool ShowCommercialBalanceValues {
            get { return _showCommercialBalanceValues; }
            set {
                if (_showCommercialBalanceValues == value) return;
                _showCommercialBalanceValues = value;
                OnPropertyChanged("ShowCommercialBalanceValues");
            }
        }
        #endregion // ShowCommercialBalanceValues

        #region ShowTaxBalanceValues
        private bool _showTaxBalanceValues = true;

        public bool ShowTaxBalanceValues {
            get { return _showTaxBalanceValues; }
            set {
                if (_showTaxBalanceValues == value) return;
                _showTaxBalanceValues = value;
                OnPropertyChanged("ShowTaxBalanceValues");
            }
        }
        #endregion // ShowTaxBalanceValues

        #region ShowReconciliationValues
        private bool _showReconciliationValues = true;

        public bool ShowReconciliationValues {
            get { return _showReconciliationValues; }
            set {
                if (_showReconciliationValues == value) return;
                _showReconciliationValues = value;
                OnPropertyChanged("ShowReconciliationValues");
            }
        }
        #endregion // ShowReconciliationValues
    }
}