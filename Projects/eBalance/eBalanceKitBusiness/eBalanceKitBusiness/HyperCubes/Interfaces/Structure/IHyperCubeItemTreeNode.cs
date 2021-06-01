// --------------------------------------------------------------------------------
// author: Mirko Dibbert
// since: 2012-01-20
// copyright 2012 AvenDATA GmbH
// --------------------------------------------------------------------------------
using System.Collections.Generic;

namespace eBalanceKitBusiness.HyperCubes.Interfaces.Structure {
    public interface IHyperCubeItemTreeNode {
        string Header { get; }
        IHyperCubeItem Item { get; }
        IEnumerable<IHyperCubeItemTreeNode> Children { get; }
        bool HasChildren { get; }
    }
}