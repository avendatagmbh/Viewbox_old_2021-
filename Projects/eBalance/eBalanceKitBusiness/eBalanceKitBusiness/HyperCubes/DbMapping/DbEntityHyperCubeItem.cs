// --------------------------------------------------------------------------------
// author: Mirko Dibbert
// since: 2012-01-02
// copyright 2011 AvenDATA GmbH
// --------------------------------------------------------------------------------
using DbAccess;
using eBalanceKitBusiness.Structures.DbMapping;

namespace eBalanceKitBusiness.HyperCubes.DbMapping {

    [DbTable("hypercube_items", ForceInnoDb = true)]
    internal class DbEntityHyperCubeItem : DbEntityBase<long> {

        [DbColumn("hypercube_id", IsInverseMapping = true)]
        public DbEntityHyperCube HyperCube { get; set; }

        [DbColumn("document_id", IsInverseMapping = true)]
        public Document Document { get; set; }

        [DbColumn("dimension_key_id", AllowDbNull = false)]
        public long DimensionId { get; set; }

        [DbColumn("value", AllowDbNull = true, Length = 4096)]
        public string Value { get; set; }
    }
}