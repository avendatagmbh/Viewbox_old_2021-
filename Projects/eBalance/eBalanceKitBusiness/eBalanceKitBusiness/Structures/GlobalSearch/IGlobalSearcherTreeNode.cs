// --------------------------------------------------------------------------------
// author: Mirko Dibbert
// since: 2012-05-11
// copyright 2012 AvenDATA GmbH
// --------------------------------------------------------------------------------
using System.Collections.Generic;

namespace eBalanceKitBusiness.Structures.GlobalSearch {
    public interface IGlobalSearcherTreeNode {
        string Label { get; }
        IEnumerable<IGlobalSearcherTreeNode> Children { get; }
        bool IsSelected { get; set; }
        bool IsExpanded { get; set; }
    }
}