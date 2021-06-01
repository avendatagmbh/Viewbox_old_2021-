// --------------------------------------------------------------------------------
// author: Gabor Bauer
// since: 2012-04-24
// copyright 2012 AvenDATA GmbH
// --------------------------------------------------------------------------------
using System.Collections.Generic;
using eBalanceKitBusiness.Structures.DbMapping;

namespace eBalanceKitBusiness.Reconciliation.Interfaces {
    public interface IReferenceListItem {
        /// <summary>
        /// The id of the element.
        /// </summary>
        int ElementId { get; }
    }
}
