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
    public interface IAccountReferenceList : IAccountReferenceListManipulator {

        /// <summary>
        /// The document assigne to the reference list.
        /// </summary>
        Document Document { get; }
        /// <summary>
        /// The user assigned to the reference list.
        /// </summary>
        User User { get; }
        /// <summary>
        /// Returns an enumeration of all assigned elements.
        /// </summary>
        IEnumerable<IAccountReferenceListItem> Items { get; }
    }
}
