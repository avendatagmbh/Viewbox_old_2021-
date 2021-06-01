// --------------------------------------------------------------------------------
// author: Mirko Dibbert
// since:  2011-06-25
// copyright 2011 AvenDATA GmbH
// --------------------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Taxonomy;
using Taxonomy.Interfaces.PresentationTree;
using eBalanceKitBusiness.Interfaces;
using eBalanceKitBusiness.Structures.DbMapping;
using IPresentationTreeNode = eBalanceKitBusiness.Interfaces.PresentationTree.IPresentationTreeNode;

namespace eBalanceKitBusiness.Structures.Presentation {
    /// <summary>
    /// Extension of presentation tree for gui presentation purposes.
    /// </summary>
    public class PresentationTree : Taxonomy.PresentationTree.PresentationTree, IPresentationTree {
        private PresentationTree(Taxonomy.Interfaces.PresentationTree.IPresentationTree ptree) : base(ptree) { 
            //_filter = new PresentationTreeFilter(ptree.RootEntries) {
            //    ShowOnlyPositionsForSelectedLegalStatus = true
            //};
        }

        internal PresentationTree(IPresentationTree ptree, Taxonomy.Interfaces.PresentationTree.IPresentationTreeNode root) : base(ptree) {
            Document = ptree.Document;

            List<Taxonomy.Interfaces.PresentationTree.IPresentationTreeNode> nodes = new List<Taxonomy.Interfaces.PresentationTree.IPresentationTreeNode>();
            Action<Taxonomy.Interfaces.PresentationTree.IPresentationTreeNode> getAllSubnodes = null;
            getAllSubnodes = (currentRoot) => {
                nodes.Add(currentRoot);
                foreach (var child in currentRoot.Children.OfType<Taxonomy.Interfaces.PresentationTree.IPresentationTreeNode>())
                    getAllSubnodes(child);
            };

            getAllSubnodes(root);
            InitNodes(nodes, root);

            _filter = new PresentationTreeFilter(RootEntries.OfType<ITaxonomyTreeNode>());
            
            foreach (PresentationTreeNode node in Nodes) {
                node.PropertyChanged += VisibleValuePropertyChanged;
                
                var presentationTreeNode = node;
                if (presentationTreeNode != null && presentationTreeNode.Value != null)
                    presentationTreeNode.Value.PropertyChanged += VisibleValuePropertyChanged;
            }
        }

        internal void VisibleValuePropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e) {
            if (e.PropertyName.Equals("IsVisible") || e.PropertyName.Equals("HasValue") || e.PropertyName.Equals("ContainsHiddenValue")) {
                
                // Fire an OnPropertyChanged but unfortunately there is no eventhandler attached to this PresentationTree
                OnPropertyChanged("ContainsHiddenValue");
                // so we have to fire the event for Node[].PresentationTree as well
                // but only if the sender is not the original PresentationTree (otherwise we would have an endless loop)
                if (!(sender is PresentationTree)) {
                    var node = Nodes.FirstOrDefault();
                    if (node != null && node.PresentationTree is PresentationTree) {
                        (node.PresentationTree as PresentationTree).OnPropertyChanged("ContainsHiddenValue");
                    }
                }
            }
        }

        ~PresentationTree() {
            // Remove the event handler
            foreach (var node in Nodes) {
                node.PropertyChanged -= VisibleValuePropertyChanged;
                var presentationTreeNode = node as PresentationTreeNode;
                if (presentationTreeNode != null && presentationTreeNode.Value != null)
                    presentationTreeNode.Value.PropertyChanged -= VisibleValuePropertyChanged;
            }
        }

        #region NodesByInternalId
        private readonly Dictionary<int, IPresentationTreeNode> _nodesByInternalId =
            new Dictionary<int, IPresentationTreeNode>();

        /// <summary>
        /// Dictionary of all contained nodes, accessed by internal id.
        /// </summary>
        private Dictionary<int, IPresentationTreeNode> NodesByInternalId { get { return _nodesByInternalId; } }
        #endregion

        internal static Dictionary<string, IPresentationTree> CreatePresentationTrees(
            ITaxonomy taxonomy, ValueTree.ValueTree vtree, IEnumerable<IBalanceList> balanceLists) {
            var ptrees = new Dictionary<string, IPresentationTree>();

            // create node dictonary
            var nodes = new Dictionary<IPresentationTreeEntry, IPresentationTreeNode>();

            List<IBalanceList> balanceListsList = balanceLists != null ? balanceLists.ToList() : null;

            // create new presentation trees
            foreach (var ptree in taxonomy.PresentationTrees) {
                var newPtree = new PresentationTree(ptree);
                ptrees[ptree.Role.RoleUri] = newPtree;

                var ptreeNodes = new List<IPresentationTreeNode>();
                foreach (var root in ptree.RootEntries) {
                    CopyPresentationTree(newPtree, root, ptreeNodes, nodes);
                }

                // copy structure
                foreach (Taxonomy.Interfaces.PresentationTree.IPresentationTreeNode node in ptree.Nodes) {
                    if (!nodes.ContainsKey(node)) continue;
                    
                    var newNode = nodes[node];
                    foreach (IPresentationTreeEntry child in node.Children) {                       
                        if (!nodes.ContainsKey(child)) continue;
                        newNode.AddChildren(nodes[child]);
                    }
                }

                newPtree.InitNodes(ptreeNodes);

                // assign node values
                if (vtree != null) {
                    newPtree._nodesByInternalId.Clear();

                    foreach (IPresentationTreeNode node in ptreeNodes) {
                        newPtree.NodesByInternalId[vtree.Document.TaxonomyIdManager.GetId(node.Element.Id)] = node;
                    }

                    foreach (IPresentationTreeNode node in ptreeNodes)
                        node.Value = vtree.GetValue(node.Element.Id);
                    newPtree.Document = vtree.Document;
                }

                // assign accounts
                if (balanceListsList != null)
                    foreach (
                        IBalanceListEntry item in
                            balanceListsList.SelectMany(
                                balanceList =>
                                balanceList.AssignedItems.Where(
                                    item => newPtree.HasNode(item.AssignedElement.Id))))
                        newPtree.GetNode(item.AssignedElement.Id).AddChildren(item);
            }

            // assign Element.PresentationTreeNodes values
            foreach (Taxonomy.Interfaces.PresentationTree.IPresentationTree ptree in taxonomy.PresentationTrees) {
                foreach (Taxonomy.Interfaces.PresentationTree.IPresentationTreeNode node in ptree.Nodes) {

                    if (!nodes.ContainsKey(node)) continue;
                    
                    foreach (
                        Taxonomy.Interfaces.PresentationTree.IPresentationTreeNode elemNode in
                            node.Element.PresentationTreeNodes) {

                        if (!nodes.ContainsKey(elemNode)) continue;
                        nodes[node].Element.PresentationTreeNodes.Add(nodes[elemNode]);
                    }
                }
            }

            // expand root nodes
            foreach (
                Taxonomy.Interfaces.PresentationTree.IPresentationTreeNode root in
                    ptrees.Values.SelectMany(ptree => ptree.RootEntries))
                ((IPresentationTreeNode) root).IsExpanded = true;

            /*
            foreach (IPresentationTree presentationTree in ptrees.Values) {
                
                foreach (KeyValuePair<IPresentationTreeEntry, IPresentationTreeNode> keyValuePair in nodes) {
                    //if (keyValuePair.Key.Element.LegalFormBNaU && Document.GcdTaxonomyInfoId == ) {
                    //Document.MainTaxonomyInfo  
                    //foreach (IPresentationTree value in ptrees.Values) {
                    //    foreach (var node in value.Nodes) {
                    //        var x = value.Document.TaxonomyIdManager.TryToConvertToElementId(node.Element.Label);
                    //        var xy = value.Document.TaxonomyIdManager.GetElement(0);
                    //        value.Document.MainTaxonomy.Elements.ContainsKey()
                    //    }
                    //}
                    //}
                    var contained =
                        presentationTree.Document.MainTaxonomy.Elements.ContainsValue(keyValuePair.Key.Element);

                    if (!contained) {
                        keyValuePair.Value.IsVisible = false;
                    }
                }
            }
            */

            //ptrees.Values.First().Document.MainTaxonomy.Elements.ContainsValue(nodes.ToList()[20].Key.Element);

            return ptrees;
        }

        private static void CopyPresentationTree(
            PresentationTree newPtree, Taxonomy.Interfaces.PresentationTree.IPresentationTreeNode root,
            List<IPresentationTreeNode> ptreeNodes,
            Dictionary<IPresentationTreeEntry,
            IPresentationTreeNode> nodes) {

            // ignore selection list entries and root nodes
            if (root.Element.IsSelectionListEntry) return;
            if (nodes.ContainsKey(root)) return;

            // create new node
            var newNode = new PresentationTreeNode(newPtree, root);
            nodes.Add(root, newNode);
            ptreeNodes.Add(newNode);

            // do not process children of hyper cube containers
            if (root.IsHypercubeContainer) return;

            // process child nodes
            foreach (var child in root.Children.OfType<Taxonomy.Interfaces.PresentationTree.IPresentationTreeNode>()) {
                CopyPresentationTree(newPtree, child, ptreeNodes, nodes);
            }
        }

        #region ShowTransferValues
        private bool _showTransferValues;

        public bool ShowTransferValues {
            get { return _showTransferValues; }
            set {
                _showTransferValues = value;
                OnPropertyChanged("ShowTransferValues");
            }
        }
        #endregion

        #region Clear
        public void Clear() {
            //foreach (IPresentationTreeNode pnode in RootEntries) {
            //    pnode.Clear();
            //}
        }
        #endregion

        #region ClearAssignedItems
        public void ClearAssignedItems() {
            //foreach (IPresentationTreeNode pnode in RootEntries) {
            //    pnode.ClearAssignedAccounts();
            //}
        }
        #endregion

        #region Filter
        private PresentationTreeFilter _filter;

        public PresentationTreeFilter Filter { get {
            return _filter ?? (_filter = new PresentationTreeFilter(RootEntries.OfType<ITaxonomyTreeNode>()));
        } set { _filter = value; } }


        public PresentationTreeFilter DefaultFilter {
            get {
                return new PresentationTreeFilter(RootEntries.OfType<ITaxonomyTreeNode>()){Filter = string.Empty, HideEmptyPositions = false, HideNonManualFields = false, ShowOnlyMandatoryPostions = Manager.UserManager.Instance.CurrentUser.Options.ShowOnlyMandatoryPostions};
            }
        }

        #endregion // Filter

        #region ValidationError
        public bool ValidationError { get { return RootEntries.OfType<IPresentationTreeNode>().Any(root => root.ValidationError); } }
        #endregion

        #region ValidationWarning
        public bool ValidationWarning { get { return RootEntries.OfType<IPresentationTreeNode>().Any(root => root.ValidationWarning); } }
        #endregion

        #region Document
        public Document Document { get; set; }
        #endregion

        #region ExpandAllNodes
        public void ExpandAllNodes() {
            foreach (IPresentationTreeNode root in RootEntries.OfType<IPresentationTreeNode>()) {
                root.IsExpanded = true;
                foreach (IPresentationTreeNode child in root.Children.OfType<IPresentationTreeNode>()) {
                    child.ExpandAllChildren();
                }
            }
        }
        #endregion ExpandAllNodes

        #region CollapseAllNodes
        public void CollapseAllNodes() {
            foreach (IPresentationTreeNode root in RootEntries.OfType<IPresentationTreeNode>()) {
                root.IsExpanded = false;
                foreach (IPresentationTreeNode child in root.Children.OfType<IPresentationTreeNode>()) {
                    child.CollapseAllChildren();
                }
            }
        }
        #endregion CollapseAllNodes

        #region ResetFilter
        public void ResetFilter() {
            foreach (IPresentationTreeNode root in RootEntries.OfType<IPresentationTreeNode>()) {
                root.IsVisible = true;
                foreach (IPresentationTreeNode child in root.Children.OfType<IPresentationTreeNode>()) {
                    child.ResetFilter();
                }
            }
        }
        #endregion ResetFilter

        #region UnselectAll
        public void UnselectAll(object selectedItem = null) {
            foreach (IPresentationTreeNode root in RootEntries.OfType<IPresentationTreeNode>()) {
                root.IsSelected = false;
                foreach (IPresentationTreeNode child in root.Children.OfType<IPresentationTreeNode>()) {
                    child.UnselectAll(selectedItem);
                }
            }
        }
        #endregion UnselectAll

        #region ExpandSelected
        public void ExpandSelected(IPresentationTreeNode node) {
            CollapseAllNodes();
            IPresentationTreeNode root = node;
            while (root != null) {
                (root).IsExpanded = true;

                foreach (Taxonomy.Interfaces.PresentationTree.IPresentationTreeNode parent in root.Parents.Where(parent => parent.PresentationTree == this)) {
                    root = parent as IPresentationTreeNode;
                    break;
                }

                if (root.Parents.Count() != 0) continue;
                (root).IsExpanded = true;
                root = null;
            }
        }
        #endregion ExpandSelected

        #region GetNode
        /// <summary>
        /// Gets the presentation tree node with the specified internal id.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public IPresentationTreeNode GetNode(int id) {
            IPresentationTreeNode result;
            NodesByInternalId.TryGetValue(id, out result);
            return result;
        }
        #endregion // GetNode

        #region ContainsHiddenValue
        //private bool _containsHiddenValue;

        public bool ContainsHiddenValue {
            get {
                bool containsHiddenValue = false;
                foreach (PresentationTreeNode node in RootEntries) {
                    foreach (PresentationTreeNode treeNode in node.PresentationTree.Nodes) {
                        containsHiddenValue |= !treeNode.IsVisible && treeNode.Value != null &&
                                               treeNode.Value.HasValue;
                    }
                }
                return containsHiddenValue;
            }
        }
        #endregion ContainsHiddenValue


    }
}