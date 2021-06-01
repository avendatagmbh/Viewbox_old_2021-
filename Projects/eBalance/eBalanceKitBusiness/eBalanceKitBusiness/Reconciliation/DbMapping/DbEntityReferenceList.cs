// --------------------------------------------------------------------------------
// author: Gabor Bauer
// since: 2012-04-24
// copyright 2012 AvenDATA GmbH
// --------------------------------------------------------------------------------
using System.Collections.Generic;
using DbAccess;
using eBalanceKitBusiness.Structures.DbMapping;

namespace eBalanceKitBusiness.Reconciliation.DbMapping {
    [DbTable("reconciliation_reflists", ForceInnoDb = true)]
    internal class DbEntityReferenceList : DbEntityBase<long> {
        
        /// <summary>
        /// DbMapping Load constructor.
        /// </summary>
        public DbEntityReferenceList() { Items = new List<DbEntityReferenceListItem>(); }

        /// <summary>
        /// Constructor for new reference list.
        /// </summary>
        /// <param name="document">Assigned document.</param>
        /// <param name="user">Assigned user.</param>
        public DbEntityReferenceList(Document document, User user) {
            Items = new List<DbEntityReferenceListItem>();
            Document = document;
            User = user;
        }

        [DbColumn("document_id", AllowDbNull = false, IsInverseMapping = true)]
        [DbIndex("idx_reconciliation_reflists_document_user")]
        public Document Document { get; internal set; }

        [DbColumn("user_id", AllowDbNull = false, IsInverseMapping = true)]
        [DbIndex("idx_reconciliation_reflists_document_user")]
        public User User { get; internal set; }

        [DbCollection("DbEntityReferenceList")]
        public List<DbEntityReferenceListItem> Items { get; private set; }
    }
}
