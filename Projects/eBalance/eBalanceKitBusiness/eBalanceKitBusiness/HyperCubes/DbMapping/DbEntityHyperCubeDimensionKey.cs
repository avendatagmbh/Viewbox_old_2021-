// --------------------------------------------------------------------------------
// author: Mirko Dibbert
// since:  2012-01-22
// copyright 2012 AvenDATA GmbH
// --------------------------------------------------------------------------------
using DbAccess;
using eBalanceKitBusiness.Structures.DbMapping;

namespace eBalanceKitBusiness.HyperCubes.DbMapping {
    /// <summary>
    /// Assignment of a uniqe id to each combination of dimensions members (used for assignment of hyper cube items).
    /// </summary>
    [DbTable("hypercube_dimension_keys", ForceInnoDb = true)]    
    internal class DbEntityHyperCubeDimensionKey : DbEntityBase<long> {

        [DbColumn("taxonomy_id", AllowDbNull = true)]
        [DbIndex("idx_hypercube_dimension_keys_taxid_cubeid")]
        public int TaxonomytId { get; set; }

        [DbColumn("cube_element_id")]
        [DbIndex("idx_hypercube_dimension_keys_taxid_cubeid")]        
        public int CubeElementId { get; set; }

        [DbColumn("primary_dimension_id", AllowDbNull = false)]
        internal int PrimaryDimensionId { get; set; }
        
        [DbColumn("dimension_id", AllowDbNull = false)]
        internal long DimensionId { get; set; }
    }
}