// --------------------------------------------------------------------------------
// author: Mirko Dibbert
// since: 2012-03-06
// copyright 2012 AvenDATA GmbH
// --------------------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Linq;
using Utils;
using eBalanceKitBusiness.Exceptions;
using eBalanceKitBusiness.Localisation;
using eBalanceKitBusiness.Logs;
using eBalanceKitBusiness.Reconciliation.DbMapping;
using eBalanceKitBusiness.Reconciliation.Interfaces;
using eBalanceKitBusiness.Reconciliation.Interfaces.ReconciliationTypes;
using eBalanceKitBusiness.Reconciliation.ReconciliationTypes;
using eBalanceKitBusiness.Structures;
using eBalanceKitBusiness.Structures.DbMapping;
using eBalanceKitBusiness.Structures.XbrlElementValue;
using eBalanceKitResources.Localisation;
using eBalanceKitBusiness.Manager;

namespace eBalanceKitBusiness.Reconciliation {
    internal class ReconciliationManager : NotifyPropertyChangedBase, IReconciliationManagerInternal, IReconciliationManagerManagement {
        private readonly Dictionary<string, List<IReconciliationTransaction>> _transactionsById =
            new Dictionary<string, List<IReconciliationTransaction>>();

        private readonly Dictionary<string, IValueTreeEntry> _valuesById = new Dictionary<string, IValueTreeEntry>();

        private readonly Dictionary<string, IReconciliationInfo> _reconciliationInfos =
            new Dictionary<string, IReconciliationInfo>();

        internal ReconciliationManager(Document document) {
            Document = document;
            LoadDbEntities();
        }

        internal ReconciliationManager() {
        }

        #region help methods
        
        /// <summary>
        /// Loads the db entities.
        /// </summary>
        private void LoadDbEntities() {
            using (var conn = AppConfig.ConnectionManager.GetConnection()) {
                try {
                    conn.DbMapping.Load<DbEntityReconciliation>(conn.Enquote("document_id") + "=" + Document.Id).ForEach(dbEntity => {
                        dbEntity.Document = Document;
                        dbEntity.Transactions.ForEach(t => t.Document = Document);

                        switch (dbEntity.ReconciliationType) {
                            case Enums.ReconciliationTypes.Reclassification:
                                Reclassification reclassification = new Reclassification(dbEntity);
                                reclassification.OnIsSelectedChanged += ReconciliationOnIsSelectedChanged;
                                _reconciliations.Add(reclassification);
                                _reclassifications.Add(reclassification);
                                break;

                            case Enums.ReconciliationTypes.ValueChange:
                                ValueChange valueChange = new ValueChange(dbEntity);
                                valueChange.OnIsSelectedChanged += ReconciliationOnIsSelectedChanged;
                                _reconciliations.Add(valueChange);
                                _valueChanges.Add(valueChange);
                                break;

                            case Enums.ReconciliationTypes.Delta:
                                DeltaReconciliation deltaReconciliation = new DeltaReconciliation(dbEntity);
                                deltaReconciliation.OnIsSelectedChanged += ReconciliationOnIsSelectedChanged;
                                _reconciliations.Add(deltaReconciliation);
                                _deltaReconciliations.Add(deltaReconciliation);
                                break;

                            case Enums.ReconciliationTypes.PreviousYearValues:
                                PreviousYearValues = new PreviousYearValues(dbEntity);
                                break;

                            case Enums.ReconciliationTypes.ImportedValues:
                                ImportedValues importedValues = new ImportedValues(dbEntity);
                                importedValues.OnIsSelectedChanged += ReconciliationOnIsSelectedChanged;
                                _reconciliations.Add(importedValues);
                                _importedValues.Add(importedValues);
                                break;

                            case Enums.ReconciliationTypes.AuditCorrection:
                                AuditCorrection auditCorrection = new AuditCorrection(dbEntity);
                                auditCorrection.OnIsSelectedChanged += ReconciliationOnIsSelectedChanged;
                                _reconciliations.Add(auditCorrection);
                                _auditCorrections.Add(auditCorrection);
                                break;

                            case Enums.ReconciliationTypes.AuditCorrectionPreviousYear:
                                AuditCorrectionsPreviousYear = new AuditCorrectionPreviousYear(dbEntity);
                                break;

                            case Enums.ReconciliationTypes.TaxBalanceValue:
                                TaxBalanceValue taxBalanceValue = new TaxBalanceValue(dbEntity);
                                taxBalanceValue.OnIsSelectedChanged += ReconciliationOnIsSelectedChanged;
                                _reconciliations.Add(taxBalanceValue);
                                _taxBalanceValues.Add(taxBalanceValue);
                                break;

                            default:
                                throw new ArgumentOutOfRangeException();
                        }
                    });

                    // sort subset lists
                    SortReconciliationList(_reclassifications);
                    SortReconciliationList(_valueChanges);
                    SortReconciliationList(_deltaReconciliations);
                    SortReconciliationList(_importedValues);
                    SortReconciliationList(_auditCorrections);
                    SortReconciliationList(_taxBalanceValues);

                    // create reconciliation for previous year values if not exists
                    if (PreviousYearValues == null) {
                        var previousYearValues = new PreviousYearValues(Document);
                        previousYearValues.Save();
                        PreviousYearValues = previousYearValues;
                    }

                    // create reconciliation for previous year correction values if not exists
                    if (AuditCorrectionsPreviousYear == null) {
                        var auditCorrectionsPreviousYear = new AuditCorrectionPreviousYear(Document);
                        auditCorrectionsPreviousYear.Save();
                        AuditCorrectionsPreviousYear  = auditCorrectionsPreviousYear;
                    }

                    PreviousYearValues.OnIsSelectedChanged += ReconciliationOnIsSelectedChanged;
                    AuditCorrectionsPreviousYear.OnIsSelectedChanged += ReconciliationOnIsSelectedChanged;

                } catch (Exception ex) {
                    throw new Exception(ResourcesReconciliation.LoadException + ex.Message, ex);
                }

                bool referenceListExists = false;
                try {
                    conn.DbMapping.Load<DbEntityReferenceList>(conn.Enquote("document_id") + "=" + Document.Id + " AND " + conn.Enquote("user_id") + "=" + UserManager.Instance.CurrentUser.Id).ForEach(dbEntity => {
                        dbEntity.Document = Document;
                        dbEntity.User = UserManager.Instance.CurrentUser;

                        ReferenceList referenceList = new ReferenceList(dbEntity);
                        ReferenceList = referenceList;
                        referenceListExists = true;
                    });
                } catch (Exception ex) {
                    throw new Exception(ResourcesReconciliation.LoadException + ex.Message, ex);
                }
                if (!referenceListExists)
                    ReferenceList = new ReferenceList(Document, UserManager.Instance.CurrentUser);
            }

            foreach (ReconciliationTypes.Reconciliation reconciliation in Reconciliations) {
                reconciliation.Validate();
            }
        }

        /// <summary>
        /// Sorts the reconciliation list.
        /// </summary>
        /// <param name="items">The items.</param>
        private static void SortReconciliationList(IList<IReconciliation> items) {
            var tmp = items.ToList();
            items.Clear();
            foreach (var item in
                from reconciliation in tmp orderby reconciliation.Name select reconciliation) {
                items.Add(item);
            }
        }

        #endregion // help methods

        #region IReconciliationManager members

        public ReferenceList ReferenceList { get; private set; }

        #region properties

        #region Document
        /// <summary>
        /// Assigned document.
        /// </summary>
        public Document Document { get; private set; }
        #endregion // Document

        #region subset lists

        #region PreviousYearValues
        /// <summary>
        /// Special reconciliation to hold all previous year values.
        /// </summary>
        /// <value></value>
        public IPreviousYearValues PreviousYearValues { get; private set; }
        #endregion // PreviousYearValues

        #region AuditCorrectionsPreviousYear
        /// <summary>
        /// Special reconciliation to hold all previous year correction values.
        /// </summary>
        /// <value></value>
        public IDeltaReconciliation AuditCorrectionsPreviousYear { get; private set; }
        #endregion // AuditCorrectionsPreviousYear

        #region Reconciliations
        private readonly ObservableCollectionAsync<IReconciliation> _reconciliations =
            new ObservableCollectionAsync<IReconciliation>();

        /// <summary>
        /// List of all reconciliations in the assigned document.
        /// </summary>
        /// <value></value>
        public IEnumerable<IReconciliation> Reconciliations {
            get {
                if (!Document.ReportRights.ReadTransferValuesAllowed)
                    throw new AccessDeniedException(ExceptionMessages.InsufficentReadRights);

                return _reconciliations;
            }
        }
        #endregion // Reconciliations

        #region Reclassifications
        private readonly ObservableCollectionAsync<IReconciliation> _reclassifications =
            new ObservableCollectionAsync<IReconciliation>();

        /// <summary>
        /// List of all reclassifications in the assigned document (subset of Reconciliations).
        /// </summary>
        /// <value></value>
        public IEnumerable<IReclassification> Reclassifications {
            get {
                if (!Document.ReportRights.ReadTransferValuesAllowed)
                    throw new AccessDeniedException(ExceptionMessages.InsufficentReadRights);

                return _reclassifications.OfType<IReclassification>();
            }
        }
        #endregion // Reclassifications

        #region ValueChanges
        private readonly ObservableCollectionAsync<IReconciliation> _valueChanges =
            new ObservableCollectionAsync<IReconciliation>();

        /// <summary>
        /// List of all value changes in the assigned document (subset of Reconciliations).
        /// </summary>
        /// <value></value>
        public IEnumerable<IValueChange> ValueChanges {
            get {
                if (!Document.ReportRights.ReadTransferValuesAllowed)
                    throw new AccessDeniedException(ExceptionMessages.InsufficentReadRights);

                return _valueChanges.OfType<IValueChange>();
            }
        }
        #endregion // ValueChanges

        #region DeltaReconciliations
        private readonly ObservableCollectionAsync<IReconciliation> _deltaReconciliations =
            new ObservableCollectionAsync<IReconciliation>();

        /// <summary>
        /// List of all delta reconciliations in the assigned document (subset of Reconciliations).
        /// </summary>
        /// <value></value>
        public IEnumerable<IDeltaReconciliation> DeltaReconciliations {
            get {
                if (!Document.ReportRights.ReadTransferValuesAllowed)
                    throw new AccessDeniedException(ExceptionMessages.InsufficentReadRights);

                return _deltaReconciliations.OfType<IDeltaReconciliation>();
            }
        }
        #endregion // DeltaReconciliations

        #region ImportedValues

        private readonly ObservableCollectionAsync<IReconciliation> _importedValues =
            new ObservableCollectionAsync<IReconciliation>();

        /// <summary>
        /// List of all imported values in the assigned document (subset of Reconciliations).
        /// </summary>
        /// <value></value>
        public IEnumerable<IImportedValues> ImportedValues {
            get {
                if (!Document.ReportRights.ReadTransferValuesAllowed)
                    throw new AccessDeniedException(ExceptionMessages.InsufficentReadRights);

                return _importedValues.OfType<IImportedValues>();
            }
        }
        #endregion // ImportedValues

        #region AuditCorrections
        private readonly ObservableCollectionAsync<IReconciliation> _auditCorrections =
            new ObservableCollectionAsync<IReconciliation>();

        /// <summary>
        /// List of all audit corrections in the assigned document (subset of Reconciliations).
        /// </summary>
        public IEnumerable<IDeltaReconciliation> AuditCorrections {
            get {
                if (!Document.ReportRights.ReadTransferValuesAllowed)
                    throw new AccessDeniedException(ExceptionMessages.InsufficentReadRights);

                return _auditCorrections.OfType<IDeltaReconciliation>();
            }
        }
        #endregion // AuditCorrections

        #region TaxBalanceValue
        private readonly ObservableCollectionAsync<IReconciliation> _taxBalanceValues =
            new ObservableCollectionAsync<IReconciliation>();

        /// <summary>
        /// List of all TaxBalanceValue in the assigned document (subset of Reconciliations).
        /// </summary>
        public IEnumerable<IDeltaReconciliation> TaxBalanceValues {
            get {
                if (!Document.ReportRights.ReadTransferValuesAllowed)
                    throw new AccessDeniedException(ExceptionMessages.InsufficentReadRights);

                return _taxBalanceValues.OfType<IDeltaReconciliation>();
            }
        }
        #endregion // TaxBalanceValue

        #endregion // subset lists

        #region SelectedReconciliation
        private IReconciliation selectedReconciliation = null;
        /// <summary>
        /// Gets the selected reconciliation.
        /// </summary>
        /// <value>The selected reconciliation.</value>
        public IReconciliation SelectedReconciliation { get {
            return selectedReconciliation;
        } }
        #endregion // SelectedReconciliation

        #endregion // properties

        #region methods
        /// <summary>
        /// Creates a new reconciliation of the specified type.
        /// </summary>
        /// <param name="type"> Type of the new reconciliation. </param>
        public IReconciliation AddReconciliation(Enums.ReconciliationTypes type) {
            if (!Document.ReportRights.WriteTransferValuesAllowed)
                throw new AccessDeniedException(ExceptionMessages.InsufficentWriteRights);

            IReconciliation reconciliation;
            switch (type) {
                case Enums.ReconciliationTypes.Reclassification: {
                    reconciliation = new Reclassification(Document);
                    _reclassifications.Add(reconciliation as IReclassification);

                    int i = 1;
                    string tmpName = ResourcesReconciliation.ReconciliationReclassification + " " + i;
                    while (_reconciliations.Any(r => r.Name == tmpName)) {
                        i++;
                        tmpName = ResourcesReconciliation.ReconciliationReclassification + " " + i;
                    }
                    reconciliation.Name = tmpName;
                    SortReconciliationList(_reclassifications);
                }
                    break;

                case Enums.ReconciliationTypes.ValueChange: {
                    reconciliation = new ValueChange(Document);
                    _valueChanges.Add(reconciliation as IValueChange);
                    int i = 1;
                    string tmpName = ResourcesReconciliation.ReconciliationValueChange + " " + i;
                    while (_reconciliations.Any(r => r.Name == tmpName)) {
                        i++;
                        tmpName = ResourcesReconciliation.ReconciliationValueChange + " " + i;
                    }
                    reconciliation.Name = tmpName;
                    SortReconciliationList(_valueChanges);
                }
                    break;

                case Enums.ReconciliationTypes.Delta: {
                    reconciliation = new DeltaReconciliation(Document);
                    _deltaReconciliations.Add(reconciliation as IDeltaReconciliation);
                    int i = 1;
                    string tmpName = ResourcesReconciliation.ReconciliationDelta + " " + i;
                    while (_deltaReconciliations.Any(r => r.Name == tmpName)) {
                        i++;
                        tmpName = ResourcesReconciliation.ReconciliationDelta + " " + i;
                    }
                    reconciliation.Name = tmpName;
                    SortReconciliationList(_deltaReconciliations);
                }
                    break;

                case Enums.ReconciliationTypes.AuditCorrection: {
                        reconciliation = new AuditCorrection(Document);
                        _auditCorrections.Add(reconciliation as IDeltaReconciliation);
                        int i = 1;
                        string tmpName = ResourcesReconciliation.ReconciliationAuditCorrection + " " + i;
                        while (_auditCorrections.Any(r => r.Name == tmpName)) {
                            i++;
                            tmpName = ResourcesReconciliation.ReconciliationAuditCorrection + " " + i;
                        }
                        reconciliation.Name = tmpName;
                        SortReconciliationList(_auditCorrections);
                    }
                    break;

                case Enums.ReconciliationTypes.AuditCorrectionPreviousYear:
                case Enums.ReconciliationTypes.PreviousYearValues:
                    throw new Exception("This reconciliation type cannot be directly created.");

                case Enums.ReconciliationTypes.ImportedValues: {
                    reconciliation = new ImportedValues(Document);
                    _importedValues.Add(reconciliation as IImportedValues);
                    int i = 1;
                    string tmpName = ResourcesReconciliation.ReconciliationImportedValues + " " + i;
                    while (_reconciliations.Any(r => r.Name == tmpName)) {
                        i++;
                        tmpName = ResourcesReconciliation.ReconciliationImportedValues + " " + i;
                    }
                    reconciliation.Name = tmpName;
                    SortReconciliationList(_importedValues);
                }
                    break;

                case Enums.ReconciliationTypes.TaxBalanceValue: {
                        reconciliation = new TaxBalanceValue(Document);
                        _taxBalanceValues.Add(reconciliation as ITaxBalanceValue);
                        int i = 1;
                        string tmpName = ResourcesReconciliation.ReconciliationImportedValues + " " + i;
                        while (_reconciliations.Any(r => r.Name == tmpName)) {
                            i++;
                            tmpName = ResourcesReconciliation.ReconciliationImportedValues + " " + i;
                        }
                        reconciliation.Name = tmpName;
                        SortReconciliationList(_taxBalanceValues);
                    }
                    break;

                default:
                    throw new ArgumentOutOfRangeException("type");
            }

            _reconciliations.Add(reconciliation);
            ((ReconciliationTypes.Reconciliation) reconciliation).Save();
            reconciliation.OnIsSelectedChanged += ReconciliationOnIsSelectedChanged;
            reconciliation.IsSelected = true;

            LogManager.Instance.NewReconciliation(reconciliation);
            
            ((ReconciliationTypes.Reconciliation)reconciliation).Validate();
            
            return reconciliation;
        }

        /// <summary>
        /// Deletes the specified reconciliation from the reconciliation list and removes it's persistant copy.
        /// </summary>
        /// <param name="reconciliation"> The reconciliation which should be deleted. </param>
        /// <param name="deleteFromDb"></param>
        public void DeleteReconciliation(IReconciliation reconciliation, bool deleteFromDb = true) {
            if (!Document.ReportRights.WriteTransferValuesAllowed)
                throw new AccessDeniedException(ExceptionMessages.InsufficentWriteRights);

            if (deleteFromDb)
                ((ReconciliationTypes.Reconciliation)reconciliation).Delete();
            
            _reconciliations.Remove(reconciliation);
            switch (reconciliation.ReconciliationType) {
                case Enums.ReconciliationTypes.AuditCorrectionPreviousYear:
                case Enums.ReconciliationTypes.PreviousYearValues:
                    // nothing to do (these special reconciliations are not removable)
                    break;
                
                case Enums.ReconciliationTypes.ImportedValues:
                    _importedValues.Remove(reconciliation as IImportedValues);
                    break;
                
                case Enums.ReconciliationTypes.Reclassification:
                    _reclassifications.Remove(reconciliation as IReclassification);
                    break;
                
                case Enums.ReconciliationTypes.ValueChange:
                     _valueChanges.Remove(reconciliation as IValueChange);
                    break;
                
                case Enums.ReconciliationTypes.Delta:
                    _deltaReconciliations.Remove(reconciliation as IDeltaReconciliation);
                    break;
                
                case Enums.ReconciliationTypes.AuditCorrection:
                    _auditCorrections.Remove(reconciliation as IDeltaReconciliation);
                    break;

                case Enums.ReconciliationTypes.TaxBalanceValue:
                    _taxBalanceValues.Remove(reconciliation as ITaxBalanceValue);
                    break;
                
                default:
                    throw new ArgumentOutOfRangeException();
            }
 
            #region remove transaction assignments
            if (reconciliation is IReclassification) {
                var reclassification = (reconciliation as IReclassification);
                UnassignTransaction(reclassification.SourceTransaction);
                UnassignTransaction(reclassification.DestinationTransaction);
            
            } else if (reconciliation is IValueChange) {
                var valueChange = (reconciliation as IValueChange);
                foreach (var transaction in valueChange.Transactions)
                    ((IReconciliationManagerInternal)this).UnassignTransaction(transaction);

            } else if (reconciliation is ITaxBalanceValue) {
                ITaxBalanceValue taxBalanceValues = reconciliation as ITaxBalanceValue;
                foreach (var transaction in taxBalanceValues.Transactions)
                    ((IReconciliationManagerInternal)this).UnassignTransaction(transaction);
            } else if (reconciliation is IDeltaReconciliation) {
                var delta = reconciliation as IDeltaReconciliation;
                foreach (var transaction in delta.Transactions)
                    ((IReconciliationManagerInternal)this).UnassignTransaction(transaction);
            
            } else if (reconciliation is IImportedValues) {
                IImportedValues importedValues = reconciliation as IImportedValues;
                foreach (var transaction in importedValues.Transactions)
                    ((IReconciliationManagerInternal)this).UnassignTransaction(transaction);
            } 
            #endregion

            if (deleteFromDb) 
                LogManager.Instance.DeleteReconciliation(reconciliation);

            reconciliation.IsSelected = false;
            OnPropertyChanged("SelectedReconciliation");
        }

        #endregion // methods

        #endregion // IReconciliationManager members

        #region IReconciliationManagerInternal members

        /// <summary>
        /// Help method, which adds the specified transaction to the transaction dictionary - used from AssignTransactions method.
        /// </summary>
        /// <param name="transaction"> The transaction which should be added to the transaction dictionary. </param>
        private void AddTransactionToDictionary(IReconciliationTransaction transaction) {
            if (transaction.Position == null) return;
            var id = transaction.Position.Id;
            if (!_transactionsById.ContainsKey(id)) _transactionsById[id] = new List<IReconciliationTransaction>();
            _transactionsById[id].Add(transaction);
        }

        /// <summary>
        /// Assigns all existing transactions to the MainTaxonomy value tree of the assigned document. 
        /// This method should be called once after the assigned value tree has been initialized.
        /// </summary>
        public void AssignTransactions() {

            if (!Document.ReportRights.ReadTransferValuesAllowed) return;

            #region build transaction dictionary
            foreach (var reconciliation in _reconciliations) {
                switch (reconciliation.ReconciliationType) {
                    case Enums.ReconciliationTypes.PreviousYearValues:
                        // nothing to do
                        break;

                    case Enums.ReconciliationTypes.Reclassification: {
                        var reclassification = (reconciliation as IReclassification);
                        AddTransactionToDictionary(reclassification.SourceTransaction);
                        AddTransactionToDictionary(reclassification.DestinationTransaction);

                    }
                        break;

                    case Enums.ReconciliationTypes.AuditCorrection:
                    case Enums.ReconciliationTypes.AuditCorrectionPreviousYear:
                    case Enums.ReconciliationTypes.Delta:
                    case Enums.ReconciliationTypes.ValueChange:
                    case Enums.ReconciliationTypes.TaxBalanceValue: {
                        var delta = reconciliation as IDeltaReconciliation;
                        foreach (var transaction in delta.Transactions)
                            AddTransactionToDictionary(transaction);

                    }
                        break;

                    case Enums.ReconciliationTypes.ImportedValues: {
                        IImportedValues importedValues = reconciliation as IImportedValues;
                        foreach (IReconciliationTransaction transaction in importedValues.Transactions)
                            AddTransactionToDictionary(transaction);
                    }
                        break;

                    default:
                        throw new NotImplementedException();
                }
            }
            #endregion

            #region build value dictionary and create assignments between transactions and values
            foreach (
                var value in
                    Document.ValueTreeMain.Root.Values.Values.OfType<XbrlElementValue_Monetary>().Where(
                        value => value.Element.IsReconciliationPosition)) {
                var id = value.Element.Id;
                _valuesById[id] = value;
                if (_transactionsById.ContainsKey(id)) {
                    // assign existing transactions
                    var transactions = _transactionsById[id];
                    var item = new ReconciliationInfo(Document, value, transactions);
                    value.ReconciliationInfo = item;
                    foreach (var transaction in transactions.OfType<ReconciliationTransaction>())
                        transaction.ReconciliationInfo = item;
                    item.XbrlElementValue = value;

                    _reconciliationInfos[id] = item;
                } else {
                    // create empty info object
                    var item = new ReconciliationInfo(Document, value);
                    value.ReconciliationInfo = item;
                    item.XbrlElementValue = value;
                    
                    _reconciliationInfos[id] = item;
                }
            }
            #endregion

            // assign ReconciliationInfo property for existing previous year transactions
            foreach (var transaction in PreviousYearValues.Transactions) {
                AssignTransaction(transaction, false);
            }

            // assign ReconciliationInfo property for existing previous year correction transactions
            foreach (var transaction in AuditCorrectionsPreviousYear.Transactions) {
                AssignTransaction(transaction, false);
            }

            var netIncomeRecInfo = (ReconciliationInfo)GetReconciliationInfo("de-gaap-ci_is.netIncome");
            if (netIncomeRecInfo != null) netIncomeRecInfo.PropertyChanged += (sender, args) => {
                if (args.PropertyName == "TransferValue") {
                    // TODO
                }
            };
        }

        public void RefreshValues() {
            //OnPropertyChanged("PresentationTreeBalanceSheetTotalAssets");
            //OnPropertyChanged("PresentationTreeBalanceSheetLiabilities");
            //OnPropertyChanged("PresentationTreeIncomeStatement");
            foreach (var reconciliationInfo in _reconciliationInfos) {
                reconciliationInfo.Value.RefreshValues();
            }
        }

        public IReconciliationInfo GetReconciliationInfo(string id) {
            IReconciliationInfo item;
            _reconciliationInfos.TryGetValue(id, out item);
            return item;
        }

        /// <summary>
        /// Assigns the specified transaction to the MainTaxonomy value tree of the assigned document.
        /// </summary>
        /// <param name="transaction"> Transaction which should be added. </param>
        /// <param name="addTransaction"><b>true</b> if the transaction should be added to ReconciliationInfo.Transactions.</param>
        public void AssignTransaction(IReconciliationTransaction transaction, bool addTransaction = true) {
            var id = transaction.Position.Id;
            IValueTreeEntry value;
            _valuesById.TryGetValue(id, out value);
            if (value == null) return;
            
            transaction.ReconciliationInfo = value.ReconciliationInfo;
            if (addTransaction) value.ReconciliationInfo.AddTransaction(transaction);
        }

        /// <summary>
        /// Removes the assignment of the specified transaction from the MainTaxonomy value tree of the assigned document.
        /// </summary>
        /// <param name="transaction"> Transaction for which the assignment should be removed. </param>
        public void UnassignTransaction(IReconciliationTransaction transaction) {
            if (transaction.Position == null) return;
            var id = transaction.Position.Id;
            IValueTreeEntry value;
            _valuesById.TryGetValue(id, out value);
            if (value == null) return;
            
            transaction.ReconciliationInfo = null;
            value.ReconciliationInfo.RemoveTransaction(transaction);
        }

        public void DeleteAllReconciliations() {
            using (var conn = AppConfig.ConnectionManager.GetConnection()) {
                conn.ExecuteNonQuery(
                    "DELETE FROM " +
                    conn.Enquote(conn.DbMapping.GetTableName(typeof(DbEntityReconciliation))) + " WHERE " +
                    conn.Enquote("document_id") + " = " + Document.Id);

                conn.ExecuteNonQuery(
                    "DELETE FROM " +
                    conn.Enquote(conn.DbMapping.GetTableName(typeof(DbEntityReconciliationTransaction))) + " WHERE " +
                    conn.Enquote("document_id") + " = " + Document.Id);
            }

            foreach(var reconciliation in Reconciliations.ToList())
                DeleteReconciliation(reconciliation, false);
        }

        public void DeleteAllPreviousYearValues() {
            if (!Document.ReportRights.WriteTransferValuesAllowed)
                throw new AccessDeniedException(ExceptionMessages.InsufficentWriteRights);

            ((PreviousYearValues)PreviousYearValues).DisableLogging();
            foreach (var transaction in PreviousYearValues.Transactions.ToList()) {
                transaction.ReconciliationInfo.TransferValueInputPreviousYear = null;
            }
            LogManager.Instance.DeleteAllTransactions(PreviousYearValues);
            ((PreviousYearValues)PreviousYearValues).EnableLogging();
        }

        public void DeleteAllPreviousYearCorrectionValues() { DeleteAllPreviousYearCorrectionValues(true); }

        /// <summary>
        /// Deletes all previous year correction values whereas logging could be optionally disabled when called from another internal methods which logs the superior action.
        /// </summary>
        /// <param name="doLog"></param>
        internal void DeleteAllPreviousYearCorrectionValues(bool doLog) {
            if (!Document.ReportRights.WriteTransferValuesAllowed)
                throw new AccessDeniedException(ExceptionMessages.InsufficentWriteRights);

            ((AuditCorrectionPreviousYear)AuditCorrectionsPreviousYear).DisableLogging();
            foreach (var transaction in AuditCorrectionsPreviousYear.Transactions.ToList()) {
                transaction.ReconciliationInfo.TransferValueInputPreviousYearCorrection = null;
            }
            
            if (doLog) LogManager.Instance.DeleteAllTransactions(AuditCorrectionsPreviousYear);
            ((AuditCorrectionPreviousYear)AuditCorrectionsPreviousYear).EnableLogging();            
        }

        public void ImportPreviousYearValues(Import.PreviousYearValues previousYearValues) {
            if (!Document.ReportRights.WriteTransferValuesAllowed)
                throw new AccessDeniedException(ExceptionMessages.InsufficentWriteRights);

            ((PreviousYearValues)PreviousYearValues).DisableLogging();

            // delete existing values
            foreach (var transaction in PreviousYearValues.Transactions.ToList()) {
                transaction.ReconciliationInfo.TransferValueInputPreviousYear = null;
            }

            // add new values if it is not IsIncomeStatementPosition because incom statement position has no previous year value so it does not make sense to import it
            //foreach (var previousYearValue in previousYearValues.Values) {
            foreach (var previousYearValue in previousYearValues.Values.Where(t => !t.Element.IsIncomeStatementPosition)) {
                IValueTreeEntry value;
                _valuesById.TryGetValue(previousYearValue.Element.Id, out value);
                if (value == null) continue;
                value.ReconciliationInfo.TransferValueInputPreviousYear = previousYearValue.Value;
            }

            ((PreviousYearValues)PreviousYearValues).EnableLogging();

            LogManager.Instance.ImportReconciliationPreviousYearValues(PreviousYearValues);
        }

        public void MergePreviousYearValues(Import.PreviousYearValues previousYearValues) {
            if (!Document.ReportRights.WriteTransferValuesAllowed)
                throw new AccessDeniedException(ExceptionMessages.InsufficentWriteRights);

            ((PreviousYearValues)PreviousYearValues).DisableLogging();
            
            // add new values
            foreach (var previousYearValue in previousYearValues.Values) {
                IValueTreeEntry value;
                _valuesById.TryGetValue(previousYearValue.Element.Id, out value);
                if (value == null) continue;
                value.ReconciliationInfo.TransferValueInputPreviousYear = previousYearValue.Value;
            }

            ((PreviousYearValues)PreviousYearValues).EnableLogging();

            LogManager.Instance.ImportReconciliationPreviousYearValues(PreviousYearValues);
        }

        #endregion // IReconciliationManagerInternal members

        #region IReconciliationManagerManagement members
        public string GetReconciliationNameManagement(string id) {
            using (var conn = AppConfig.ConnectionManager.GetConnection()) {
                //StringBuilder stringBuilder = new StringBuilder();
                //stringBuilder.Append("SELECT ");
                //stringBuilder.Append(conn.Enquote("Name"));
                //stringBuilder.Append(" FROM ");
                //stringBuilder.Append(conn.Enquote(conn.DbMapping.GetTableName<DbEntityReconciliation>()));
                //stringBuilder.Append(" WHERE ");
                //stringBuilder.Append(conn.Enquote("id"));
                //stringBuilder.Append(" = {0}");
                List<DbEntityReconciliation> lineWithId =
                    conn.DbMapping.Load<DbEntityReconciliation>(string.Format(conn.Enquote("id") + " = {0}", id));
                return lineWithId.Count == 0 ? ResourcesLogging.DeletedReconciliation : lineWithId[0].Name;
            }
        }
        #endregion

        public event SelectedChangedEventHandler OnIsSelectedChanged;
        public void ReconciliationOnIsSelectedChanged(IReconciliation reconciliation, SelectedChangedEventArgs args) {

            if (args.PreviousState != args.CurrentState) {
                if (args.CurrentState) {
                    selectedReconciliation = reconciliation;
                } else {
                    if (PreviousYearValues != null && PreviousYearValues.IsSelected)
                        selectedReconciliation = PreviousYearValues;
                    else if (AuditCorrectionsPreviousYear != null && AuditCorrectionsPreviousYear.IsSelected)
                        selectedReconciliation = AuditCorrectionsPreviousYear;
                    selectedReconciliation = Reconciliations.FirstOrDefault(r => r.IsSelected);
                }
                if (this.OnIsSelectedChanged != null)
                    this.OnIsSelectedChanged(selectedReconciliation, new SelectedChangedEventArgs(args.PreviousState, args.CurrentState));
                OnPropertyChanged("SelectedReconciliation");
            }
        }
    }
}