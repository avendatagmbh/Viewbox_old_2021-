// --------------------------------------------------------------------------------
// author: Mirko Dibbert
// since: 2012-05-15
// copyright 2012 AvenDATA GmbH
// --------------------------------------------------------------------------------
using System.Collections.Generic;
using DbAccess;
using eBalanceKitBusiness.Structures.DbMapping;

namespace eBalanceKitBusiness.AuditCorrections.DbMapping {
    [DbTable("audit_correction", ForceInnoDb = true)]
    internal class DbEntityAuditCorrection : DbEntityBase<long> {

        public DbEntityAuditCorrection() { Transactions = new List<DbEntityAuditCorrectionTransaction>(); }

        [DbColumn("document_id", AllowDbNull = true, IsInverseMapping = true)]
        [DbIndex("idx_ac")]
        public Document Document { get; internal set; }

        [DbColumn("name", Length = 512)]
        public string Name { get; set; }

        [DbColumn("comment", Length = 8192)]
        public string Comment { get; set; }

        [DbCollection("DbEntityAuditCorrection")]
        public List<DbEntityAuditCorrectionTransaction> Transactions { get; private set; }
    }
}