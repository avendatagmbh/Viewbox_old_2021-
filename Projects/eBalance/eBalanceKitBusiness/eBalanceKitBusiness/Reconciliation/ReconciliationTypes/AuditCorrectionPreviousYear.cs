// --------------------------------------------------------------------------------
// author: Mirko Dibbert
// since: 2012-05-22
// copyright 2012 AvenDATA GmbH
// --------------------------------------------------------------------------------
using eBalanceKitBusiness.Reconciliation.DbMapping;
using eBalanceKitBusiness.Structures.DbMapping;

namespace eBalanceKitBusiness.Reconciliation.ReconciliationTypes {
    internal class AuditCorrectionPreviousYear : DeltaReconciliation {
        #region Constructors
        /// <summary>
        /// Constructor for new reconciliation.
        /// </summary>
        /// <param name="document">Assigned document.</param>
        internal AuditCorrectionPreviousYear(Document document)
            : base(document, Enums.ReconciliationTypes.AuditCorrectionPreviousYear) { }

        /// <summary>
        /// Constructor for existing reconciliation.
        /// </summary>
        /// <param name="dbEntityReconciliation">Assigned reconciliation.</param>
        internal AuditCorrectionPreviousYear(DbEntityReconciliation dbEntityReconciliation)
            : base(dbEntityReconciliation) { }
        #endregion // constructors         

        /// <summary>
        /// True, if transactions could be assigned to presentation tree nodes.
        /// </summary>
        internal override bool IsAssignable { get { return false; } }

    }
}