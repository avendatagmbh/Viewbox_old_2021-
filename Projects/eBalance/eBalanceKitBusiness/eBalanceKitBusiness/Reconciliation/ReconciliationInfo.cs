// --------------------------------------------------------------------------------
// author: Mirko Dibbert
// since:  2011-06-03 
// (based on TransferHBST class / redesigned on 2012-03-24)
// copyright 2011 AvenDATA GmbH
// --------------------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using Utils;
using eBalanceKitBase.Structures;
using eBalanceKitBusiness.Exceptions;
using eBalanceKitBusiness.Localisation;
using eBalanceKitBusiness.Reconciliation.Enums;
using eBalanceKitBusiness.Reconciliation.Interfaces;
using eBalanceKitBusiness.Reconciliation.ReconciliationTypes;
using eBalanceKitBusiness.Structures.DbMapping;
using eBalanceKitBusiness.Structures.XbrlElementValue;

namespace eBalanceKitBusiness.Reconciliation {
    /// <summary>
    /// 
    /// </summary>
    public class ReconciliationInfo : NotifyPropertyChangedBase, IReconciliationInfo {
        internal ReconciliationInfo(Document document, IValueTreeEntry value, List<IReconciliationTransaction> assignedTransactions = null) {

            Document = document;
            Value = value;
            if (assignedTransactions != null) {

                _transactions =
                    new ObservableCollection<IReconciliationTransaction>(
                        (from t in assignedTransactions
                         orderby t.Reconciliation.ReconciliationType , t.Reconciliation.Name
                         select t));

                _transactions.CollectionChanged += TransactionsCollectionChanged;
                assignedTransactions.ForEach(transaction => transaction.PropertyChanged += TransactionPropertyChanged);

            } else {
                _transactions = new ObservableCollection<IReconciliationTransaction>();
                _transactions.CollectionChanged += TransactionsCollectionChanged;
            }

            ComputedValueTransfer = new ComputedValue();
            ComputedValueTransfer.PropertyChanged += ComputedValueTransferPropertyChanged;

            ComputedValueTransferPreviousYear = new ComputedValue();
            ComputedValueTransferPreviousYear.PropertyChanged += ComputedValueTransferPreviousYearPropertyChanged;

            ComputedValueTransferPreviousYearCorrection = new ComputedValue();
            ComputedValueTransferPreviousYearCorrection.PropertyChanged += ComputedValueTransferPreviousYearCorrectionPropertyChanged;

            ComputeTransactionSums();

            // assign previous year value
            var previousYearValues = ((PreviousYearValues)Document.ReconciliationManager.PreviousYearValues);
            var previousYearTransaction = previousYearValues.GetTransaction(Value.Element);
            if (previousYearTransaction != null)
                ComputedValueTransferPreviousYear.ManualValue = previousYearTransaction.Value;
            
            // assign audit correction sum for previous year
            var previousYearCorrectionValues = ((AuditCorrectionPreviousYear)Document.ReconciliationManager.AuditCorrectionsPreviousYear);
            var previousYearCorrectionTransaction = previousYearCorrectionValues.GetTransaction(Value.Element);
            if (previousYearCorrectionTransaction != null)
                ComputedValueTransferPreviousYearCorrection.ManualValue = previousYearCorrectionTransaction.Value;           
        }

        private void ComputeTransactionSums() { ComputedValueTransfer.Transactions = Transactions; }

        #region events
        internal event EventHandler<TransactionChangedEventArgs> TransactionAdded;
        private void OnTransactionAdded(IReconciliationTransaction transaction) { if (TransactionAdded != null) TransactionAdded(this, new TransactionChangedEventArgs(transaction)); }

        internal event EventHandler<TransactionChangedEventArgs> TransactionRemoved;
        private void OnTransactionRemoved(IReconciliationTransaction transaction) { if (TransactionRemoved != null) TransactionRemoved(this, new TransactionChangedEventArgs(transaction)); }

        internal event EventHandler<TransactionChangedEventArgs> TransactionSelected;
        internal void OnTransactionSelected(IReconciliationTransaction transaction) { if (TransactionSelected != null) TransactionSelected(this, new TransactionChangedEventArgs(transaction)); }
        #endregion // events

        #region event handler
        private void TransactionPropertyChanged(object sender, PropertyChangedEventArgs propertyChangedEventArgs) {
            ComputeTransactionSums();
            
            if (propertyChangedEventArgs.PropertyName == "IsSelected") OnPropertyChanged("HasSelectedTransaction");
            else if (propertyChangedEventArgs.PropertyName == "IsReconciliationSelected") OnPropertyChanged("IsAssignedToSelectedReconciliation");
        }

        void TransactionsCollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e) {
            ComputeTransactionSums();
            OnPropertyChanged("HasSelectedTransaction");
            OnPropertyChanged("IsAssignedToSelectedReconciliation");
        }

        private void ComputedValueTransferPropertyChanged(object sender, PropertyChangedEventArgs e) {
            if (e.PropertyName == "Value") {
                OnPropertyChanged("TransferValue");
                OnPropertyChanged("TransferValueDisplayString");

                OnPropertyChanged("STValue");
                OnPropertyChanged("STValueDisplayString");
            }

            if (e.PropertyName == "HasComputedValue") {
                OnPropertyChanged("HasComputedValue");
            }
        }

        private void ComputedValueTransferPreviousYearPropertyChanged(object sender, PropertyChangedEventArgs e) {
            if (e.PropertyName == "Value") {
                OnPropertyChanged("TransferValuePreviousYear");
                OnPropertyChanged("TransferValuePreviousYearDisplayString");

                OnPropertyChanged("STValue");
                OnPropertyChanged("STValueDisplayString");
            }

            if (e.PropertyName == "HasComputedValue") {
                OnPropertyChanged("HasComputedValue");
            }

        }

        private void ComputedValueTransferPreviousYearCorrectionPropertyChanged(object sender, PropertyChangedEventArgs e) {
            if (e.PropertyName == "Value") {
                OnPropertyChanged("TransferValuePreviousYearCorrection");
                OnPropertyChanged("TransferValuePreviousYearCorrectionDisplayString");

                OnPropertyChanged("STValue");
                OnPropertyChanged("STValueDisplayString");
            }

            if (e.PropertyName == "HasComputedValue") {
                OnPropertyChanged("HasComputedValue");
            }

        }

        private void XbrlElementValuePropertyChanged(object sender, PropertyChangedEventArgs e) {
            if (e.PropertyName == "Value") {
                OnPropertyChanged("HBValue");
                OnPropertyChanged("HBValueDisplayString");

                OnPropertyChanged("STValue");
                OnPropertyChanged("STValueDisplayString");
            }
        }

        #endregion event handler

        #region properties

        protected IValueTreeEntry Value { get; set; }
        protected Document Document { get; set; }

        #region Transactions
        private readonly ObservableCollection<IReconciliationTransaction> _transactions;

        public IEnumerable<IReconciliationTransaction> Transactions { get { return _transactions; } }
        #endregion // Transactions

        #region HasSelectedTransaction
        public bool HasSelectedTransaction { get { return _transactions.Any(t => t.IsSelected); } }
        #endregion // HasSelectedTransaction

        #region IsAssignedToSelectedReconciliation
        public bool IsAssignedToSelectedReconciliation { get { return _transactions.Any(t => t.IsReconciliationSelected); } }
        #endregion // IsAssignedToSelectedReconciliation
        
        #region ComputedValueTransfer
        private ComputedValue _computedValueTransfer;

        public ComputedValue ComputedValueTransfer {
            get { return _computedValueTransfer; }
            set {
                _computedValueTransfer = value;
                OnPropertyChanged("ComputedValueTransfer");
            }
        }
        #endregion

        #region ComputedValueTransferPreviousYear
        private ComputedValue _computedValueTransferPreviousYear;

        public ComputedValue ComputedValueTransferPreviousYear {
            get { return _computedValueTransferPreviousYear; }
            set {
                _computedValueTransferPreviousYear = value;
                OnPropertyChanged("ComputedValueTransferPreviousYear");
            }
        }
        #endregion // ComputedValueTransferPreviousYear

        #region ComputedValueTransferPreviousYearCorrection
        private ComputedValue _computedValueTransferPreviousYearCorrection;

        public ComputedValue ComputedValueTransferPreviousYearCorrection {
            get { return _computedValueTransferPreviousYearCorrection; }
            set {
                _computedValueTransferPreviousYearCorrection = value;
                OnPropertyChanged("ComputedValueTransferPreviousYearCorrection");
            }
        }
        #endregion

        #region XbrlElementValue
        private XbrlElementValueBase _xbrlElementValue;

        public XbrlElementValueBase XbrlElementValue {
            get { return _xbrlElementValue; }
            internal set {
                if (_xbrlElementValue != null)
                    _xbrlElementValue.PropertyChanged -= XbrlElementValuePropertyChanged;
                _xbrlElementValue = value;
                if (_xbrlElementValue != null && _xbrlElementValue.DbValue != null) {
                    _xbrlElementValue.PropertyChanged += XbrlElementValuePropertyChanged;
                }

                OnPropertyChanged("HBValue");
                OnPropertyChanged("HBValueDisplayString");

                OnPropertyChanged("STValue");
                OnPropertyChanged("STValueDisplayString");

                OnPropertyChanged("HasComputedValue");

                OnPropertyChanged("TransferValue");
                OnPropertyChanged("TransferValueDisplayString");

                OnPropertyChanged("TransferValuePreviousYear");
                OnPropertyChanged("TransferValuePreviousYearDisplayString");

                OnPropertyChanged("TransferValuePreviousYearCorrection");
                OnPropertyChanged("TransferValuePreviousYearCorrectionDisplayString");
            }
        }
        #endregion // XbrlElementValue

        #region TransferValue
        public decimal? TransferValue {
            get { 
                //return ComputedValueTransfer.Value;

                if (Document.ReconciliationMode == ReconciliationMode.General) {
                    return ComputedValueTransfer.Value; 
                } else {
                    if (!STValue.HasValue && !HBValue.HasValue && !HBValue.HasValue && !TransferValuePreviousYearCorrection.HasValue)
                        return null;

                    return (STValue.HasValue ? STValue.Value : 0) -
                                    (HBValue.HasValue ? HBValue.Value : 0) -
                                    (TransferValuePreviousYear.HasValue ? TransferValuePreviousYear.Value : 0) -
                                    (TransferValuePreviousYearCorrection.HasValue ? TransferValuePreviousYearCorrection.Value : 0);
                }
            }
        }

        public string TransferValueDisplayString {
            get { return GetDisplayString(TransferValue); }
        }
        #endregion

        #region TransferValuePreviousYear
        public decimal? TransferValuePreviousYear { get { return ComputedValueTransferPreviousYear.Value; } }
        public string TransferValuePreviousYearDisplayString { get { return GetDisplayString(TransferValuePreviousYear); } }
        #endregion // TransferValuePreviousYear

        #region TransferValueInputPreviousYear
        public decimal? TransferValueInputPreviousYear {
            get { return ComputedValueTransferPreviousYear.ManualValue; }
            set {

                if (!Document.ReportRights.WriteTransferValuesAllowed)
                    throw new AccessDeniedException(ExceptionMessages.InsufficentWriteRights);
                
                if (//(!ComputedValueTransferPreviousYear.ManualValue.HasValue && value == null) ||
                    (value.HasValue && ComputedValueTransferPreviousYear.ManualValue.HasValue && ComputedValueTransferPreviousYear.ManualValue.Value ==
                    Math.Round(Convert.ToDecimal(value), 2, MidpointRounding.AwayFromZero)))
                    return;

                if (value == null || value.Value.ToString(CultureInfo.InvariantCulture).Length == 0) ComputedValueTransferPreviousYear.ManualValue = null;
                else
                    ComputedValueTransferPreviousYear.ManualValue = Math.Round(Convert.ToDecimal(value), 2,
                                                                               MidpointRounding.AwayFromZero);

                var previousYearValues = ((PreviousYearValues)Document.ReconciliationManager.PreviousYearValues);
                var transaction = previousYearValues.GetTransaction(Value.Element);

                if (value == null) {
                    if (transaction != null) transaction.Remove();
                } else {
                    if (transaction == null) {
                        previousYearValues.AddTransaction(Value.Element);
                        transaction = previousYearValues.GetTransaction(Value.Element);
                        transaction.ReconciliationInfo = this;
                    }
                    transaction.Value = ComputedValueTransferPreviousYear.ManualValue;
                }

                OnPropertyChanged("TransferValueInputPreviousYear");
                if (Document.ReconciliationMode == ReconciliationMode.TaxBalanceSheetValues) {
                    OnPropertyChanged("TransferValue");
                    OnPropertyChanged("TransferValueDisplayString");
                }
            }
        }
        #endregion // TransferValueInputPreviousYear

        #region TransferValuePreviousYearCorrection
        public decimal? TransferValuePreviousYearCorrection { get { return ComputedValueTransferPreviousYearCorrection.Value; } }
        public string TransferValuePreviousYearCorrectionDisplayString { get { return GetDisplayString(TransferValuePreviousYearCorrection); } }
        #endregion // TransferValuePreviousYearCorrection

        #region TransferValueInputPreviousYearCorrection
        public decimal? TransferValueInputPreviousYearCorrection {
            get { return ComputedValueTransferPreviousYearCorrection.ManualValue; }
            set {

                if (!Document.ReportRights.WriteTransferValuesAllowed)
                    throw new AccessDeniedException(ExceptionMessages.InsufficentWriteRights);

                if (//(!ComputedValueTransferPreviousYear.ManualValue.HasValue && value == null) ||
                    (value.HasValue && ComputedValueTransferPreviousYearCorrection.ManualValue.HasValue && ComputedValueTransferPreviousYearCorrection.ManualValue.Value ==
                    Math.Round(Convert.ToDecimal(value), 2, MidpointRounding.AwayFromZero)))
                    return;

                if (value == null || value.Value.ToString(CultureInfo.InvariantCulture).Length == 0) ComputedValueTransferPreviousYearCorrection.ManualValue = null;
                else
                    ComputedValueTransferPreviousYearCorrection.ManualValue = Math.Round(Convert.ToDecimal(value), 2,
                                                                               MidpointRounding.AwayFromZero);

                var previousYearValues = ((DeltaReconciliation)Document.ReconciliationManager.AuditCorrectionsPreviousYear);
                var transaction = previousYearValues.GetTransaction(Value.Element);

                if (value == null) {
                    if (transaction != null) transaction.Remove();
                } else {
                    if (transaction == null) {
                        previousYearValues.AddTransaction(Value.Element);
                        transaction = previousYearValues.GetTransaction(Value.Element);
                        transaction.ReconciliationInfo = this;
                    }
                    transaction.Value = ComputedValueTransferPreviousYearCorrection.ManualValue;
                }

                OnPropertyChanged("TransferValueInputPreviousYearCorrection");
            }
        }
        #endregion // TransferValueInputPreviousYear

        #region HasTransferValue
        public bool HasTransferValue {
            get {
                return (TransferValue != null && TransferValue != 0) ||
                       (TransferValuePreviousYear != null && TransferValuePreviousYear != 0);
            }
        }
        #endregion

        #region HBValue
        public decimal? HBValue {
            get { return XbrlElementValue == null ? null : XbrlElementValue.HasValue ? (decimal?)XbrlElementValue.DecimalValue : null; }
        }

        public string HBValueDisplayString {
            get { return GetDisplayString(HBValue); }
        }
        #endregion

        #region STValue
        public decimal? STValue {
            get {
                //if (!HBValue.HasValue && 
                //    !TransferValuePreviousYear.HasValue && 
                //    !TransferValue.HasValue && 
                //    !TransferValuePreviousYearCorrection.HasValue) return null;

                //decimal value =
                //    (HBValue.HasValue ? HBValue.Value : 0) +
                //    (TransferValuePreviousYear.HasValue ? TransferValuePreviousYear.Value : 0) +
                //    (TransferValuePreviousYearCorrection.HasValue ? TransferValuePreviousYearCorrection.Value : 0) +
                //    (TransferValue.HasValue ? TransferValue.Value : 0);

                decimal? value;
                if (Document.ReconciliationMode == ReconciliationMode.General) {
                    if (!HBValue.HasValue &&
                    !TransferValuePreviousYear.HasValue &&
                    !TransferValue.HasValue &&
                    !TransferValuePreviousYearCorrection.HasValue) return null;

                    value = (HBValue.HasValue ? HBValue.Value : 0) +
                            (TransferValuePreviousYear.HasValue ? TransferValuePreviousYear.Value : 0) +
                            (TransferValuePreviousYearCorrection.HasValue ? TransferValuePreviousYearCorrection.Value : 0) +
                            (TransferValue.HasValue ? TransferValue.Value : 0);
                } else {
                    value = ComputedValueTransfer.Value;// ?? 0;
                }
                return value;
            }
        }

        public string STValueDisplayString {
            get { return GetDisplayString(STValue); }
        }
        #endregion

        #region HasComputedValue
        public bool HasComputedValue {
            get { return ComputedValueTransfer.HasComputedValue || ComputedValueTransferPreviousYear.HasComputedValue || ComputedValueTransferPreviousYearCorrection.HasComputedValue; }
        }
        #endregion HasComputedValue
        
        #endregion properties

        #region GetDisplayString
        public string GetDisplayString(decimal? value) { return value.HasValue ? LocalisationUtils.CurrencyToString(value.Value) : "-"; }
        #endregion

        #region AddTransaction
        internal void AddTransaction(IReconciliationTransaction transaction) {
            var tmp = new List<IReconciliationTransaction>(_transactions);
            tmp.Add(transaction);
            _transactions.Clear();
            foreach (var t in (from t in tmp orderby t.Reconciliation.ReconciliationType, t.Reconciliation.Name select t)) {
                _transactions.Add(t);
            }
            transaction.PropertyChanged += TransactionPropertyChanged;
            OnTransactionAdded(transaction);
        }
        #endregion // AddTransaction
        
        #region RemoveTransaction
        internal void RemoveTransaction(IReconciliationTransaction transaction) {
            _transactions.Remove(transaction);
            transaction.PropertyChanged -= TransactionPropertyChanged;
            OnTransactionRemoved(transaction);
        }
        #endregion // RemoveTransactio

        public bool IsValid {
            get {
                if (!HasTransferValue) {
                    return true;
                }

                var validCurrYear = ComputedValueTransfer.HasComputedValue &&
                         (!ComputedValueTransfer.ManualComputedValue.HasValue &&
                          !ComputedValueTransfer.ManualValue.HasValue &&
                          !ComputedValueTransfer.BalanceListEntrySum.HasValue);

                validCurrYear |= !ComputedValueTransfer.HasComputedValue &&
                         (ComputedValueTransfer.ManualComputedValue.HasValue ||
                          ComputedValueTransfer.ManualValue.HasValue ||
                          ComputedValueTransfer.BalanceListEntrySum.HasValue);

                validCurrYear |= !ComputedValueTransfer.HasComputedValue &&
                         !ComputedValueTransfer.ManualComputedValue.HasValue &&
                          !ComputedValueTransfer.ManualValue.HasValue &&
                          !ComputedValueTransfer.BalanceListEntrySum.HasValue;

                var validPrevYear = ComputedValueTransferPreviousYear.HasComputedValue &&
                         (!ComputedValueTransferPreviousYear.ManualComputedValue.HasValue &&
                          !ComputedValueTransferPreviousYear.ManualValue.HasValue &&
                          !ComputedValueTransferPreviousYear.BalanceListEntrySum.HasValue);

                validPrevYear |= !ComputedValueTransferPreviousYear.HasComputedValue &&
                         (ComputedValueTransferPreviousYear.ManualComputedValue.HasValue ||
                          ComputedValueTransferPreviousYear.ManualValue.HasValue ||
                          ComputedValueTransferPreviousYear.BalanceListEntrySum.HasValue);
                
                validPrevYear |= !ComputedValueTransferPreviousYear.HasComputedValue &&
                         !ComputedValueTransferPreviousYear.ManualComputedValue.HasValue &&
                          !ComputedValueTransferPreviousYear.ManualValue.HasValue &&
                          !ComputedValueTransferPreviousYear.BalanceListEntrySum.HasValue;

                return validCurrYear && validPrevYear;
            }
        }

        public void RefreshValues() {
            OnPropertyChanged("TransferValue");
            OnPropertyChanged("TransferValueInputPreviousYear");
            OnPropertyChanged("STValue");

            OnPropertyChanged("TransferValueDisplayString");
            OnPropertyChanged("TransferValueInputPreviousYearDisplayString");
            OnPropertyChanged("STValueDisplayString");
        }
    }
}