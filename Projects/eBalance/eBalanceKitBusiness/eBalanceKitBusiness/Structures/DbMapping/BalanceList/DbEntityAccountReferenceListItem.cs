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
    [DbTable("account_reflist_items", ForceInnoDb = true)]
    internal class DbEntityAccountReferenceListItem : DbEntityBase<int> {
        /// <summary>
        /// DbMapping Load constructor.
        /// </summary>
        public DbEntityAccountReferenceListItem() {  }

        /// <summary>
        /// Constructor for new reference list item.
        /// </summary>
        /// <param name="dbEntityReferenceList">Assigned reference list.</param>
        /// <param name="accountType">Assigned account type.</param>
        /// <param name="accountId">Assigned account id.</param>
        public DbEntityAccountReferenceListItem(DbEntityAccountReferenceList dbEntityAccountReferenceList, AccountTypeEnum accountType, long accountId) {
            DbEntityAccountReferenceList = dbEntityAccountReferenceList;
            AccountType_Internal = (int)accountType;
            AccountId = accountId;
        }

        [DbColumn("account_reflist_id", AllowDbNull = true, IsInverseMapping = true)]
        [DbIndex("idx_account_reflist")]
        public DbEntityAccountReferenceList DbEntityAccountReferenceList { get; private set; }

        /// <summary>
        /// Assigned account type.
        /// </summary>
        internal AccountTypeEnum AccountType {
            get {
                return (AccountTypeEnum)AccountType_Internal;
            }
        }

        /// <summary>
        /// Assigned account type.
        /// </summary>
        [DbColumn("account_type")]
        [DbIndex("idx_account")]
        internal int AccountType_Internal { get; set; }

        /// <summary>
        /// Assigned account id.
        /// </summary>
        [DbColumn("account_id")]
        [DbIndex("idx_account")]
        internal long AccountId { get; set; }
    }
}
