// --------------------------------------------------------------------------------
// author: Mirko Dibbert
// since: 2012-01-25
// copyright 2012 AvenDATA GmbH
// --------------------------------------------------------------------------------
using Taxonomy;

namespace eBalanceKitBusiness.HyperCubes.Structures.HyperCubeItem {
    internal class HyperCubeAbstractItem : HyperCubeItemBase {

        internal HyperCubeAbstractItem(HyperCubeItemCollection items, IElement primaryDimensionValue)
            : base(items, null, primaryDimensionValue) { }

        public override object Value { get { return null; } set { } }
    }
}