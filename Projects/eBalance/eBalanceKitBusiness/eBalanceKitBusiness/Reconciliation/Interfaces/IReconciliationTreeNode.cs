// --------------------------------------------------------------------------------
// author: Mirko Dibbert
// since: 2012-03-13
// copyright 2012 AvenDATA GmbH
// --------------------------------------------------------------------------------
using System.Collections.Generic;
using Taxonomy;
using Taxonomy.Interfaces;
using eBalanceKitBusiness.Interfaces;

namespace eBalanceKitBusiness.Reconciliation.Interfaces {
    public interface IReconciliationTreeNode : ITaxonomyTreeNode, IReconciliationTreeEntry, IEnumerable<IReconciliationTreeEntry> {

        IReconciliationTree ReconciliationTree { get; }
        IValueTreeEntry Value { get; }

        IEnumerable<IReconciliationTreeEntry> Children { get; }

        bool IsVisible { get; set; }
        bool IsAssignedToReferenceList { get; set; }

        void ExpandAllChildren();
        void CollapseAllChildren();
        void UnselectAllNodes();

        bool ValidationWarning { get; }
        string ValidationWarningMessage { get; }
        bool ValidationError { get; }
        string ValidationErrorMessage { get; }

        bool ExpandAllChildrenForReferenceList();
    }
}