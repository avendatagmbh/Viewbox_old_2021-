// --------------------------------------------------------------------------------
// author: Gabor Bauer
// since: 2012-04-24
// copyright 2012 AvenDATA GmbH
// --------------------------------------------------------------------------------

using eBalanceKitBusiness.Reconciliation.DbMapping;
using eBalanceKitBusiness.Reconciliation.Interfaces;

namespace eBalanceKitBusiness.Reconciliation {
    public class ReferenceListItem : IReferenceListItem {

        internal DbEntityReferenceListItem DbEntity { get; set; }

        /// <summary>
        /// Public constructor for ReferenceListItem
        /// </summary>
        /// <param name="elementId">The element id.</param>
        public ReferenceListItem(int elementId) {
            ElementId = elementId;
        }
        /// <summary>
        /// Constructor for existing reference list item.
        /// </summary>
        /// <param name="dbEntityReferenceListItem">Assigned element.</param>
        internal ReferenceListItem(DbEntityReferenceListItem dbEntityReferenceListItem) {
            ElementId = dbEntityReferenceListItem.ElementId;
            DbEntity = dbEntityReferenceListItem;
        }

        /// <inheritdoc />
        public int ElementId { get; private set; }
    }
}
