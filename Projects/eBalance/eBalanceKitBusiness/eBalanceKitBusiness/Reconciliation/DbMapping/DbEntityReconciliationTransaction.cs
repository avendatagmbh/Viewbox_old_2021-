// --------------------------------------------------------------------------------
// author: Mirko Dibbert
// since: 2012-03-06
// copyright 2012 AvenDATA GmbH
// --------------------------------------------------------------------------------
using DbAccess;
using eBalanceKitBusiness.Reconciliation.Enums;
using eBalanceKitBusiness.Structures.DbMapping;

namespace eBalanceKitBusiness.Reconciliation.DbMapping {
    [DbTable("reconciliation_transactions", ForceInnoDb = true)]
    internal class DbEntityReconciliationTransaction : DbEntityBase<long> {

        /// <summary>
        /// DbMapping Load constructor.
        /// </summary>
        public DbEntityReconciliationTransaction() { }

        /// <summary>
        /// Constructor for new transactions.
        /// </summary>
        /// <param name="document">Assigned Document.</param>
        /// <param name="dbEntityReconciliation">Assigned reconciliation entity.</param>
        /// <param name="transactionType">Assigned transaction type.</param>
        /// <param name="elementId">Assigned element id.</param>
        internal DbEntityReconciliationTransaction(Document document, DbEntityReconciliation dbEntityReconciliation, TransactionTypes transactionType, int elementId) {
            Document = document;
            DbEntityReconciliation = dbEntityReconciliation;
            TransactionType = transactionType;
            ElementId = elementId;
        }

        [DbColumn("document_id", AllowDbNull = true, IsInverseMapping = true)]
        [DbIndex("idx_reconciliation_trans")]
        public Document Document { get; internal set; }

        [DbColumn("reconciliation_id", AllowDbNull = true, IsInverseMapping = true)]
        public DbEntityReconciliation DbEntityReconciliation { get; private set; }

        [DbColumn("transaction_type")]
        public TransactionTypes TransactionType { get; set; }

        /// <summary>
        /// Assigned element id (internal id).
        /// </summary>
        [DbColumn("element_id")]
        internal int ElementId { get; set; }

        [DbColumn("value")]
        public decimal Value { get; set; }
    }
}