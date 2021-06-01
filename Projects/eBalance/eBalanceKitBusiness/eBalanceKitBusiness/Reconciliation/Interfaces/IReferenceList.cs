// --------------------------------------------------------------------------------
// author: Gabor Bauer
// since: 2012-04-24
// copyright 2012 AvenDATA GmbH
// --------------------------------------------------------------------------------
using System.Collections.Generic;
using eBalanceKitBusiness.Structures.DbMapping;

namespace eBalanceKitBusiness.Reconciliation.Interfaces {
    public interface IReferenceList : IReferenceListManipulator {

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
        IEnumerable<IReferenceListItem> Items { get; }
    }
}
