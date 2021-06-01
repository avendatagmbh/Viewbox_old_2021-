// --------------------------------------------------------------------------------
// author: Gabor Bauer
// since: 2012-04-24
// copyright 2012 AvenDATA GmbH
// --------------------------------------------------------------------------------
using System.Collections.Generic;
using DbAccess;
using eBalanceKitBusiness.Reconciliation.Enums;
using eBalanceKitBusiness.Structures.DbMapping;

namespace eBalanceKitBusiness.Reconciliation.DbMapping {
    [DbTable("reconciliation_reflist_items", ForceInnoDb = true)]
    internal class DbEntityReferenceListItem : DbEntityBase<long> {
        /// <summary>
        /// DbMapping Load constructor.
        /// </summary>
        public DbEntityReferenceListItem() {  }

        /// <summary>
        /// Constructor for new reference list item.
        /// </summary>
        /// <param name="dbEntityReferenceList">Assigned reference list.</param>
        /// <param name="elementId">Assigned element id.</param>
        public DbEntityReferenceListItem(DbEntityReferenceList dbEntityReferenceList, int elementId) {
            DbEntityReferenceList = dbEntityReferenceList;
            ElementId = elementId;
        }

        [DbColumn("reconciliation_reflist_id", AllowDbNull = true, IsInverseMapping = true)]
        [DbIndex("idx_r_reflist_items")]
        public DbEntityReferenceList DbEntityReferenceList { get; private set; }

        /// <summary>
        /// Assigned element id (taxonomy id).
        /// </summary>
        [DbColumn("element_id")]
        [DbIndex("idx_r_refelist_items_elementid")]
        internal int ElementId { get; set; }
    }
}
