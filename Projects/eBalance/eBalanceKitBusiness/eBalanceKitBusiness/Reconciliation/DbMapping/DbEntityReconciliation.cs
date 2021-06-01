// --------------------------------------------------------------------------------
// author: Mirko Dibbert
// since: 2012-03-06
// copyright 2012 AvenDATA GmbH
// --------------------------------------------------------------------------------
using System.Collections.Generic;
using DbAccess;
using eBalanceKitBusiness.Structures.DbMapping;

namespace eBalanceKitBusiness.Reconciliation.DbMapping {
    [DbTable("reconciliations", ForceInnoDb = true)]
    internal class DbEntityReconciliation : DbEntityBase<long> {

        /// <summary>
        /// DbMapping Load constructor.
        /// </summary>
        public DbEntityReconciliation() { Transactions = new List<DbEntityReconciliationTransaction>(); }

        /// <summary>
        /// Constructor for new reconciliations.
        /// </summary>
        /// <param name="document">Assigned document.</param>
        /// <param name="type">Type of the reconciliation.</param>
        /// <param name="transferKinds">Kind of the contained transactions (corresponds with XBRL element hbst.transfer.kind) </param>
        public DbEntityReconciliation(Document document, Enums.ReconciliationTypes type, Enums.TransferKinds transferKinds) {
            Transactions = new List<DbEntityReconciliationTransaction>();
            Document = document;
            ReconciliationType = type;
            TransferKinds = transferKinds;
        }

        [DbColumn("document_id", AllowDbNull = true, IsInverseMapping = true)]
        [DbIndex("idx_reconciliations")]
        public Document Document { get; internal set; }

        /// <summary>
        /// Type of the reconciliation dataset.
        /// </summary>
        [DbColumn("type")]
        public Enums.ReconciliationTypes ReconciliationType { get; private set; }

        /// <summary>
        /// Kind of the contained transactions.
        /// </summary>
        [DbColumn("transfer_kind")]
        public Enums.TransferKinds TransferKinds { get; set; }

        [DbColumn("name", Length = 512)]
        public string Name { get; set; }

        [DbColumn("comment", Length = 8192)]
        public string Comment { get; set; }

        [DbCollection("DbEntityReconciliation")]
        public List<DbEntityReconciliationTransaction> Transactions { get; private set; }
    }
}