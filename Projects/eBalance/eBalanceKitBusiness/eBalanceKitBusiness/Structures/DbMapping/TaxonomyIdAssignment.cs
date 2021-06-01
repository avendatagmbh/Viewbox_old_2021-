// --------------------------------------------------------------------------------
// author: Mirko Dibbert
// since:  2011-06-13
// copyright 2011 AvenDATA GmbH
// --------------------------------------------------------------------------------
using DbAccess;

namespace eBalanceKitBusiness.Structures.DbMapping {
    /// <summary>
    /// Assignment of an internal id to a taxonomy id.
    /// </summary>
    [DbTable("taxonomy_ids", ForceInnoDb = true)]
    public class TaxonomyIdAssignment {
        #region constructor
        public TaxonomyIdAssignment() { } // needed for DbAccess
        internal TaxonomyIdAssignment(int taxonomyId, string xbrlElementId, string number) {
            TaxonomyId = taxonomyId;
            XbrlElementId = xbrlElementId;
            Number = number;
        }
        #endregion

        #region Id
        [DbColumn("id", AllowDbNull = false)]
        [DbPrimaryKey]
        internal int Id { get; private set; }
        #endregion  

        #region TaxonomyId
        [DbColumn("taxonomy_id")]
        [DbIndex("idx_taxonomy_ids__taxonomy_id")]
        public int TaxonomyId { get; private set; }
        #endregion

        #region XbrlElementId
        [DbColumn("xbrl_element_id", AllowDbNull = false, Length = 1024)]
        public string XbrlElementId { get; private set; }
        #endregion

        #region Number
        [DbColumn("number", AllowDbNull = false, Length = 10)]
        public string Number { get; private set; }
        #endregion
    }
}