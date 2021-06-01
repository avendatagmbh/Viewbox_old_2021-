// --------------------------------------------------------------------------------
// author: Mirko Dibbert
// since: 2012-03-13
// copyright 2012 AvenDATA GmbH
// --------------------------------------------------------------------------------
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Taxonomy;
using Taxonomy.Interfaces;
using Utils;
using eBalanceKitBusiness.Reconciliation.Interfaces;
using eBalanceKitBusiness.Structures;
using eBalanceKitBusiness.Structures.DbMapping;
using IPresentationTreeNode = eBalanceKitBusiness.Interfaces.PresentationTree.IPresentationTreeNode;

namespace eBalanceKitBusiness.Reconciliation {
    internal class ReconciliationTreeNode : NotifyPropertyChangedBase, IReconciliationTreeNode {

        private bool _isExpandedFirstTime = true;

        internal ReconciliationTreeNode(IReconciliationTree reconciliationTree, IPresentationTreeNode node) {
            Document = node.Document;
            ReconciliationTree = reconciliationTree;
            Element = node.Element;
            Value = node.Value;
            Order = node.Order;
            
            if (Value != null) {
                Value.PropertyChanged += ValuePropertyChanged;
                if (Value.ReconciliationInfo != null) {
                    foreach (var transaction in Value.ReconciliationInfo.Transactions) {
                        _children.Add(transaction);
                        transaction.SetParent(this);

                        //transaction.PropertyChanged += Transaction_PropertyChanged;
                    }
                    Value.ReconciliationInfo.TransactionAdded += ReconciliationInfoOnTransactionAdded;
                    Value.ReconciliationInfo.TransactionRemoved += ReconciliationInfoOnTransactionRemoved;
                    Value.ReconciliationInfo.PropertyChanged +=
                        (sender, args) => {
                            if (args.PropertyName == "IsAssignedToSelectedReconciliation")
                                OnPropertyChanged("IsAssignedToSelectedReconciliation");
                        };

                    Value.ReconciliationInfo.TransactionSelected +=
                        (o, eventArgs) => ((ReconciliationTree) ReconciliationTree).ExpandSelected(
                            !((ReconciliationTypes.Reconciliation)eventArgs.Transaction.Reconciliation).IsAssignable
                                ? (IReconciliationTreeEntry)ReconciliationTree.GetNode(eventArgs.Transaction.Position.Id)
                                : eventArgs.Transaction);

                } else {
                    Value.PropertyChanged += (sender, args) => {
                        if (args.PropertyName != "ReconciliationInfo") return;
                        foreach (var transaction in Value.ReconciliationInfo.Transactions) {
                            _children.Add(transaction);
                            transaction.SetParent(this);
                            //transaction.PropertyChanged += Transaction_PropertyChanged;
                        }
                        Value.ReconciliationInfo.TransactionAdded += ReconciliationInfoOnTransactionAdded;
                        Value.ReconciliationInfo.TransactionRemoved += ReconciliationInfoOnTransactionRemoved;
                        Value.ReconciliationInfo.PropertyChanged +=
                            (sender1, args1) => {
                                if (args1.PropertyName == "IsAssignedToSelectedReconciliation")
                                    OnPropertyChanged("IsAssignedToSelectedReconciliation");
                            };

                        Value.ReconciliationInfo.TransactionSelected +=
                            (o, eventArgs) => ((ReconciliationTree) ReconciliationTree).ExpandSelected(this);
                    };
                }
            }

            foreach (IPresentationTreeNode child in node.Children.OfType<IPresentationTreeNode>()) {
                var newNode = new ReconciliationTreeNode(reconciliationTree, child);
                newNode._parents.Add(this);
                _children.Add(newNode);
            }

            foreach (var child in Children.OfType<ReconciliationTreeNode>())
                child._parents.Add(this);

            SortChildren();
        }

        private void ValuePropertyChanged(object sender, PropertyChangedEventArgs e) {
            switch (e.PropertyName) {
                case "ValidationError":
                case "ValidationErrorMessage":
                case "ValidationWarning":
                case "ValidationWarningMessage":
                    OnPropertyChanged(e.PropertyName);
                    break;
            }
        }

        private void ReconciliationInfoOnTransactionRemoved(object sender, TransactionChangedEventArgs e) {
            //e.Transaction.PropertyChanged -= Transaction_PropertyChanged;
            _children.Remove(e.Transaction);
            e.Transaction.ResetParent();
        }

        void ReconciliationInfoOnTransactionAdded(object sender, TransactionChangedEventArgs e) {
            //e.Transaction.PropertyChanged += Transaction_PropertyChanged;
            e.Transaction.SetParent(this);
            _children.Add(e.Transaction);
            SortChildren();
        }

        #region properties

        #region Document
        public Document Document { get; private set; }
        #endregion // Document

        #region ReconciliationTree
        public IReconciliationTree ReconciliationTree { get; private set; }
        #endregion // ReconciliationTree

        #region Element
        public IElement Element { get; private set; }
        #endregion // Element

        #region Value
        public IValueTreeEntry Value { get; private set; }
        #endregion // Value

        public bool IsPositiveComputationSource { get { return Element.HasPositiveComputationSources; } }
        public bool IsNegativeComputationSource { get { return Element.HasNegativeComputationSources; } }
        public bool IsComputed { get { return Element.HasComputationTargets; } }
        public bool IsComputationSource { get { return Element.HasComputationSources; } }

        #region IsSelected
        private bool _isSelected;

        public bool IsSelected {
            get { return _isSelected; }
            set {
                if (_isSelected == value) return;
                _isSelected = value;
                OnPropertyChanged("IsSelected");
            }
        }
        #endregion // IsSelected

        #region IsAssignedToSelectedReconciliation
        public bool IsAssignedToSelectedReconciliation {
            get {
                return Value != null &&
                       (Value.ReconciliationInfo != null && Value.ReconciliationInfo.IsAssignedToSelectedReconciliation);
            }
        }
        #endregion // IsAssignedToSelectedReconciliation

        #region IsAssignedToReferenceList
        private bool _isAssignedToReferenceList = false;
        //public bool IsAssignedToReferenceList { get; set; }
        public bool IsAssignedToReferenceList {
            get { return _isAssignedToReferenceList; }
            set {
                _isAssignedToReferenceList = value;
                OnPropertyChanged("IsAssignedToReferenceList");
            }
        }
        #endregion IsAssignedToReferenceList

        #region IsExpanded
        private bool _isExpanded;

        public bool IsExpanded {
            get { return _isExpanded; }
            set {
                _isExpanded = value;
                OnPropertyChanged("IsExpanded");
            }
        }
        #endregion // IsExpanded

        #region IsVisible
        private bool _isVisible = true;

        public bool IsVisible {
            get { return _isVisible; }
            set {
                _isVisible = value;
                OnPropertyChanged("IsVisible");
            }
        }
        #endregion // IsVisible

        #region Children
        private readonly ObservableCollectionAsync<IReconciliationTreeEntry> _children = new ObservableCollectionAsync<IReconciliationTreeEntry>();

        public IEnumerable<IReconciliationTreeEntry> Children { get { return _children; } }
        #endregion // Children

        #region Parents
        private readonly List<IReconciliationTreeNode> _parents = new List<IReconciliationTreeNode>();

        public virtual IEnumerable<IReconciliationTreeNode> Parents { get { return _parents; } }
        #endregion // Parents

        #region IsRoot
        public virtual bool IsRoot { get { return _parents.Count == 0; } }
        #endregion // IsRoot

        #region Order
        public double Order { get; set; }
        #endregion

        #region Validation specific properties

        #region ValidationWarning
        /// <summary>
        /// Returns true, iif. ValidationWarning is true for the assigned value of this node or for the assigned value of at least one child node.
        /// </summary>
        public bool ValidationWarning {
            get {
                if (Value != null && Value.ValidationWarning) return true;
                return Children.OfType<IReconciliationTreeNode>().Any(child => child.ValidationWarning);
            }
        }
        #endregion // ValidationWarning

        #region ValidationWarningMessage
        public string ValidationWarningMessage {
            get {
                if (Value != null && Value.ValidationWarning) return Value.ValidationWarningMessage;
                return Children.OfType<IReconciliationTreeNode>().Any(child => child.ValidationWarning)
                           ? "Warnung in einem untergeordneten Knoten"
                           : string.Empty;
            }
        }
        #endregion // ValidationWarningMessage

        #region ValidationError
        /// <summary>
        /// Returns true, iif. ValidationError is true for the assigned value of this node or for the assigned value of at least one child node.
        /// </summary>
        public bool ValidationError {
            get {
                if (Value != null && Value.ValidationError) return true;
                return Children.OfType<IReconciliationTreeNode>().Any(child => child.ValidationError);
            }
        }
        #endregion // ValidationError

        #region ValidationErrorMessage
        public string ValidationErrorMessage {
            get {
                if (Value != null && Value.ValidationError) return Value.ValidationErrorMessage;
                return Children.OfType<IReconciliationTreeNode>().Any(child => child.ValidationError)
                           ? "Fehler in einem untergeordneten Knoten"
                           : string.Empty;
            }
        }
        #endregion // ValidationErrorMessage

        #endregion Validation specific properties

        #region TreeViewVisualOptions
        public TreeViewVisualOptions VisualOptions { get { return ReconciliationTree.VisualOptions; } }
        #endregion // TreeViewVisualOptions

        #endregion // properties

        #region methods

        #region ExpandAllChildren
        public void ExpandAllChildren() {
            IsExpanded = true;

            // allow GUI to render visual
            System.Threading.Thread.Sleep(_isExpandedFirstTime ? 10 : 2);
            _isExpandedFirstTime = false;

            foreach (var child in Children.OfType<IReconciliationTreeNode>()) child.ExpandAllChildren();
        }
        #endregion // ExpandAllChildren

        #region [ ExpandAllChildrenForReferenceList ]

        public bool ExpandAllChildrenForReferenceList() {
            // allow GUI to render visual
            System.Threading.Thread.Sleep(_isExpandedFirstTime ? 10 : 2);
            _isExpandedFirstTime = false;
            
            bool hasSelectedChild = false;
            IEnumerable<IReconciliationTreeNode> children = Children.OfType<IReconciliationTreeNode>();
            foreach (var child in children) {
                if (child.ExpandAllChildrenForReferenceList()) {
                    hasSelectedChild = true;
                    child.IsExpanded = true;
                }
            }
            if (hasSelectedChild || IsAssignedToReferenceList) {
                IsExpanded = true;
                return true;
            }

            return false;
        }

        #endregion [ ExpandAllChildrenForReferenceList ]

        #region CollapseAllChildren
        public void CollapseAllChildren() {
            IsExpanded = false;
            foreach (var child in Children.OfType<IReconciliationTreeNode>()) child.CollapseAllChildren();
        }
        #endregion // CollapseAllChildren

        #region UnselectAllNodes
        public void UnselectAllNodes() {
            IsSelected = false;
            foreach (var child in Children.OfType<IReconciliationTreeNode>()) child.UnselectAllNodes();
        }
        #endregion // UnselectAllNodes

        #region UpdateVisibility
        public void UpdateVisibility(PresentationTreeFilter filter, bool updateIsExpanded = false) {

            string f = (filter.Filter == null ? null : filter.Filter.Trim().ToLower());

            foreach (var child in Children.OfType<IReconciliationTreeNode>()) child.UpdateVisibility(filter, updateIsExpanded);

            if (!IsRoot) IsVisible = filter.GetVisiblility(Document, Element, Value);

            if (Children.OfType<IReconciliationTreeNode>().Any(child => child.IsVisible)) {
                IsVisible = true;
                if (updateIsExpanded) IsExpanded = IsVisible; // set to isVisible before return
                return;
            }

            // if HideNonManualFields is true then set IsVisible value. otherwise compelete filter incl. the search string
            if (IsVisible) {
                IsVisible =
                    string.IsNullOrEmpty(f) ||
                    Element.Label.ToLower().Contains(f) ||
                    Element.Id.ToLower().Contains(f) ||
                    Element.Id.ToLower().Contains(f);
            }

            if (updateIsExpanded) IsExpanded = IsVisible;
        }
        #endregion // UpdateVisibility

        #region ResetFilter

        public void ResetFilter() {
            IsVisible = true;
            foreach (var child in Children.OfType<IReconciliationTreeNode>()) child.ResetFilter();
        }
        #endregion // ResetFilter

        private void SortChildren() {
            var tmp = (
                          from child in Children
                          orderby
                              child.Order ,
                              (child is IReconciliationTransaction
                                   ? ((IReconciliationTransaction) child).Reconciliation.Name ?? string.Empty
                                   : string.Empty)
                          select child).ToList();
            
            _children.Clear();
            foreach (var entry in tmp) _children.Add(entry);
        }

        public override string ToString() { return Element.ToString(); }

        public IEnumerator<IReconciliationTreeEntry> GetEnumerator() { return Children.GetEnumerator(); }
        IEnumerator IEnumerable.GetEnumerator() { return GetEnumerator(); }
        
        #endregion // methods

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
    }
}