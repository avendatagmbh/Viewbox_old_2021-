// -------------------------------------------------------------------------------
// author: Mirko Dibbert
// since: 2012-03-20
// copyright 2012 AvenDATA GmbH
// --------------------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Linq;
using Taxonomy;
using eBalanceKitBusiness.Exceptions;
using eBalanceKitBusiness.Localisation;
using eBalanceKitBusiness.Reconciliation.DbMapping;
using eBalanceKitBusiness.Reconciliation.Enums;
using eBalanceKitBusiness.Reconciliation.Interfaces;
using eBalanceKitBusiness.Reconciliation.Interfaces.ReconciliationTypes;
using eBalanceKitBusiness.Structures.DbMapping;
using eBalanceKitResources.Localisation;

namespace eBalanceKitBusiness.Reconciliation.ReconciliationTypes {
    internal class Reclassification : Reconciliation, IReclassification {

        #region constructors
        /// <summary>
        /// Constructor for new reconciliation.
        /// </summary>
        /// <param name="document">Assigned document.</param>
        internal Reclassification(Document document) : base(document, Enums.ReconciliationTypes.Reclassification) {
            Save();
            SourceTransaction = CreateTransaction(TransactionTypes.Source);
            DestinationTransaction = CreateTransaction(TransactionTypes.Destination);
        }

        /// <summary>
        /// Constructor for existing reconciliation.
        /// </summary>
        /// <param name="dbEntityReconciliation">Assigned reconciliation.</param>
        internal Reclassification(DbEntityReconciliation dbEntityReconciliation) : base(dbEntityReconciliation) {
            SourceTransaction = CreateTransaction(
                DbEntity.Transactions.FirstOrDefault(t => t.TransactionType == TransactionTypes.Source));

            DestinationTransaction = CreateTransaction(
                DbEntity.Transactions.FirstOrDefault(t => t.TransactionType == TransactionTypes.Destination));

        }
        #endregion // constructors

        internal override void Validate() {
            base.Validate();
            OnPropertyChanged("IsValid");
        }

        #region SourceElement
        public IElement SourceElement {
            get { return SourceTransaction == null ? null : SourceTransaction.Position; }
            set {
                if (!Document.ReportRights.WriteTransferValuesAllowed)
                    throw new AccessDeniedException(ExceptionMessages.InsufficentWriteRights);

                string result;
                if (!IsAssignmentAllowed(value, out result)) return;
                
                SourceTransaction.Position = value;
                OnPropertyChanged("SourceElement");
            }
        }
        #endregion // SourceElement

        #region SourceTransaction
        private IReconciliationTransaction _sourceTransaction;

        public IReconciliationTransaction SourceTransaction {
            get { return _sourceTransaction; }
            private set {
                if (_sourceTransaction == value) return;

                if (_sourceTransaction != null) _sourceTransaction.Validated -= TransactionOnValidated;
                _sourceTransaction = value;                
                if (_sourceTransaction != null) _sourceTransaction.Validated += TransactionOnValidated;
                
                OnPropertyChanged("SourceTransaction");
            }
        }

        #endregion // SourceTransaction
        
        #region DestinationElement
        public IElement DestinationElement {
            get { return DestinationTransaction == null ? null : DestinationTransaction.Position; }
            set {
                if (!Document.ReportRights.WriteTransferValuesAllowed)
                    throw new AccessDeniedException(ExceptionMessages.InsufficentWriteRights);

                string result;
                if (!IsAssignmentAllowed(value, out result)) return;

                DestinationTransaction.Position = value;
                OnPropertyChanged("DestinationElement");
            }
        }
        #endregion // DestinationElement

        #region DestinationTransaction
        private IReconciliationTransaction _destinationTransaction;

        public IReconciliationTransaction DestinationTransaction {
            get { return _destinationTransaction; }
            private set {
                if (_destinationTransaction == value) return;
                if (_destinationTransaction != null) _destinationTransaction.Validated -= TransactionOnValidated;
                _destinationTransaction = value;
                if (_destinationTransaction != null) _destinationTransaction.Validated += TransactionOnValidated; 
                OnPropertyChanged("DestinationTransaction");
            }
        }
        #endregion // DestinationTransaction

        #region Transactions
        /// <summary>
        /// Returns an enumeration of all assigned transactions.
        /// </summary>
        public override IEnumerable<IReconciliationTransaction> Transactions { get { return new[] { SourceTransaction, DestinationTransaction }; } }
        #endregion // Transactions

        #region RemoveTransaction
        public override void RemoveTransaction(IReconciliationTransaction transaction) {
            if (!Document.ReportRights.WriteTransferValuesAllowed)
                throw new AccessDeniedException(ExceptionMessages.InsufficentWriteRights);

            Document.ReconciliationManager.UnassignTransaction(transaction);
            transaction.Position = null;

            switch (transaction.TransactionType) {
                case TransactionTypes.Source:
                    OnPropertyChanged("SourceElement");
                    break;

                case TransactionTypes.Destination:
                    OnPropertyChanged("DestinationElement");
                    break;
            }
        }

        #endregion // RemoveTransaction

        public override bool IsAssignmentAllowed(IElement position, out string result) {
            result = null;

            if (position == null) return true;

            if (Document.ReconciliationManager.Reclassifications.Any(reclassification => reclassification.SourceElement != null && reclassification.SourceElement.Id == position.Id)) {
                result = ResourcesReconciliation.TransactionAssignmentErrorPositionIsReclassificationSource;
                return false;
            }

            if (SourceElement != null) {
                if (SourceElement.Id == position.Id) {
                    result = ResourcesReconciliation.TransactionAssignmentErrorPositionIsAlreadyAssigned; 
                    return false;
                }

                if (SourceElement.IsBalanceSheetAssetsPosition && !position.IsBalanceSheetAssetsPosition) {
                    result = ResourcesReconciliation.TransactionAssignmentErrorReclassivicationIllegalGroup;
                    return false;
                }
                
                if (SourceElement.IsBalanceSheetLiabilitiesPosition && !position.IsBalanceSheetLiabilitiesPosition) {
                    result = ResourcesReconciliation.TransactionAssignmentErrorReclassivicationIllegalGroup;
                    return false;
                }

                if (SourceElement.IsIncomeStatementPosition && !position.IsIncomeStatementPosition) {
                    result = ResourcesReconciliation.TransactionAssignmentErrorReclassivicationIllegalGroup;
                    return false;
                }
            }

            if (DestinationElement != null) {
                if (DestinationElement.Id == position.Id) {
                    result = ResourcesReconciliation.TransactionAssignmentErrorPositionIsAlreadyAssigned;
                    return false;
                }

                if (DestinationElement.IsBalanceSheetAssetsPosition && !position.IsBalanceSheetAssetsPosition) {
                    result = ResourcesReconciliation.TransactionAssignmentErrorReclassivicationIllegalGroup;
                    return false;
                }
                
                if (DestinationElement.IsBalanceSheetLiabilitiesPosition && !position.IsBalanceSheetLiabilitiesPosition) {
                    result = ResourcesReconciliation.TransactionAssignmentErrorReclassivicationIllegalGroup;
                    return false;
                }
                
                if (DestinationElement.IsIncomeStatementPosition && !position.IsIncomeStatementPosition) {
                    result = ResourcesReconciliation.TransactionAssignmentErrorReclassivicationIllegalGroup;
                    return false;
                }
            }
            return true;
        }

        private void TransactionOnValidated(object sender, System.EventArgs eventArgs) { Validate(); }

    }
}