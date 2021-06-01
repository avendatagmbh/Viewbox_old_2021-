// --------------------------------------------------------------------------------
// author: Mirko Dibbert
// since:  2012-03-22
// copyright 2012 AvenDATA GmbH
// --------------------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using Taxonomy;
using Taxonomy.Enums;
using Taxonomy.Interfaces;
using Utils;
using eBalanceKitBase.Structures;
using eBalanceKitBusiness.Exceptions;
using eBalanceKitBusiness.Localisation;
using eBalanceKitBusiness.Logs;
using eBalanceKitBusiness.Reconciliation.DbMapping;
using eBalanceKitBusiness.Reconciliation.Enums;
using eBalanceKitBusiness.Reconciliation.Interfaces;
using eBalanceKitBusiness.Reconciliation.Interfaces.ReconciliationTypes;
using eBalanceKitBusiness.Structures;
using eBalanceKitBusiness.Structures.DbMapping;
using eBalanceKitResources.Localisation;

namespace eBalanceKitBusiness.Reconciliation {
    public class ReconciliationTransaction : NotifyPropertyChangedBase, IReconciliationTransaction {
        internal DbEntityReconciliationTransaction DbEntity { get; private set; }

        private bool _doLog = true;

        #region contructors
        /// <summary>
        ///   Creates a new transaction.
        /// </summary>
        /// <param name="document"> Assigned document. </param>
        /// <param name="reconciliation"> Assigned reconciliation. </param>
        /// <param name="reconciliationEntity"> Assigned reconciliation entity. </param>
        /// <param name="type"> Assigned transaction type. </param>
        /// <param name="position"> Assigned position (optional). </param>
        internal ReconciliationTransaction(
            Document document,
            IReconciliation reconciliation,
            DbEntityReconciliation reconciliationEntity,
            TransactionTypes type,
            IElement position = null) {
            Document = document;
            Reconciliation = reconciliation;
            DbEntity = new DbEntityReconciliationTransaction(
                document, reconciliationEntity, type,
                position == null ? 0 : Document.TaxonomyIdManager.GetId(position.Id));
            _position = Document.TaxonomyIdManager.GetElement(DbEntity.ElementId);
            reconciliationEntity.Transactions.Add(DbEntity);
            Save();
            Validate();
        }

        /// <summary>
        ///   Creates a new transaction for an existing transaction entity.
        /// </summary>
        /// <param name="document"> Assigned document. </param>
        /// <param name="reconciliation"> Assigned reconciliation. </param>
        /// <param name="transactionEntity"> Assigned transaction entity. </param>
        internal ReconciliationTransaction(
            Document document,
            IReconciliation reconciliation,
            DbEntityReconciliationTransaction transactionEntity) {
            Document = document;
            Reconciliation = reconciliation;
            DbEntity = transactionEntity;
            _position = Document.TaxonomyIdManager.GetElement(DbEntity.ElementId);
            Validate();
        }
        #endregion // constructors

        #region Validated event
        public event EventHandler Validated;
        private void OnValidated() { if (Validated != null) Validated(this, null); }
        #endregion // Validated event

        #region Document
        public Document Document { get; private set; }
        #endregion // Document

        #region Position
        private IElement _position;

        public IElement Position {
            get { return _position; }
            set {
                if (!Document.ReportRights.WriteTransferValuesAllowed)
                    throw new AccessDeniedException(ExceptionMessages.InsufficentWriteRights);

                if (_position != null) Document.ReconciliationManager.UnassignTransaction(this);

                string oldValue = _position == null ? null : _position.Id;
                _position = value;
                if (_doLog) LogManager.Instance.UpdateReconciliationTransactionPosition(this, oldValue);

                if (_position != null) Document.ReconciliationManager.AssignTransaction(this);

                DbEntity.ElementId = value == null ? 0 : Document.TaxonomyIdManager.GetId(value.Id);
                OnPropertyChanged("Position");

                Save();
                Validate();
            }
        }
        #endregion // Position

        #region Label
        public string Label { get { return Reconciliation.Label; } }
        #endregion // Label

        #region Reconciliation
        private IReconciliation _reconciliation;

        public IReconciliation Reconciliation {
            get { return _reconciliation; }
            private set {
                _reconciliation = value;
                if (value != null)
                    value.PropertyChanged +=
                        (sender, args) => {
                            if (args.PropertyName == "Name") OnPropertyChanged("Label");
                            else if (args.PropertyName == "IsSelected") OnPropertyChanged("IsReconciliationSelected");
                        };
            }
        }
        #endregion // Reconciliation

        #region Value
        public virtual decimal? Value {
            get {
                switch (TransactionType) {

                    case TransactionTypes.Source: {

                            if (ReconciliationInfo == null) return null;

                            if (!ReconciliationInfo.HBValue.HasValue &&
                                !ReconciliationInfo.TransferValuePreviousYear.HasValue)
                                return null;

                            decimal result = 0;
                            if (ReconciliationInfo.HBValue.HasValue) result -= ReconciliationInfo.HBValue.Value;
                            if (ReconciliationInfo.TransferValuePreviousYear.HasValue)
                                result -= ReconciliationInfo.TransferValuePreviousYear.Value;
                            return result;
                        }

                    case TransactionTypes.Destination: {
                            var transaction = ((IReclassification)Reconciliation).SourceTransaction;
                            if (transaction == null || transaction.ReconciliationInfo == null) return null;
                            if (!transaction.ReconciliationInfo.HBValue.HasValue &&
                                !transaction.ReconciliationInfo.TransferValuePreviousYear.HasValue)
                                return null;

                            decimal result = 0;
                            if (transaction.ReconciliationInfo.HBValue.HasValue) result += transaction.ReconciliationInfo.HBValue.Value;
                            if (transaction.ReconciliationInfo.TransferValuePreviousYear.HasValue)
                                result += transaction.ReconciliationInfo.TransferValuePreviousYear.Value;
                            return result;
                        }

                    case TransactionTypes.NetIncome: {
                        // add NetIncomePosition
                        switch (Document.MainTaxonomyInfo.Type) {
                            case TaxonomyType.Unknown:
                                break;

                            case TaxonomyType.GCD:
                                break;

                            case TaxonomyType.GAAP:
                            case TaxonomyType.OtherBusinessClass:
                                if (Document != null && Document.ReconciliationManager != null) {
                                    var reconciliationInfo =
                                        Document.ReconciliationManager.GetReconciliationInfo("de-gaap-ci_is.netIncome");
                                    if (reconciliationInfo == null) return null;
                                    if (!reconciliationInfo.ComputedValueTransfer.
                                        ComputedValueByReconciliation.ContainsKey(Reconciliation)) return null;
                                    
                                    return reconciliationInfo.ComputedValueTransfer.
                                        ComputedValueByReconciliation[Reconciliation];
                                }
                                return null;
                                
                            case TaxonomyType.Financial:
                                // not yet supported
                                break;

                            case TaxonomyType.Insurance:
                                // not yet supported
                                break;

                            default:
                                throw new ArgumentOutOfRangeException();
                        }

                        return null;
                    }

                    default:
                        return DbEntity.Value;
                }
            }
            set {
                if (!Document.ReportRights.WriteTransferValuesAllowed)
                    throw new AccessDeniedException(ExceptionMessages.InsufficentWriteRights);

                switch (TransactionType) {

                    case TransactionTypes.Source:
                    case TransactionTypes.Destination:
                        // not supported for reclassification
                        break;

                    default:
                        string oldValue = DbEntity.Value.ToString(CultureInfo.InvariantCulture);
                        DbEntity.Value = value ?? 0;
                        if (_doLog) LogManager.Instance.UpdateReconciliationTransactionValue(this, oldValue);
                        OnPropertyChanged("Value");
                        Save();
                        Validate();
                        break;
                }
            }
        }
        #endregion // Value

        #region ValueDisplayString
        public string ValueDisplayString { get { return LocalisationUtils.CurrencyToString(Value); } }
        #endregion // ValueDisplayString

        #region ReconciliationInfo
        private ReconciliationInfo _reconciliationInfo;

        public ReconciliationInfo ReconciliationInfo {
            get { return _reconciliationInfo; }
            set {
                if (_reconciliationInfo != null)
                    _reconciliationInfo.PropertyChanged -= ReconciliationInfoOnPropertyChanged;
                _reconciliationInfo = value;
                if (_reconciliationInfo != null)
                    _reconciliationInfo.PropertyChanged += ReconciliationInfoOnPropertyChanged;

                OnPropertyChanged("ReconciliationInfo");
                RaiseValuePropertyChanged();
            }
        }

        private void ReconciliationInfoOnPropertyChanged(object sender, PropertyChangedEventArgs e) {
            if (e.PropertyName == "HBValue" || e.PropertyName == "TransferValuePreviousYear" || e.PropertyName == "TransferValuePreviousYearCorrection") {
                RaiseValuePropertyChanged();
            }
        }

        private void RaiseValuePropertyChanged() {
            switch (TransactionType) {
                case TransactionTypes.Source: {
                    OnPropertyChanged("Value");
                    OnPropertyChanged("ValueDisplayString");
                    Validate();
                    var transaction =
                        (ReconciliationTransaction) ((IReclassification) Reconciliation).DestinationTransaction;
                    if (transaction != null) {
                        transaction.OnPropertyChanged("Value");
                        transaction.OnPropertyChanged("ValueDisplayString");
                        transaction.Validate();
                    }
                }
                    break;
            }
        }
        #endregion // ReconciliationInfo

        #region TransactionType
        public TransactionTypes TransactionType { get { return DbEntity.TransactionType; } }
        #endregion // TransactionType

        #region Warnings
        private readonly ObservableCollectionAsync<string> _warnings = new ObservableCollectionAsync<string>();
        public IEnumerable<string> Warnings { get { return _warnings; } }
        #endregion // Warnings

        #region IsValid
        public bool IsValid { get { return _warnings.Count == 0; } }
        #endregion // IsValid

        #region IsSelected
        private bool _isSelected;

        public bool IsSelected {
            get { return _isSelected; }
            set {
                if (_isSelected == value) return;
                _isSelected = value;
                OnPropertyChanged("IsSelected");

                if (_isSelected && ReconciliationInfo != null)
                    ReconciliationInfo.OnTransactionSelected(this);
            }
        }
        #endregion // IsSelected

        #region IsExpanded
        // ReSharper disable ValueParameterNotUsed
        public bool IsExpanded {
            get { return false; }
            set {
                // not supported
            }
        }

        // ReSharper restore ValueParameterNotUsed
        #endregion // IsExpanded

        #region IsVisible
        // ReSharper disable ValueParameterNotUsed
        public bool IsVisible {
            get { return true; }
            set {
                // not supported
            }
        }

        // ReSharper restore ValueParameterNotUsed
        #endregion // IsVisible

        #region IsReconciliationSelected
        public bool IsReconciliationSelected { get { return Reconciliation.IsSelected; } }
        #endregion // IsReconciliationSelected

        /// <summary>
        ///   Deletes the database representation of this entity (called from assigned reconciliation in remove method).
        /// </summary>
        internal void Delete() {
            using (var conn = AppConfig.ConnectionManager.GetConnection()) {
                try {
                    conn.DbMapping.Delete(DbEntity);
                } catch (Exception ex) {
                    throw new Exception(ResourcesReconciliation.DeleteTransactionException + ex.Message, ex);
                }
            }

            if (_doLog) LogManager.Instance.DeleteReconciliationTransaction(this);
        }

        /// <summary>
        ///   Saves this entity to database (called on value change).
        /// </summary>
        private void Save() {
            using (var conn = AppConfig.ConnectionManager.GetConnection()) {
                try {
                    conn.DbMapping.Save(DbEntity);
                } catch (Exception ex) {
                    throw new Exception(ResourcesReconciliation.SaveTransactionException + ex.Message, ex);
                }
            }
        }

        /// <summary>
        ///   Removes this transaction from the assigned reconciliation.
        /// </summary>
        public void Remove() { Reconciliation.RemoveTransaction(this); }

        /// <summary>
        /// Validates the reconciliation transaction.
        /// </summary>
        public void Validate() {
            _warnings.Clear();
            if (Position == null) _warnings.Add(ResourcesReconciliation.ReconciliationWarningNoAssignedPosition);
            if (Value == null)
                switch (TransactionType) {
                    case TransactionTypes.Source:
                        if (Position != null)
                            _warnings.Add(ResourcesReconciliation.ReconciliationWarningNoComercialBalanceValue);
                        break;

                    case TransactionTypes.Destination:
                        // no warning for destination positions
                        break;

                    default:
                        _warnings.Add(ResourcesReconciliation.ReconciliationWarningNoAssignedValue);
                        break;
                }

            OnValidated();
            OnPropertyChanged("IsValid");
        }

        public bool IsAssignmentAllowed(IElement position) {
            if (position == null || !position.IsReconciliationPosition) return false;
            if (Position.Id == position.Id) return false;
            if (Position.IsBalanceSheetAssetsPosition && !position.IsBalanceSheetAssetsPosition) return false;
            if (Position.IsBalanceSheetLiabilitiesPosition && !position.IsBalanceSheetLiabilitiesPosition) return false;
            if (Position.IsIncomeStatementPosition && !position.IsIncomeStatementPosition) return false;

            if (Document.ReconciliationManager.Reclassifications.Any(reclassification => reclassification.SourceElement != null && reclassification.SourceElement.Id == position.Id)) {
                //result = ResourcesReconciliation.TransactionAssignmentErrorPositionIsReclassificationSource;
                return false;
            }
            string result;
            return Reconciliation.IsAssignmentAllowed(position, out result);
        }

        #region Parents
        private readonly List<IReconciliationTreeNode> _parents = new List<IReconciliationTreeNode>();

        public virtual IEnumerable<IReconciliationTreeNode> Parents { get { return _parents; } }
        #endregion // Parents

        public void SetParent(IReconciliationTreeNode parent) {
            _parents.Clear();
            _parents.Add(parent);
        }

        public void ResetParent() { _parents.Clear(); }

        public double Order {
            get {
                switch (DbEntity.TransactionType) {
                    case TransactionTypes.Unspecified:
                        return -4;

                    case TransactionTypes.Source:
                        return -3;

                    case TransactionTypes.Destination:
                        return -2;

                    default:
                        return -1;
                }
            }
        }

        internal void DisableLogging() { _doLog = false; }
        internal void EnableLogging() { _doLog = true; }

        public event ScrollIntoViewRequestedEventHandler ScrollIntoViewRequested;
        public event ScrollIntoViewRequestedEventHandler SearchLeaveFocusRequested;
        public void ScrollIntoView(IList<ISearchableNode> path) {
            if (ScrollIntoViewRequested != null)
                ScrollIntoViewRequested(path);
        }

        public void SearchLeaveFocus(IList<ISearchableNode> path) {
            if (SearchLeaveFocusRequested != null)
                SearchLeaveFocusRequested(path);
        }

        public IElement Element { get { return null; } }
    }
}