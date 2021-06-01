// --------------------------------------------------------------------------------
// author: Mirko Dibbert
// since: 2012-03-20
// copyright 2012 AvenDATA GmbH
// --------------------------------------------------------------------------------
using System.Linq;
using Taxonomy;
using eBalanceKitBusiness.Manager;
using eBalanceKitBusiness.Reconciliation.DbMapping;
using eBalanceKitBusiness.Reconciliation.Interfaces.ReconciliationTypes;
using eBalanceKitBusiness.Structures.DbMapping;
using eBalanceKitResources.Localisation;

namespace eBalanceKitBusiness.Reconciliation.ReconciliationTypes {
    internal class ValueChange : DeltaReconciliation, IValueChange {

        #region Constructors
        /// <summary>
        /// Constructor for new reconciliation.
        /// </summary>
        /// <param name="document">Assigned document.</param>
        internal ValueChange(Document document)
            : base(document, Enums.ReconciliationTypes.ValueChange) {

            Comment = "Vorjahreswerte";
            DocumentManager.Instance.PropertyChanged += (sender, args) => {
                if (args.PropertyName == "HasPreviousYearReports")
                    OnPropertyChanged(args.PropertyName);
            };
        }

        /// <summary>
        /// Constructor for existing reconciliation.
        /// </summary>
        /// <param name="dbEntityReconciliation">Assigned reconciliation.</param>
        internal ValueChange(DbEntityReconciliation dbEntityReconciliation)
            : base(dbEntityReconciliation) { }
        #endregion // constructors           

        public override bool IsAssignmentAllowed(IElement position, out string result) {
            result = null;

            if (position == null) return true;

            if (Transactions.Any(t => t.Position.Id == position.Id)) {
                result = ResourcesReconciliation.TransactionAssignmentErrorPositionIsAlreadyAssigned;
                return false;
            }

            if (!position.IsIncomeStatementPosition && (BalanceListAssets.Transactions.Any() || BalanceListLiabilities.Transactions.Any())) {
                result = ResourcesReconciliation.TransactionAssignmentErrorOnlyOneBalanceSheetPosAllowed;
                return false;
            }

            if (
                Document.ReconciliationManager.Reclassifications.Any(
                    reclassification =>
                    reclassification.SourceElement != null && reclassification.SourceElement.Id == position.Id)) {
                result = ResourcesReconciliation.TransactionAssignmentErrorPositionIsReclassificationSource;
                return false;
            }

            return true;
        }

        internal override void Validate() {
            _warnings.Clear();

            // transactions existing?
            if (!Transactions.Any()) {
                _warnings.Add(ResourcesReconciliation.ReconciliationWarningNoAssignedTransaction);
                return;
            }

            // income statement transactions existing?
            if (!Transactions.Any(t => t.Position != null && t.Position.IsIncomeStatementPosition)) {
                _warnings.Add(ResourcesReconciliation.ReconciliationWarningNoAssignedISPosition);
            }

            // balance sheet position existing?
            if (!Transactions.Any(t => t.Position != null && (t.Position.IsBalanceSheetAssetsPosition || t.Position.IsBalanceSheetLiabilitiesPosition))) {
                _warnings.Add(ResourcesReconciliation.ReconciliationWarningNoAssignedBalanceSheetPosition);
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
    }
}