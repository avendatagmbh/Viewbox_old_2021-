// --------------------------------------------------------------------------------
// author: Mirko Dibbert
// since: 2012-05-15
// copyright 2012 AvenDATA GmbH
// --------------------------------------------------------------------------------
using DbAccess;
using eBalanceKitBusiness.Reconciliation.Enums;
using eBalanceKitBusiness.Structures.DbMapping;

namespace eBalanceKitBusiness.AuditCorrections.DbMapping {
    [DbTable("audit_correction_transaction", ForceInnoDb = true)]
    internal class DbEntityAuditCorrectionTransaction : DbEntityBase<long> {

        [DbColumn("document_id", AllowDbNull = true, IsInverseMapping = true)]
        [DbIndex("idx_audit")]
        public Document Document { get; set; }

        [DbColumn("head_id", AllowDbNull = true, IsInverseMapping = true)]
        public DbEntityAuditCorrection DbEntityAuditCorrection { get; set; }

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