// --------------------------------------------------------------------------------
// author: Mirko Dibbert
// since:  2010-10-06
// copyright 2010-2011 AvenDATA GmbH
// --------------------------------------------------------------------------------
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using Taxonomy.Enums;
using Taxonomy.Interfaces;
using Taxonomy.Interfaces.PresentationTree;


namespace Taxonomy.PresentationTree {
    /// <summary>
    /// Represents a presentation tree, which is defined by the xbrl presentation linkbase. All nodes 
    /// whithout a parent are defined as root nodes (usually exactly one node fullfills this statement).
    /// </summary>
    public class PresentationTree : IPresentationTree {

        #region ctor
        public PresentationTree() { }
        public PresentationTree(IPresentationTree ptree) { Role = ptree.Role; }
        #endregion ctor

        #region events
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string property) { if (PropertyChanged != null) PropertyChanged(this, new PropertyChangedEventArgs(property)); }
        #endregion events

        #region InitNodes
        /// <summary>
        /// Initializes the node dictionaries and the RootEntries enumeration.
        /// </summary>
        /// <param name="nodes">Enumeration of all nodes in the presentation tree.</param>
        /// <param name="root"> </param>
        public void InitNodes(IEnumerable<IPresentationTreeNode> nodes, IPresentationTreeNode expliciteRoot = null) {

            var nodesList = nodes.ToList();

            _nodesById.Clear();

            foreach (IPresentationTreeNode node in nodesList) {
                NodesById[node.Element.Id] = node;
            }

            // get root entries
            _rootEntries.Clear();
            if (expliciteRoot != null) _rootEntries.Add(expliciteRoot);
            else {
                foreach (IPresentationTreeNode node in nodesList.Where(node => node.IsRoot))
                    _rootEntries.Add(node);
            }
        }

        #endregion InitNodes

        #region SetIsHypercubeFlag
        public void SetIsHypercubeFlag(IEnumerable<IPresentationTreeNode> nodes) {
            _hypercubeContainerNodes.Clear();
            foreach (IPresentationTreeNode node in nodes) {
                ((PresentationTreeEntry)node).IsHypercubeContainer = node.Children.Any(child => child.Element.IsHypercubeItem);
                if (node.IsHypercubeContainer) {
                    _hypercubeContainerNodes.Add(node);
                    node.Element.ValueType = XbrlElementValueTypes.HyperCubeContainerItem;
                }
            }
        }
        #endregion SetIsHypercubeFlag

        #region properties

        #region RootEntries
        private readonly ObservableCollection<IPresentationTreeNode> _rootEntries =
            new ObservableCollection<IPresentationTreeNode>();

        public IEnumerable<IPresentationTreeNode> RootEntries { get { return _rootEntries; } }
        #endregion

        #region NodesById
        private readonly Dictionary<string, IPresentationTreeNode> _nodesById =
            new Dictionary<string, IPresentationTreeNode>();

        /// <summary>
        /// Dictionary of all contained nodes, accessed by taxonomy id.
        /// </summary>
        private Dictionary<string, IPresentationTreeNode> NodesById { get { return _nodesById; } }
        #endregion

        #region Nodes
        public IEnumerable<IPresentationTreeNode> Nodes { get { return _nodesById.Values; } }
        #endregion

        #region Nodes
        private readonly List<IPresentationTreeNode> _hypercubeContainerNodes = new List<IPresentationTreeNode>();
        public IEnumerable<IPresentationTreeNode> HypercubeContainerNodes { get { return _hypercubeContainerNodes; } }
        #endregion

        #region Role
        public IRoleType Role { get; internal set; }
        #endregion

        #endregion properties

        #region methods

        #region HasNode
        public bool HasNode(string key) { return NodesById.ContainsKey(key); }
        #endregion

        #region GetNode
        public IPresentationTreeNode GetNode(string key) {
            IPresentationTreeNode result;
            NodesById.TryGetValue(key, out result);
            return result;
        }
        #endregion

        public IEnumerator<IPresentationTreeEntry> GetEnumerator() { return RootEntries.GetEnumerator(); }
        IEnumerator IEnumerable.GetEnumerator() { return RootEntries.GetEnumerator(); }

        #endregion methods

        public int CompareTo(object obj) { return (obj is PresentationTree) ? Role.CompareTo((obj as PresentationTree).Role) : 0; }
    }
}