// --------------------------------------------------------------------------------
// author: Mirko Dibbert
// since:  2011-09-10
// copyright 2011 AvenDATA GmbH
// --------------------------------------------------------------------------------
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using eBalanceKitBusiness.Reconciliation.Interfaces;
using eBalanceKitBusiness.Reconciliation.Interfaces.ReconciliationTypes;

namespace eBalanceKitBusiness.Structures.XbrlElementValue {
    /// <summary>
    ///   This class implements monetary values for xbrl elements. Monetary values provide the ability to
    ///   compute it's value depending on the taxonomy computation rules.
    /// </summary>
    public class ComputedValue : INotifyPropertyChanged {

        internal ComputedValue() { }

        #region PropertyChanged event
        public event PropertyChangedEventHandler PropertyChanged;

        public void OnPropertyChanged(string propertyName) {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion PropertyChanged event

        #region Value
        private decimal? _value;

        /// <summary>
        /// Sum of all value parts.
        /// </summary>
        public decimal? Value {
            get { return _value; }
            private set {
                _value = value;
                OnPropertyChanged("Value");
            }
        }
        #endregion Value

        #region ManualValue
        private decimal? _manualValue;

        /// <summary>
        /// Manual correction value.
        /// </summary>
        public decimal? ManualValue {
            get { return _manualValue; }
            set {
                if (_manualValue == value)
                    return;

                _manualValue = value;
                OnPropertyChanged("ManualValue");

                UpdateValue();
            }
        }
        #endregion ManualValue

        #region ManualComputedValue
        private decimal? _manualComputedValue;

        /// <summary>
        /// Computed value, depending on "manual" computation rules. 
        /// The value equals the sum of all child nodes for which AutoComputeAllowed is true.
        /// </summary>
        public decimal? ManualComputedValue {
            get { return _manualComputedValue; }
            set {
                if (_manualComputedValue == value)
                    return;

                _manualComputedValue = value;
                OnPropertyChanged("ManualComputedValue");

                UpdateValue();
            }
        }
        #endregion ManualComputedValue

        #region TaxonomyComputedValue
        private decimal? _taxonomyComputedValue;

        /// <summary>
        /// Computed value, depending on taxonomy computation rules.
        /// </summary>
        public decimal? TaxonomyComputedValue {
            get { return _taxonomyComputedValue; }
            set {
                if (_taxonomyComputedValue == value)
                    return;

                _taxonomyComputedValue = value;
                OnPropertyChanged("TaxonomyComputedValue");
                OnPropertyChanged("HasComputedValue");

                UpdateValue();
            }
        }
        #endregion

        #region BalanceListEntrySum
        private decimal? _balanceListEntrySum;

        /// <summary>
        /// Sum of all assigned balance list entries.
        /// </summary>
        public decimal? BalanceListEntrySum {
            get { return _balanceListEntrySum; }
            set {
                if (_balanceListEntrySum == value)
                    return;

                _balanceListEntrySum = value;
                OnPropertyChanged("BalanceListEntrySum");

                UpdateValue();
            }
        }
        #endregion BalanceListEntrySum

        #region HasComputedValue
        public bool HasComputedValue { get { return TaxonomyComputedValue.HasValue; } }
        #endregion HasComputedValue

        #region Transactions
        private IEnumerable<IReconciliationTransaction> _transactions;

        internal IEnumerable<IReconciliationTransaction> Transactions {
            get { return _transactions; }
            set {
                _transactions = value;
                
                _manualValueByReconciliation.Clear();

                foreach (
                    var transactionGroup in
                        (from t in _transactions
                         group t by t.Reconciliation
                             into g
                             select new { Reconciliation = g.Key, Transactions = g }).Where(
                             transactionGroup => transactionGroup.Transactions.Any(t => t.Value.HasValue))) {
                    _manualValueByReconciliation[transactionGroup.Reconciliation] =
                        transactionGroup.Transactions.Sum(t => t.Value != null ? t.Value.Value : 0);
                }

                ManualValue = Transactions.Any(t => t.Value.HasValue)
                                  ? Transactions.Sum(t => t.Value != null ? t.Value.Value : 0)
                                  : (decimal?) null;

            }
        }
        #endregion // Transactions
        
        #region ComputedValueByReconciliation
        private readonly Dictionary<IReconciliation, decimal> _computedValueByReconciliation =
            new Dictionary<IReconciliation, decimal>();

        public Dictionary<IReconciliation, decimal> ComputedValueByReconciliation { get { return _computedValueByReconciliation; } }
        #endregion // ComputedValueByReconciliation

        #region ManualValueByReconciliation
        private readonly Dictionary<IReconciliation, decimal> _manualValueByReconciliation =
            new Dictionary<IReconciliation, decimal>();

        public Dictionary<IReconciliation, decimal> ManualValueByReconciliation { get { return _manualValueByReconciliation; } }
        #endregion // ManualValueByReconciliation

        #region methods

        #region UpdateValue
        /// <summary>
        /// Computes the value property as sum of all other value properties which are not null.
        /// </summary>
        private void UpdateValue() {
            if ((!ManualValue.HasValue &&
                 !TaxonomyComputedValue.HasValue &&
                 !ManualComputedValue.HasValue &&
                 !BalanceListEntrySum.HasValue))
                Value = null;

            else {
                var val =
                    (ManualValue.HasValue ? ManualValue.Value : 0) +
                    (TaxonomyComputedValue.HasValue ? TaxonomyComputedValue.Value : 0) +
                    (ManualComputedValue.HasValue ? ManualComputedValue.Value : 0) +
                    (BalanceListEntrySum.HasValue ? BalanceListEntrySum.Value : 0);

                Value = val;
            }

        }

        #endregion

        #endregion
    }
}