// --------------------------------------------------------------------------------
// author: Mirko Dibbert
// since: 2012-03-20
// copyright 2012 AvenDATA GmbH
// --------------------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Linq;
using Taxonomy;
using Taxonomy.Enums;
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
    internal class DeltaReconciliation : Reconciliation, IDeltaReconciliation {

        #region constructors
        /// <summary>
        /// Constructor for new reconciliation for derived classes.
        /// </summary>
        /// <param name="document">Assigned document.</param>
        /// <param name="type">Reconciliation type.</param>
        protected DeltaReconciliation(Document document, Enums.ReconciliationTypes type)
            : base(document, type) {
            AddTransactionCollectionsChangedEventHandler();
            CreateSpecialTransactions();
        }

        /// <summary>
        /// Constructor for new reconciliation.
        /// </summary>
        /// <param name="document">Assigned document.</param>
        internal DeltaReconciliation(Document document)
            : base(document, Enums.ReconciliationTypes.Delta) {
            AddTransactionCollectionsChangedEventHandler();
            CreateSpecialTransactions();
        }

        /// <summary>
        /// Constructor for existing reconciliation.
        /// </summary>
        /// <param name="dbEntityReconciliation">Assigned reconciliation.</param>
        internal DeltaReconciliation(DbEntityReconciliation dbEntityReconciliation)
            : base(dbEntityReconciliation) {

            AddTransactionCollectionsChangedEventHandler();

            DbEntity.Transactions.ForEach(
                t => {
                    var transaction = CreateTransaction(t);
                    _transactions.Add(transaction);
                    _transactionsById[transaction.Position.Id] = transaction;
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

        private void CreateSpecialTransactions() {

#pragma warning disable 0162
            // DEBUG
            return;

            // add NetIncomePosition
            IValueTreeEntry netIncomeValue = null;
            switch (Document.MainTaxonomyInfo.Type) {
                case TaxonomyType.Unknown:
                    break;

                case TaxonomyType.GCD:
                    break;

                case TaxonomyType.GAAP:
                case TaxonomyType.OtherBusinessClass:
                    netIncomeValue = Document.ValueTreeMain.GetValue("de-gaap-ci_bs.eqLiab.equity.netIncome");
                    break;

                case TaxonomyType.Financial:
                    // not yet supported
                    break;

                case TaxonomyType.Insurance:
                    // not yet supported
                    break;

                default:
                    throw new ArgumentOutOfRangeException();
            }
            
            if (netIncomeValue == null) return;
            
            var transaction = CreateTransaction(TransactionTypes.NetIncome, netIncomeValue.Element);
            _transactions.Add(transaction);
            _transactionsById[netIncomeValue.Element.Id] = transaction;
            Document.ReconciliationManager.AssignTransaction(transaction);
#pragma warning restore 0162
        }

        #region Transactions
        private readonly ObservableCollectionAsync<IReconciliationTransaction> _transactions =
            new ObservableCollectionAsync<IReconciliationTransaction>();

        private readonly Dictionary<string, IReconciliationTransaction> _transactionsById =
            new Dictionary<string, IReconciliationTransaction>();

        public override IEnumerable<IReconciliationTransaction> Transactions {
            get { return _transactions; } }
        #endregion // Transactions

        private void TransactionOnValidated(object sender, System.EventArgs eventArgs) { Validate(); }

        #region HasTransaction
        public bool HasTransaction { get { return _transactions.Any(); } }
        #endregion // HasTransaction

        #region GetTransaction
        internal IReconciliationTransaction GetTransaction(IElement position) {
            IReconciliationTransaction transaction;
            _transactionsById.TryGetValue(position.Id, out transaction);
            return transaction;
        }
        #endregion // GetTransaction

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
        public IEnumerable<ITransactionGroup> TransactionGroups { get { return new[] {BalanceListAssets, BalanceListLiabilities, IncomeStatement}; } }
        #endregion // TransactionGroups
        
        #endregion // transaction groups

        #region AddTransaction
        public virtual void AddTransaction(IElement position) {
            if (!Document.ReportRights.WriteTransferValuesAllowed)
                throw new AccessDeniedException(ExceptionMessages.InsufficentWriteRights);

            string result;
            if (!IsAssignmentAllowed(position, out result)) return;

            if (!position.IsReconciliationPosition) 
                throw new Exception("Only balance sheet and income statement positions are allowed.");

            var transaction = CreateTransaction(TransactionTypes.Unspecified, position);
            _transactions.Add(transaction);
            _transactionsById[position.Id] = transaction;

            if (IsAssignable) Document.ReconciliationManager.AssignTransaction(transaction);

            if (transaction.Position.IsBalanceSheetAssetsPosition) _balanceListAssets.AddTransaction(transaction);
            else if (transaction.Position.IsBalanceSheetLiabilitiesPosition) _balanceListLiabilities.AddTransaction(transaction);
            else if (transaction.Position.IsIncomeStatementPosition) _incomeStatement.AddTransaction(transaction);

            OnPropertyChanged("HasTransaction");
        }
        #endregion // AddTransaction
        
        #region RemoveTransaction
        public override void RemoveTransaction(IReconciliationTransaction transaction) {
            if (!Document.ReportRights.WriteTransferValuesAllowed)
                throw new AccessDeniedException(ExceptionMessages.InsufficentWriteRights);

            _transactions.Remove(transaction);
            _transactionsById.Remove(transaction.Position.Id);
            if (transaction.Position.IsBalanceSheetAssetsPosition) _balanceListAssets.RemoveTransaction(transaction);
            if (transaction.Position.IsBalanceSheetLiabilitiesPosition) _balanceListLiabilities.RemoveTransaction(transaction);
            if (transaction.Position.IsIncomeStatementPosition) {
                _incomeStatement.RemoveTransaction(transaction);
            }

            if (IsAssignable) Document.ReconciliationManager.UnassignTransaction(transaction);

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

            if (Transactions.Any(t => t.Position.Id == position.Id)) {
                result = ResourcesReconciliation.TransactionAssignmentErrorPositionIsAlreadyAssigned;
                return false;
            }
            
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