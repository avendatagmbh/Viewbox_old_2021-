// --------------------------------------------------------------------------------
// author: Gabor Bauer
// since: 2012-05-10
// copyright 2012 AvenDATA GmbH
// --------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DbAccess;

namespace eBalanceKitBusiness.Structures.DbMapping.BalanceList {
    [DbTable("account_reflists", ForceInnoDb = true)]
    internal class DbEntityAccountReferenceList : DbEntityBase<int> {
        /// <summary>
        /// DbMapping Load constructor.
        /// </summary>
        public DbEntityAccountReferenceList() { Items = new List<DbEntityAccountReferenceListItem>(); }

        /// <summary>
        /// Constructor for new reference list.
        /// </summary>
        /// <param name="document">Assigned document.</param>
        /// <param name="user">Assigned user.</param>
        public DbEntityAccountReferenceList(Document document, User user) {
            Items = new List<DbEntityAccountReferenceListItem>();
            Document = document;
            User = user;
        }

        [DbColumn("document_id", AllowDbNull = false, IsInverseMapping = true)]
        [DbIndex("idx_account_reflists_document_user")]
        public Document Document { get; internal set; }

        [DbColumn("user_id", AllowDbNull = false, IsInverseMapping = true)]
        [DbIndex("idx_account_reflists_document_user")]
        public User User { get; internal set; }

        [DbCollection("DbEntityAccountReferenceList")]
        public List<DbEntityAccountReferenceListItem> Items { get; private set; }
    }
}
