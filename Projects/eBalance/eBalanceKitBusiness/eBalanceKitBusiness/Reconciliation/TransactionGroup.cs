// --------------------------------------------------------------------------------
// author: Mirko Dibbert
// since: 2012-04-02
// copyright 2012 AvenDATA GmbH
// --------------------------------------------------------------------------------
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using Utils;
using eBalanceKitBase.Structures;
using eBalanceKitBusiness.Reconciliation.Interfaces;
using eBalanceKitResources.Localisation;

namespace eBalanceKitBusiness.Reconciliation {
    internal class TransactionGroup : NotifyPropertyChangedBase, ITransactionGroup {

        #region constructor
        public TransactionGroup(string label) { Label = label; }
        #endregion // constructor
        
        #region Label
        public string Label { get; private set; }
        #endregion // Label
        
        #region Transactions
        private readonly ObservableCollectionAsync<IReconciliationTransaction> _transactions =
            new ObservableCollectionAsync<IReconciliationTransaction>();

        public IEnumerable<IReconciliationTransaction> Transactions { get { return _transactions; } }
        #endregion // Transactions

        #region Sum
        public decimal? Sum {
            get {
                decimal? sum = null;
                foreach (var transaction in Transactions.Where(transaction => transaction.Value.HasValue)) {
                    Debug.Assert(transaction.Value != null, "transaction.Value != null");
                    if (sum == null) sum = transaction.Value.Value;
                    else sum += transaction.Value.Value;
                }
                return sum;
            }
        }
        #endregion // Sum

        #region SumDisplayString
        public string SumDisplayString { get { return " (" + ResourcesReconciliation.Sum + (Sum.HasValue ? LocalisationUtils.DecimalToString(Sum.Value) : "-") + " €)"; } }
        #endregion // SumDisplayString

        #region IsVisible
        public bool IsVisible { get { return _transactions.Count > 0; } }
        #endregion // IsVisible

        #region AddTransaction
        internal void AddTransaction(IReconciliationTransaction transaction, bool setIsExpanded = true) {
            transaction.PropertyChanged += TransactionOnPropertyChanged;
            _transactions.Add(transaction);
            OnPropertyChanged("Sum");
            OnPropertyChanged("SumDisplayString");
            OnPropertyChanged("IsVisible");

            if (setIsExpanded) IsExpanded = true;
        }
        #endregion // AddTransaction
        
        #region RemoveTransaction
        internal void RemoveTransaction(IReconciliationTransaction transaction) {
            transaction.PropertyChanged -= TransactionOnPropertyChanged;
            _transactions.Remove(transaction);
            OnPropertyChanged("Sum");
            OnPropertyChanged("SumDisplayString");
            OnPropertyChanged("IsVisible");
        }
        #endregion // RemoveTransaction
        
        #region TransactionOnPropertyChanged
        private void TransactionOnPropertyChanged(object sender, PropertyChangedEventArgs e) {
            if (e.PropertyName == "Value") {
                OnPropertyChanged("Sum");
                OnPropertyChanged("SumDisplayString");
            }
        }
        #endregion // TransactionOnPropertyChanged

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
    }
}