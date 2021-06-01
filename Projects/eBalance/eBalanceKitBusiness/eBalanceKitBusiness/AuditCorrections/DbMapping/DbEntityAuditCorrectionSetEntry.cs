// --------------------------------------------------------------------------------
// author: Mirko Dibbert
// since: 2012-06-07
// copyright 2012 AvenDATA GmbH
// --------------------------------------------------------------------------------
using DbAccess;
using eBalanceKitBusiness.Structures.DbMapping;

namespace eBalanceKitBusiness.AuditCorrections.DbMapping {
    [DbTable("audit_correction_set", ForceInnoDb = true)]
    internal class DbEntityAuditCorrectionSetEntry : DbEntityBase<long> {

        #region SetId
        [DbColumn("set_id", AllowDbNull = false)]
        [DbIndex("idx_audit_correction_set_1")]
        public long SetId { get; set; }
        #endregion // SetId

        #region CorrectionId
        [DbColumn("correction_id", AllowDbNull = false)]
        [DbIndex("idx_audit_correction_set_2")]
        public long CorrectionId { get; set; }
        #endregion // CorrectionId
    }
}