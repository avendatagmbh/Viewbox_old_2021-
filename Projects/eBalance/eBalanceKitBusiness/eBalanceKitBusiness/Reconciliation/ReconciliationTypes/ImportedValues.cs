using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using Taxonomy;
using Utils;
using eBalanceKitBusiness.Exceptions;
using eBalanceKitBusiness.Localisation;
using eBalanceKitBusiness.Reconciliation.DbMapping;
using eBalanceKitBusiness.Reconciliation.Enums;
using eBalanceKitBusiness.Reconciliation.Interfaces;
using eBalanceKitBusiness.Reconciliation.Interfaces.ReconciliationTypes;
using eBalanceKitBusiness.Structures.DbMapping;
using eBalanceKitResources.Localisation;

namespace eBalanceKitBusiness.Reconciliation.ReconciliationTypes {
    internal class ImportedValues : Reconciliation, IImportedValues {

        #region Constructors
        /// <summary>
        /// Constructor for new reconciliation.
        /// </summary>
        /// <param name="document">Assigned document.</param>
        internal ImportedValues(Document document)
            : base(document, Enums.ReconciliationTypes.ImportedValues) {
            AddTransactionCollectionsChangedEventHandler();
        }

        /// <summary>
        /// Constructor for existing reconciliation.
        /// </summary>
        /// <param name="dbEntityReconciliation">Assigned reconciliation.</param>
        internal ImportedValues(DbEntityReconciliation dbEntityReconciliation)
            : base(dbEntityReconciliation) {

            AddTransactionCollectionsChangedEventHandler();

            DbEntity.Transactions.ForEach(
                t => {
                    var transaction = CreateTransaction(t);
                    _transactions.Add(transaction);
                    if (transaction.Position.IsBalanceSheetAssetsPosition) _balanceListAssets.AddTransaction(transaction, false);
                    else if (transaction.Position.IsBalanceSheetLiabilitiesPosition) _balanceListLiabilities.AddTransaction(transaction, false);
                    else if (transaction.Position.IsIncomeStatementPosition) _incomeStatement.AddTransaction(transaction, false);
                });

        }
        #endregion // constructors

        internal override void Validate() {
            base.Validate();
            OnPropertyChanged("IsValid");
        }

        #region Transactions
        private readonly ObservableCollectionAsync<IReconciliationTransaction> _transactions = new ObservableCollectionAsync<IReconciliationTransaction>();

        public override IEnumerable<IReconciliationTransaction> Transactions {
            get { return _transactions; } }
        #endregion // Transactions

        private void TransactionOnValidated(object sender, System.EventArgs eventArgs) { Validate(); }

        #region HasTransaction
        public bool HasTransaction { get { return _transactions.Any(); } }
        #endregion // HasTransaction

        #region transaction groups

        #region BalanceListAssets
        private readonly TransactionGroup _balanceListAssets =
            new TransactionGroup(ResourcesReconciliation.BalanceListAssets);

        public ITransactionGroup BalanceListAssets { get { return _balanceListAssets; } }
        #endregion // BalanceListAssets

        #region BalanceListLiabilities
        private readonly TransactionGroup _balanceListLiabilities =
            new TransactionGroup(ResourcesReconciliation.BalanceListLiabilities);

        public ITransactionGroup BalanceListLiabilities { get { return _balanceListLiabilities; } }
        #endregion // BalanceListLiabilities

        #region IncomeStatement
        private readonly TransactionGroup _incomeStatement =
            new TransactionGroup(ResourcesReconciliation.IncomeStatement);

        public ITransactionGroup IncomeStatement { get { return _incomeStatement; } }
        #endregion // IncomeStatement

        #region TransactionGroups
        public IEnumerable<ITransactionGroup> TransactionGroups { get { return new[] { BalanceListAssets, BalanceListLiabilities, IncomeStatement }; } }
        #endregion // TransactionGroups

        #endregion // transaction groups

        #region AddTransaction
        public ReconciliationTransaction AddTransaction(IElement position) {
            if (!Document.ReportRights.WriteTransferValuesAllowed)
                throw new AccessDeniedException(ExceptionMessages.InsufficentWriteRights);

            IReconciliationTransaction transaction = Transactions.FirstOrDefault(t => t.Position.Id == position.Id);
            if (transaction != null) return transaction as ReconciliationTransaction;

            string result;
            if (!IsAssignmentAllowed(position, out result)) return null;

            if (!position.IsReconciliationPosition)
                throw new Exception("Only balance sheet and income statement positions are allowed.");

            transaction = CreateTransaction(TransactionTypes.Unspecified, position);
            _transactions.Add(transaction);
            Document.ReconciliationManager.AssignTransaction(transaction);

            if (transaction.Position.IsBalanceSheetAssetsPosition) _balanceListAssets.AddTransaction(transaction);
            else if (transaction.Position.IsBalanceSheetLiabilitiesPosition) _balanceListLiabilities.AddTransaction(transaction);
            else if (transaction.Position.IsIncomeStatementPosition) _incomeStatement.AddTransaction(transaction);

            OnPropertyChanged("HasTransaction");
            return transaction as ReconciliationTransaction;
        }
        #endregion // AddTransaction

        #region RemoveTransaction
        public override void RemoveTransaction(IReconciliationTransaction transaction) {
            if (!Document.ReportRights.WriteTransferValuesAllowed)
                throw new AccessDeniedException(ExceptionMessages.InsufficentWriteRights);

            _transactions.Remove(transaction);
            if (transaction.Position.IsBalanceSheetAssetsPosition) _balanceListAssets.RemoveTransaction(transaction);
            if (transaction.Position.IsBalanceSheetLiabilitiesPosition) _balanceListLiabilities.RemoveTransaction(transaction);
            if (transaction.Position.IsIncomeStatementPosition) _incomeStatement.RemoveTransaction(transaction);

            Document.ReconciliationManager.UnassignTransaction(transaction);

            // delete persistant representation
            ((ReconciliationTransaction)transaction).Delete();

            OnPropertyChanged("HasTransaction");
        }
        #endregion // RemoveTransaction

        public override bool IsAssignmentAllowed(IElement position, out string result) {
            result = null;
            if (Document.ReconciliationManager.Reclassifications.Any(reclassification => reclassification.SourceElement != null && reclassification.SourceElement.Id == position.Id)) {
                result = ResourcesReconciliation.TransactionAssignmentErrorPositionIsReclassificationSource;
                return false;
            }

            if (Transactions.Any(t => t.Position.Id == position.Id)) return false;
            return true;
        }

        private void AddTransactionCollectionsChangedEventHandler() {
            _transactions.CollectionChanged += (sender, args) => {
                if (args.OldItems != null)
                    foreach (IReconciliationTransaction transaction in args.OldItems)
                        transaction.Validated -= TransactionOnValidated;
                if (args.NewItems != null)
                    foreach (IReconciliationTransaction transaction in args.NewItems)
                        transaction.Validated += TransactionOnValidated;

                Validate();
            };
        }
    }
}
