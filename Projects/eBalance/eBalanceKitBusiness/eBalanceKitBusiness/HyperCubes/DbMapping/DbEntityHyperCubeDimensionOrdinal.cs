// --------------------------------------------------------------------------------
// author: Mirko Dibbert
// since:  2012-01-22
// copyright 2012 AvenDATA GmbH
// --------------------------------------------------------------------------------
using DbAccess;
using eBalanceKitBusiness.Structures.DbMapping;

namespace eBalanceKitBusiness.HyperCubes.DbMapping {
    /// <summary>
    /// Assignment of ordinal numbers to hypercube elements (unique per hypercube dimension element).
    /// </summary>
    [DbTable("hyper_cube_dimension_ordinals", ForceInnoDb = true)]
    internal class DbEntityHyperCubeDimensionOrdinal : DbEntityBase<int> {

        [DbColumn("taxonomy_id")]
        [DbIndex("idx_hc_dimension_ordinals")]
        public int TaxonomyId { get; set; }

        [DbColumn("cube_element_id")]
        [DbIndex("idx_hc_dimension_ordinals")]
        public int CubeElementId { get; set; }

        [DbColumn("dimension_element_id", AllowDbNull = false)]
        internal int DimensionElementId { get; set; }

        [DbColumn("ordinal", AllowDbNull = false)]
        internal int Ordinal { get; set; }
         
    }
}