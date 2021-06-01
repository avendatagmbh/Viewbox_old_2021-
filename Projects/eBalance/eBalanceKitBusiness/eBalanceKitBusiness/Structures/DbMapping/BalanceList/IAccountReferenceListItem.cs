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
    public interface IAccountReferenceListItem {
        /// <summary>
        /// The type of the account
        /// </summary>
        AccountTypeEnum AccountType { get; }
        /// <summary>
        /// The id of the account.
        /// </summary>
        long AccountId { get; }
        /// <summary>
        /// The hash of the IAccountReferenceListItem.
        /// </summary>
        long Hash { get; }
    }
}
