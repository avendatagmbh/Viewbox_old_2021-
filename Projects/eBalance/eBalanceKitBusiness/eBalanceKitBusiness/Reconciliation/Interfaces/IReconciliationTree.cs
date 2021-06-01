// --------------------------------------------------------------------------------
// author: Mirko Dibbert
// since: 2012-03-13
// copyright 2012 AvenDATA GmbH
// --------------------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.ComponentModel;
using eBalanceKitBusiness.Interfaces;
using eBalanceKitBusiness.Structures.DbMapping;

namespace eBalanceKitBusiness.Reconciliation.Interfaces {
    public interface IReconciliationTree : ITaxonomyTree, IEnumerable<IReconciliationTreeNode>, INotifyPropertyChanged {

        event EventHandler ExpandSelectedCalled;

        Document Document { get; }
        IEnumerable<IReconciliationTreeNode> RootEntries { get; }
        TreeViewVisualOptions VisualOptions { get; }
        IReconciliationTreeNode GetNode(string id);
        IReconciliationTreeNode GetNodeWithoutPrefix(string id);

        void ExpandAllSelectedNodesForReferenceList(IReconciliationTreeNode node);
    }
}