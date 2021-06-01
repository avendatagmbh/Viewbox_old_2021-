// --------------------------------------------------------------------------------
// author: Mirko Dibbert
// since: 2012-01-25
// copyright 2012 AvenDATA GmbH
// --------------------------------------------------------------------------------
using Taxonomy;
using eBalanceKitBusiness.HyperCubes.DbMapping;

namespace eBalanceKitBusiness.HyperCubes.Structures.HyperCubeItem {
    internal class HyperCubeStringItem : HyperCubeItemBase {

        internal HyperCubeStringItem(HyperCubeItemCollection items, DbEntityHyperCubeItem dbEntity, IElement primaryDimensionValue)
            : base(items, dbEntity, primaryDimensionValue) { }
         
    }
}