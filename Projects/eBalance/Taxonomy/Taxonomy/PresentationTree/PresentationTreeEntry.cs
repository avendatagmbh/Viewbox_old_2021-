// --------------------------------------------------------------------------------
// author: Mirko Dibbert
// since:  2011-06-26
// copyright 2011 AvenDATA GmbH
// --------------------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.ComponentModel;
using Taxonomy.Interfaces;
using Taxonomy.Interfaces.PresentationTree;
using Utils;

namespace Taxonomy.PresentationTree {
    /// <summary>
    /// Basic IPresentationTreeEntry implementation. All properties and methods are declared 
    /// as virtual and could therefore be overwritten in derived classes, if neccesary.
    /// 
    /// The default value of the Order proerty is -1, thus usually all new items with no
    /// explicite order value will be inserted before all other values, which usually have
    /// an order value greater or equal to zero.
    /// </summary>
    public class PresentationTreeEntry : NotifyPropertyChangedBase, IPresentationTreeEntry {

        #region Parents
        private readonly ObservableCollectionAsync<IPresentationTreeNode> _parents =
            new ObservableCollectionAsync<IPresentationTreeNode>();

        public virtual IEnumerable<IPresentationTreeNode> Parents { get { return _parents; } }
        #endregion

        #region IsRoot
        public virtual bool IsRoot { get { return _parents.Count == 0; } }
        #endregion

        #region Order
        private double _order = -1;

        public double Order { get { return _order; } set { _order = value; } }
        #endregion

        #region IsHypercubeContainer
        public bool IsHypercubeContainer { get; internal set; }
        #endregion

        #region AddParent
        public virtual void AddParent(IPresentationTreeNode parent) { _parents.Add(parent); }
        #endregion

        #region RemoveParent
        public virtual void RemoveParent(IPresentationTreeNode parent) { _parents.Remove(parent); }
        #endregion

        #region RemoveFromParent
        public virtual void RemoveFromParents() {
            var tmpParents = new List<IPresentationTreeNode>(Parents);
            foreach (IPresentationTreeNode node in tmpParents) node.RemoveChildren(this);
        }
        #endregion

        public event ScrollIntoViewRequestedEventHandler ScrollIntoViewRequested;
        public event ScrollIntoViewRequestedEventHandler SearchLeaveFocusRequested;

        public void ScrollIntoView(IList<ISearchableNode> path) {
            if (ScrollIntoViewRequested != null)
                ScrollIntoViewRequested(path);
        }

        public void SearchLeaveFocus(IList<ISearchableNode> path) {
            if (SearchLeaveFocusRequested != null)
                SearchLeaveFocusRequested(path);
        }

        #region IPresentationTreeEntry Members
        public IElement Element { get; protected set; }
        #endregion
    }
}