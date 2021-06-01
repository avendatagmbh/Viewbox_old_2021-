// --------------------------------------------------------------------------------
// author: Mirko Dibbert
// since:  2011-01-03
// copyright 2011 AvenDATA GmbH
// --------------------------------------------------------------------------------
using System;
using System.Collections;
using System.Collections.Generic;
using Taxonomy.Interfaces.PresentationTree;
using Utils;

namespace Taxonomy.PresentationTree {
    /// <summary>
    /// This class represents a node in the presentation tree.
    /// </summary>
    public class PresentationTreeNode : PresentationTreeEntry, IPresentationTreeNode {
        #region constructor
        internal PresentationTreeNode(IPresentationTree ptree, IElement element) {
            PresentationTree = ptree;
            Element = element;
        }

        public PresentationTreeNode(IPresentationTree ptree, IPresentationTreeEntry ptreeNode) {
            PresentationTree = ptree;
            Element = ((Element) ptreeNode.Element).PartialClone();
            IsHypercubeContainer = ptreeNode.IsHypercubeContainer;
            Order = ptreeNode.Order;
        }
        #endregion

        #region properties

        #region Children
        protected ObservableCollectionAsync<IPresentationTreeEntry> _children =
            new ObservableCollectionAsync<IPresentationTreeEntry>();

        public IEnumerable<IPresentationTreeEntry> Children { get { return _children; } }
        protected void ClearChildren() { _children.Clear(); }
        #endregion

        public bool IsPositiveComputationSource { get { return Element.HasPositiveComputationSources; } }
        public bool IsNegativeComputationSource { get { return Element.HasNegativeComputationSources; } }
        public IPresentationTree PresentationTree { get; set; }
        public bool IsComputed { get { return Element.HasComputationTargets; } }
        public bool IsComputationSource { get { return Element.HasComputationSources; } }

        #region IsVisible
        private bool _isVisible = true;

        public bool IsVisible {
            get { return _isVisible; }
            set {
                if (_isVisible == value) {
                    return;
                }
                _isVisible = value;
                OnPropertyChanged("IsVisible");
            }
        }
        #endregion



        #endregion properties

        #region methods

        #region ToString
        public override string ToString() {
            if (Element != null) return Element.ToString();
            else return base.ToString();
        }
        #endregion

        #region AddChildren
        public virtual void AddChildren(IPresentationTreeEntry child) {
            child.AddParent(this);
            int i = 0;
            foreach (IPresentationTreeEntry actChild in _children) {
                if (child.Order < actChild.Order) break;
                else i++;
            }

            _children.Insert(i, child);
        }
        #endregion

        #region RemoveChildren
        public virtual void RemoveChildren(IPresentationTreeEntry child) {
            child.RemoveParent(this);
            _children.Remove(child);
        }
        #endregion

        public IEnumerator<IPresentationTreeEntry> GetEnumerator() { return Children.GetEnumerator(); }
        IEnumerator IEnumerable.GetEnumerator() { return Children.GetEnumerator(); }
        #endregion methods
    }
}