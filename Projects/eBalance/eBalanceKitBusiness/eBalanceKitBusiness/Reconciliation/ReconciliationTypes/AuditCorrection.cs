// --------------------------------------------------------------------------------
// author: Mirko Dibbert
// since: 2012-05-22
// copyright 2012 AvenDATA GmbH
// --------------------------------------------------------------------------------
using eBalanceKitBusiness.Reconciliation.DbMapping;
using eBalanceKitBusiness.Structures.DbMapping;

namespace eBalanceKitBusiness.Reconciliation.ReconciliationTypes {
    internal class AuditCorrection : DeltaReconciliation {
         
        #region Constructors
        /// <summary>
        /// Constructor for new reconciliation.
        /// </summary>
        /// <param name="document">Assigned document.</param>
        internal AuditCorrection(Document document)
            : base(document, Enums.ReconciliationTypes.AuditCorrection) { }

        /// <summary>
        /// Constructor for existing reconciliation.
        /// </summary>
        /// <param name="dbEntityReconciliation">Assigned reconciliation.</param>
        internal AuditCorrection(DbEntityReconciliation dbEntityReconciliation)
            : base(dbEntityReconciliation) { }
        #endregion // constructors
    }
}