// --------------------------------------------------------------------------------
// author: Mirko Dibbert
// since: 2012-01-20
// copyright 2012 AvenDATA GmbH
// --------------------------------------------------------------------------------
using System.Collections.Generic;
using eBalanceKitBusiness.HyperCubes.Interfaces.Structure;

namespace eBalanceKitBusiness.HyperCubes.ViewModels {
    internal class HyperCubeItemTreeNode : IHyperCubeItemTreeNode {

        internal HyperCubeItemTreeNode(IHyperCubeDimensionValue dimValue, IHyperCubeItem item) {
            DimensionValue = dimValue;
            Item = item;
        }

        #region Element
        private IHyperCubeDimensionValue DimensionValue { get; set; }
        #endregion

        #region Header
        public string Header { get { return DimensionValue.Element.Label; } }
        #endregion

        #region Item
        public IHyperCubeItem Item { get; private set; }
        #endregion

        #region Children
        private readonly List<IHyperCubeItemTreeNode> _children = new List<IHyperCubeItemTreeNode>();
        public IEnumerable<IHyperCubeItemTreeNode> Children { get { return _children; } }
        #endregion Children

        public bool HasChildren { get { return _children.Count > 0; } }

        internal void AddChildren(IHyperCubeItemTreeNode item) { _children.Add(item); }
    }
}