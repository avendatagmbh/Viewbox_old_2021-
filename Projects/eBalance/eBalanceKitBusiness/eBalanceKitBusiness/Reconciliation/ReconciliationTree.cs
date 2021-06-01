// --------------------------------------------------------------------------------
// author: Mirko Dibbert
// since: 2012-03-13
// copyright 2012 AvenDATA GmbH
// --------------------------------------------------------------------------------
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using eBalanceKitBusiness.Interfaces.PresentationTree;
using eBalanceKitBusiness.Reconciliation.Interfaces;
using eBalanceKitBusiness.Structures;
using eBalanceKitBusiness.Structures.DbMapping;

namespace eBalanceKitBusiness.Reconciliation {
    public class ReconciliationTree : Utils.NotifyPropertyChangedBase, IReconciliationTree {
         
        internal ReconciliationTree(TreeViewVisualOptions visualOptions, IPresentationTree ptree) {
            VisualOptions = visualOptions;
            Document = ptree.Document;
            foreach (IPresentationTreeNode root in ptree.RootEntries.OfType<IPresentationTreeNode>()) {
                _rootEntries.Add(new ReconciliationTreeNode(this, root));
            }

            foreach (var root in RootEntries) InitNodeDict(root);
            //Filter.ShowOnlyPositionsForSelectedLegalStatus = true;
        }

        #region ExpandSelectedCalled
        public event EventHandler ExpandSelectedCalled;
        private void OnExpandSelectedCalled() { if (ExpandSelectedCalled != null) ExpandSelectedCalled(this, new System.EventArgs()); }
        #endregion // ExpandSelectedCalled
        
        private void InitNodeDict(IReconciliationTreeNode root) {
            root.IsAssignedToReferenceList = IsSelectedForReferenceList(root);
            _nodesById[root.Element.Id] = root;
            foreach (var child in root.Children.OfType<IReconciliationTreeNode>()) InitNodeDict(child);
        }

        private readonly Dictionary<string, IReconciliationTreeNode> _nodesById =
            new Dictionary<string, IReconciliationTreeNode>();

        public Document Document { get; private set; }

        #region RootEntries
        private readonly List<IReconciliationTreeNode> _rootEntries = new List<IReconciliationTreeNode>();

        public IEnumerable<IReconciliationTreeNode> RootEntries { get { return _rootEntries; } }
        #endregion RootEntries

        #region Filter
        private PresentationTreeFilter _filter;

        public PresentationTreeFilter Filter { get { return _filter ?? (_filter = new PresentationTreeFilter(RootEntries)); } set { _filter = value; } }
        #endregion // Filter

        #region ResetFilter
        public void ResetFilter() {
            foreach (var root in RootEntries) {
                root.IsVisible = true;
                foreach (var child in root.Children.OfType<IReconciliationTreeNode>()) child.ResetFilter();
            }
        }
        #endregion // ResetFilter

        #region ExpandAllNodes
        public void ExpandAllNodes() {
            foreach (var root in RootEntries) {
                root.IsExpanded = true;
                foreach (var child in root.Children.OfType<IReconciliationTreeNode>()) child.ExpandAllChildren();
            }
        }
        #endregion ExpandAllNodes

        #region [ ExpandAllSelectedNodesForReferenceList ]

        public void ExpandAllSelectedNodesForReferenceList(IReconciliationTreeNode root) {
            bool hasSelectedChild = false;
            foreach (IReconciliationTreeNode node in root.Children.OfType<IReconciliationTreeNode>()) {
                if (node.ExpandAllChildrenForReferenceList()) {
                    hasSelectedChild = true;
                    node.IsExpanded = true;
                }
            }
            if (hasSelectedChild)
                root.IsExpanded = true;
        }

        #endregion [ ExpandAllSelectedNodesForReferenceList ]

        #region CollapseAllNodes
        public void CollapseAllNodes() {
            foreach (var root in RootEntries) {
                root.IsExpanded = false;
                foreach (var child in root.Children.OfType<IReconciliationTreeNode>()) child.CollapseAllChildren();
            }
        }
        #endregion // CollapseAllNodes

        #region UnselectAllNodes
        public void UnselectAllNodes() {
            foreach (var root in RootEntries) {
                root.IsExpanded = false;
                foreach (var child in root.Children.OfType<IReconciliationTreeNode>()) child.UnselectAllNodes();
            }
        }
        #endregion // UnselectAllNodes

        #region TreeViewVisualOptions
        public TreeViewVisualOptions VisualOptions { get; private set; }               
        #endregion // TreeViewVisualOptions

        public IEnumerator<IReconciliationTreeNode> GetEnumerator() { return RootEntries.GetEnumerator(); }
        IEnumerator IEnumerable.GetEnumerator() { return GetEnumerator(); }

        #region ExpandSelected
        public void ExpandSelected(IReconciliationTreeEntry entry) {

            IReconciliationTreeNode node;
            if (entry is IReconciliationTreeNode) {
                node = entry as IReconciliationTreeNode;
            } else {
                Debug.Assert(entry.Parents.Any(), "Item has no assigned parent!");
                node = entry.Parents.First();
            }

            //CollapseAllNodes();
            //UnselectAllNodes();
            var root = node;
            while (root != null) {
                bool foundParent = false;
                (root).IsExpanded = true;

                foreach (var parent in root.Parents.Where(parent => (parent as ReconciliationTreeNode).ReconciliationTree == this)) {
                    root = parent as ReconciliationTreeNode;
                    foundParent = true;
                    break;
                }
                //Debug.Assert(foundParent, "Parent was not found.");
                if (root.Parents.Count() != 0 && foundParent) continue;
                (root).IsExpanded = true;
                root = null;
            }

            entry.IsSelected = true;
            OnExpandSelectedCalled();
        }
        #endregion ExpandSelected

        public IReconciliationTreeNode GetNode(string id) {
            IReconciliationTreeNode result;
            _nodesById.TryGetValue(id, out result);
            return result;
        }

        public IReconciliationTreeNode GetNodeWithoutPrefix(string id) {
            string result =_nodesById.Keys.FirstOrDefault(n => n.EndsWith(id));
            if (result == null) return null;
            return _nodesById[result];
        }

        #region Reference list helper methods
        
        private bool IsSelectedForReferenceList(IReconciliationTreeNode treeNode) {
            if (treeNode.Value != null)
                return Document.ReconciliationManager.ReferenceList.IsElementContainedInReferenceList(treeNode.Value.DbValue.ElementId);
            else
                return false;
        }

        #endregion Reference list helper methods
    }
}