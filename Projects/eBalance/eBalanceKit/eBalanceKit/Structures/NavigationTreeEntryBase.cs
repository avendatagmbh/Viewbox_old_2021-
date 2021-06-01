// --------------------------------------------------------------------------------
// author: Mirko Dibbert
// since:  2011-11-07
// copyright 2011 AvenDATA GmbH
// --------------------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using Taxonomy.Interfaces.PresentationTree;
using Utils;
using eBalanceKitBase.Interfaces;
using System.Linq;
using eBalanceKitBusiness.Manager;

namespace eBalanceKit.Structures {
    internal class NavigationTreeEntryBase : NotifyPropertyChangedBase, INavigationTreeEntryBase {
        public INavigationTreeEntryBase Parent { get; set; }

        public IEnumerable<IPresentationTreeEntry> PresentationTreeRoots {
            get { return _presentationTreeRoots; }
            set {
                if (value != null) {
                    // Add event handler
                    foreach (var presentationTreeNode in value.OfType<IPresentationTreeNode>()) {
                        eBalanceKitBusiness.IPresentationTree presentationTree;
                        if (DocumentManager.Instance.CurrentDocument.GaapPresentationTrees.TryGetValue(
                            presentationTreeNode.PresentationTree.Role.RoleUri, out presentationTree)) {
                            presentationTree.PropertyChanged += PresentationTreePropertyChanged;
                            var tree = presentationTree as eBalanceKitBusiness.Structures.Presentation.PresentationTree;
                            if (tree != null)
                                tree.PropertyChanged += PresentationTreePropertyChanged;
                        }
                    }
                } else if(_presentationTreeRoots != null) {
                    // remove event handler
                    foreach (var presentationTreeNode in _presentationTreeRoots.OfType<IPresentationTreeNode>()) {
                        eBalanceKitBusiness.IPresentationTree presentationTree;
                        if (DocumentManager.Instance.CurrentDocument.GaapPresentationTrees.TryGetValue(
                            presentationTreeNode.PresentationTree.Role.RoleUri, out presentationTree)) {
                            presentationTree.PropertyChanged -= PresentationTreePropertyChanged;
                        }
                    }
                }
                _presentationTreeRoots = value;
            }
        }

        /// <summary>
        /// Fire an OnPropertyChanged for ContainsHiddenValue if the ContainsHiddenValue property of one of the PresentationTreeRoots was changed
        /// </summary>
        private void PresentationTreePropertyChanged(object sender, PropertyChangedEventArgs propertyChangedEventArgs) {
            if (propertyChangedEventArgs.PropertyName.Equals("ContainsHiddenValue")) {
                OnPropertyChanged("ContainsHiddenValue");
            }
        }
        private IEnumerable<IPresentationTreeEntry> _presentationTreeRoots;

        #region Children
        private readonly ObservableCollectionAsync<INavigationTreeEntryBase> _children =
            new ObservableCollectionAsync<INavigationTreeEntryBase>();

        public IList<INavigationTreeEntryBase> Children { get { return _children; } }
        #endregion
        
        public string Header { get; set; }
        public Taxonomy.IElement XbrlElem { get; set; }
        public object Model { get; set; }
        public object Content { get; set; }
        public string RoleId { get; set; }

        private string _nodeId = null;
        /// <inheritdoc />
        public string NodeId {
            get {
                if (this._nodeId == null) {
                    if (XbrlElem != null) this._nodeId = XbrlElem.Id;
                    else this._nodeId = RoleId;
                }
                return this._nodeId;
            }
            set { this._nodeId = value; }
        }

        #region IsVisible
        private bool _isVisible;

        public bool IsVisible {
            get {
                return _isVisible;
            }
            set {
                if (_isVisible != value) {
                    _isVisible = value;
                    OnPropertyChanged("IsVisible");
                }
            }
        }
        #endregion // IsVisible


        #region ContainsHiddenValue
        /// <summary>
        /// Property that shows if a presentation tree contains an invisible node with value.
        /// </summary>
        public bool ContainsHiddenValue {
            get {
                return PresentationTreeRoots != null &&
                       PresentationTreeRoots.OfType<IPresentationTreeNode>().Any(
                           root =>
                               DocumentManager.Instance.CurrentDocument.GaapPresentationTrees.ContainsKey(root.PresentationTree.Role.RoleUri) &&
                DocumentManager.Instance.CurrentDocument.GaapPresentationTrees[root.PresentationTree.Role.RoleUri].ContainsHiddenValue);
            }
        }
        #endregion // ContainsHiddenValue

        #region IsExpanded
        public bool IsExpanded {
            get { return _isExpanded; }
            set {
                if (_isExpanded != value) {
                    _isExpanded = value;
                    OnPropertyChanged("IsExpanded");
                }
            }
        }
        private bool _isExpanded;
        #endregion 

        #region IsSelected
        public bool IsSelected {
            get { return _isSelected; }
            set {
                _isSelected = value;

                // auto expand subtree if element is selected
                //if (_isSelected && !IsExpanded) IsExpanded = true;

                OnPropertyChanged("IsSelected");
            }
        }
        private bool _isSelected;
        #endregion
        
        public void Addchildren(NavigationTreeEntryBase entry) {
            entry.Parent = this;
            _children.Add(entry);
        }

    }
}