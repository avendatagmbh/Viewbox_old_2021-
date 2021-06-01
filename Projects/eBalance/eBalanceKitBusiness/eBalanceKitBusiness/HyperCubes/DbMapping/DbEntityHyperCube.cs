// --------------------------------------------------------------------------------
// author: Mirko Dibbert
// since: 2012-01-02
// copyright 2012 AvenDATA GmbH
// --------------------------------------------------------------------------------
using System;
using System.Collections.Generic;
using DbAccess;
using eBalanceKitBusiness.Structures;
using eBalanceKitBusiness.Structures.DbMapping;

namespace eBalanceKitBusiness.HyperCubes.DbMapping {
    [DbTable("hypercubes", ForceInnoDb = true)]
    internal class DbEntityHyperCube : DbEntityBase<int> {

        [DbColumn("document_id", AllowDbNull = true, IsInverseMapping = true)]
        [DbIndex("idx_hypercubes1")]
        [DbIndex("idx_hypercubes2")]
        internal Document Document { get; set; }

        [DbColumn("taxonomy_id", AllowDbNull = true)]
        [DbIndex("idx_hypercubes2")]
        public int TaxonomytId { get; set; }

        [DbColumn("cube_element_id", AllowDbNull = true)]
        [DbIndex("idx_hypercubes2")]
        public int CubeElementId { get; set; }

        [DbColumn("comment", AllowDbNull = true, Length = 4096)]
        public string Comment { get; set; }

        #region Items
        private List<DbEntityHyperCubeItem> _items;

        [DbCollection("HyperCube", LazyLoad = true)]
        public List<DbEntityHyperCubeItem> Items {
            get {
                if (_items == null) {
                    using (var conn = AppConfig.ConnectionManager.GetConnection()) {
                        Items = conn.DbMapping.Load<DbEntityHyperCubeItem>(conn.Enquote("hypercube_id") + " = " + Id + " AND " + conn.Enquote("document_id") + " = " + Document.Id);
                        foreach (var item in Items) {
                            item.HyperCube = this;
                            item.Document = Document;
                        }
                    }
                    return _items;
                }
                return _items;
            }
            private set { _items = value; }
        }

        #endregion Items
    }
}