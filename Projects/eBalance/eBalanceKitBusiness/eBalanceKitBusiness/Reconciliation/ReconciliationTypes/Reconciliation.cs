// --------------------------------------------------------------------------------
// author: Mirko Dibbert
// since: 2012-03-06
// copyright 2012 AvenDATA GmbH
// --------------------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using Taxonomy;
using Utils;
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

namespace eBalanceKitBusiness.Reconciliation.ReconciliationTypes {
    
    internal abstract class Reconciliation : NotifyPropertyChangedBase, IReconciliation {

        private bool _doLog = true;

        internal DbEntityReconciliation DbEntity { get; set; }

        /// <summary>
        /// Constructor for new reconciliation.
        /// </summary>
        /// <param name="document">Assigned document.</param>
        /// <param name="type">Type of the reconciliation.</param>
        protected Reconciliation(Document document, Enums.ReconciliationTypes type) {
            Document = document;
            switch (type) {
                case Enums.ReconciliationTypes.Reclassification:
                    DbEntity = new DbEntityReconciliation(document, type, Enums.TransferKinds.Reclassification);
                    break;
                
                case Enums.ReconciliationTypes.ValueChange:
                    DbEntity = new DbEntityReconciliation(document, type, Enums.TransferKinds.ValueChange);
                    break;

                case Enums.ReconciliationTypes.AuditCorrection:
                case Enums.ReconciliationTypes.AuditCorrectionPreviousYear:
                case Enums.ReconciliationTypes.Delta:
                    DbEntity = new DbEntityReconciliation(document, type, Enums.TransferKinds.ReclassificationWithValueChange);
                    break;
                
                case Enums.ReconciliationTypes.PreviousYearValues:
                    DbEntity = new DbEntityReconciliation(document, type, Enums.TransferKinds.ReclassificationWithValueChange);
                    break;

                case Enums.ReconciliationTypes.ImportedValues:
                    DbEntity = new DbEntityReconciliation(document, type, Enums.TransferKinds.ReclassificationWithValueChange);
                    break;
                
                case Enums.ReconciliationTypes.TaxBalanceValue:
                    DbEntity = new DbEntityReconciliation(document, type, Enums.TransferKinds.ReclassificationWithValueChange);
                    break;

                default:
                    throw new ArgumentOutOfRangeException("type");
            }
        }

        /// <summary>
        /// Constructor for existing reconciliation.
        /// </summary>
        /// <param name="dbEntityReconciliation">Assignned database entity.</param>
        protected Reconciliation(DbEntityReconciliation dbEntityReconciliation) {
            Document = dbEntityReconciliation.Document;
            DbEntity = dbEntityReconciliation;
        }

        public Document Document { get; private set; }

        #region Name
        public string Name {
            get { return DbEntity.Name; }
            set {
                if (!Document.ReportRights.WriteTransferValuesAllowed)
                    throw new AccessDeniedException(ExceptionMessages.InsufficentWriteRights);

                if (DbEntity.Name == value) return;
                string oldValue = Name;
                DbEntity.Name = string.IsNullOrEmpty(value) ? null : StringUtils.Left(value, 512);
                if (_doLog && DbEntity.Id > 0) LogManager.Instance.UpdateReconciliationName(this, oldValue);

                Save();

                OnPropertyChanged("Name");
                OnPropertyChanged("Label");
            }
        }
        #endregion // Name

        #region Label
        public string Label { get { return Name ?? ResourcesReconciliation.EmptyReconciliationName; } }
        #endregion // Label

        #region Comment
        public string Comment {
            get { return DbEntity.Comment; }
            set {
                if (!Document.ReportRights.WriteTransferValuesAllowed)
                    throw new AccessDeniedException(ExceptionMessages.InsufficentWriteRights);

                if (DbEntity.Comment == value) return;
                string oldValue = Comment;
                DbEntity.Comment = string.IsNullOrEmpty(value) ? null : StringUtils.Left(value, 8192);
                if (_doLog) LogManager.Instance.UpdateReconciliationComment(this, oldValue);

                Save();

                OnPropertyChanged("Comment");
            }
        }
        #endregion // Comment      

        #region TransferKind
        /// <summary>
        /// Selected transfer kind.
        /// </summary>
        public TransferKinds TransferKind {
            get { return DbEntity.TransferKinds; }
            set {
                if (!Document.ReportRights.WriteTransferValuesAllowed)
                    throw new AccessDeniedException(ExceptionMessages.InsufficentWriteRights);

                if (DbEntity.TransferKinds == value) return;
                string oldValue = ((int)TransferKind).ToString(CultureInfo.InvariantCulture);
                DbEntity.TransferKinds = value;
                if (_doLog) LogManager.Instance.UpdateReconciliationTransferKind(this, oldValue);

                Save();

                OnPropertyChanged("TransferKind");
            }
        }
        #endregion // TransferKind

        #region TransferKinds
        /// <summary>
        /// Enumeration of all available transfer kinds.
        /// </summary>
        public IEnumerable<TransferKinds> TransferKinds { get { return Enum.GetValues(typeof (TransferKinds)).Cast<TransferKinds>(); } }
        #endregion // TransferKinds
        
        #region IsSelected

        public event SelectedChangedEventHandler OnIsSelectedChanged;
        private bool _isSelected;

        public bool IsSelected {
            get { return _isSelected; }
            set {
                bool prev = _isSelected;
                _isSelected = value;
                bool curr = _isSelected;
                
                OnPropertyChanged("IsSelected");
                if (this.OnIsSelectedChanged != null)
                    this.OnIsSelectedChanged(this, new SelectedChangedEventArgs(prev, curr));

                if (!_isSelected)
                    foreach (var transaction in Transactions)
                        transaction.IsSelected = false;
            }
        }
        #endregion // IsSelected

        #region ReconciliationType
        public Enums.ReconciliationTypes ReconciliationType { get { return DbEntity.ReconciliationType; } }
        #endregion // ReconciliationType

        #region IsEditable
        public bool IsEditable {
            get {
                switch (DbEntity.ReconciliationType) {
                    case Enums.ReconciliationTypes.ImportedValues:
                        return false;

                    default:
                        return Document.ReportRights.WriteTransferValuesAllowed;
                }
            }
        }
        #endregion // IsEditable

        #region Transactions
        /// <summary>
        /// Returns an enumeration of all assigned transactions.
        /// </summary>
        public abstract IEnumerable<IReconciliationTransaction> Transactions { get; }
        #endregion // Transactions

        #region Warnings
        protected readonly ObservableCollectionAsync<string> _warnings = new ObservableCollectionAsync<string>();
        public IEnumerable<string> Warnings { get { return _warnings; } }
        #endregion // Warnings

        #region IsValid
        public bool IsValid { get { return _warnings.Count == 0; } }
        #endregion // IsValid

        #region Delete
        internal void Delete() {
            using (var conn = AppConfig.ConnectionManager.GetConnection()) {
                try {
                    conn.DbMapping.Delete(DbEntity);
                } catch (Exception ex) {
                    throw new Exception(ResourcesReconciliation.DeleteException + ex.Message, ex);
                }
            }
        }
        #endregion // Delete
        
        #region Save
        internal void Save() {
            using (var conn = AppConfig.ConnectionManager.GetConnection()) {
                try {
                    conn.DbMapping.Save(DbEntity);
                } catch (Exception ex) {
                    throw new Exception(ResourcesReconciliation.SaveException + ex.Message, ex);
                }
            }
        }
        #endregion // Save

        //Used to avoid binding errors
        public bool IsExpandable { get { return true; } }

        public abstract void RemoveTransaction(IReconciliationTransaction transaction);

        /// <summary>
        /// Creates a new transaction for the specified element.
        /// </summary>
        /// <returns></returns>
        /// <param name="type">Type of the transaction.</param>
        /// <param name="position">Assigned position (optional)</param>
        protected ReconciliationTransaction CreateTransaction(TransactionTypes type, IElement position = null) {
            var transaction = new ReconciliationTransaction(Document, this, DbEntity, type, position);
            if (_doLog) LogManager.Instance.NewReconciliationTransaction(transaction);
            return transaction;
        }

        /// <summary>
        /// Creates a new transaction from the specified database entity.
        /// </summary>
        /// <param name="transactionDbEntity">An existing tranaction db entity, which should be assigned to the new transaction.</param>
        /// <returns></returns>
        protected ReconciliationTransaction CreateTransaction(DbEntityReconciliationTransaction transactionDbEntity) {

            Debug.Assert(transactionDbEntity != null, "The parameter 'dbEntity' is not allowed to be null!");
            return new ReconciliationTransaction(Document, this, transactionDbEntity);
        }

        public abstract bool IsAssignmentAllowed(IElement position, out string result);
        
        /// <summary>
        /// True, if transactions could be assigned to presentation tree nodes.
        /// </summary>
        internal virtual bool IsAssignable { get { return true; } }
        
        internal virtual void Validate() {
            _warnings.Clear();
            if (!Transactions.Any()) {
                _warnings.Add(ResourcesReconciliation.ReconciliationWarningNoAssignedTransaction);
                return;
            }

            // check for assets = liabilities not possible, since the previous year values are not assigned to the reconciliation datasets
            
            //var assetsTransactions = Transactions.Where(t => t.Position != null && t.Position.IsBalanceSheetAssetPosition && t.Value.HasValue).ToList();
            //var liabilitiesTransaction = Transactions.Where(t => t.Position != null && t.Position.IsBalanceSheetLiabilitiesPosition && t.Value.HasValue).ToList();

            //var hasAssetsTransactions = assetsTransactions.Any();
            //var hasLiabilitiesTransaction = liabilitiesTransaction.Any();

            //// ReSharper disable PossibleInvalidOperationException
            //if (hasAssetsTransactions && !hasLiabilitiesTransaction) {
            //    var sumAssets = assetsTransactions.Sum(t => t.Value.Value);
            //    if (sumAssets > 0) _warnings.Add("Summe Aktiva ungleich Summe Passiva.");

            //} else if (!hasAssetsTransactions && hasLiabilitiesTransaction) {
            //    var sumLiabilities = liabilitiesTransaction.Sum(t => t.Value.Value);
            //    if (sumLiabilities > 0) _warnings.Add("Summe Aktiva ungleich Summe Passiva.");

            //} else if (hasAssetsTransactions) { // && hasLiabilitiesTransaction
            //    var sumAssets = assetsTransactions.Sum(t => t.Value.Value);
            //    var sumLiabilities = liabilitiesTransaction.Sum(t => t.Value.Value);
            //    if (sumAssets != sumLiabilities) _warnings.Add("Summe Aktiva ungleich Summe Passiva.");

            //}
            //// ReSharper restore PossibleInvalidOperationException

            if (Transactions.Any(t => t.Warnings.Any())) _warnings.Add(ResourcesReconciliation.ReconciliationWarningWarningInAssignedTransaction);

            OnPropertyChanged("IsValid");
        }

        internal void DisableLogging() {
            _doLog = false;
            foreach (var transaction in Transactions)
                ((ReconciliationTransaction) transaction).DisableLogging();
        }

        internal void EnableLogging() {
            _doLog = true;
            foreach (var transaction in Transactions)
                ((ReconciliationTransaction)transaction).EnableLogging();
        }
    }
}