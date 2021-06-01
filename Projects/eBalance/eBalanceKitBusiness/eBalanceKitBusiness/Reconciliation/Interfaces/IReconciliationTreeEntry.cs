// --------------------------------------------------------------------------------
// author: Mirko Dibbert
// since: 2012-04-02
// copyright 2012 AvenDATA GmbH
// --------------------------------------------------------------------------------
using System.Collections.Generic;
using Taxonomy.Interfaces;

namespace eBalanceKitBusiness.Reconciliation.Interfaces {
    public interface IReconciliationTreeEntry : ISearchableNode {
        IEnumerable<IReconciliationTreeNode> Parents { get; }

        bool IsExpanded { get; set; }
        bool IsSelected { get; set; }

        double Order { get; }
    }
}