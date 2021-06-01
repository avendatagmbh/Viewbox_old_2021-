// --------------------------------------------------------------------------------
// author: Gabor Bauer
// since: 2012-05-10
// copyright 2012 AvenDATA GmbH
// --------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace eBalanceKitBusiness.Structures.DbMapping.BalanceList {
    public class AccountReferenceListItem : IAccountReferenceListItem {

        internal DbEntityAccountReferenceListItem DbEntity { get; set; }

        /// <summary>
        /// Public constructor for AccountReferenceListItem
        /// </summary>
        /// <param name="accountType">The account type.</param>
        /// <param name="accountId">The account id.</param>
        public AccountReferenceListItem(AccountTypeEnum accountType, long accountId) {
            AccountType = accountType;
            AccountId = accountId;
        }
        /// <summary>
        /// Constructor for existing account reference list item.
        /// </summary>
        /// <param name="dbEntityAccountReferenceListItem">Assigned element.</param>
        internal AccountReferenceListItem(DbEntityAccountReferenceListItem dbEntityAccountReferenceListItem) {
            AccountId = dbEntityAccountReferenceListItem.AccountId;
            AccountType = dbEntityAccountReferenceListItem.AccountType;
            DbEntity = dbEntityAccountReferenceListItem;
        }

        /// <inheritdoc />
        public AccountTypeEnum AccountType { get; private set; }
        /// <inheritdoc />
        public long AccountId { get; private set; }

        private long _hash = 0;
        public long Hash {
            get {
                if (_hash == 0)
                    _hash = GetHash(this);
                return _hash;
            }
        }

        public static long GetHash(IAccountReferenceListItem item) {
            return (int)item.AccountType ^ item.AccountId;
        }

        public static long GetHash(AccountTypeEnum accountType, long accountId) {
            return (int)accountType ^ accountId;
        }
    }
}
