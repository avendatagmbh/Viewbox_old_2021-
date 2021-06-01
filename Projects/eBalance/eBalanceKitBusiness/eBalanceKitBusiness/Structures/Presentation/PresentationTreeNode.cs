// --------------------------------------------------------------------------------
// author: Mirko Dibbert
// since:  2011-06-25
// copyright 2011 AvenDATA GmbH
// --------------------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Taxonomy;
using Taxonomy.Enums;
using Taxonomy.Interfaces.PresentationTree;
using eBalanceKitBusiness.AuditCorrections;
using eBalanceKitBusiness.Interfaces;
using eBalanceKitBusiness.Structures.DbMapping;
using eBalanceKitBusiness.Structures.XbrlElementValue;
using IPresentationTreeNode = eBalanceKitBusiness.Interfaces.PresentationTree.IPresentationTreeNode;

namespace eBalanceKitBusiness.Structures.Presentation {
    /// <summary>
    /// Extension of presentation tree for gui presentation purposes.
    /// </summary>
    public class PresentationTreeNode : Taxonomy.PresentationTree.PresentationTreeNode, IPresentationTreeNode {

        private bool _isExpandedFirstTime = true;
        
        public PresentationTreeNode(PresentationTree ptree,
                                    Taxonomy.Interfaces.PresentationTree.IPresentationTreeNode ptreeNode)
            : base(ptree, ptreeNode) {

            PresentationTree.PropertyChanged += PresentationTree_PropertyChanged;
            this.PropertyChanged += (PresentationTree as PresentationTree).VisibleValuePropertyChanged;
        }

        public bool ShowTransferValues { get { return (PresentationTree as IPresentationTree).ShowTransferValues; } }

        #region Validation specific properties

        #region ValidationWarning
        /// <summary>
        /// Returns true, iif. ValidationWarning is true for the assigned value of this node or for the assigned value of at least one child node.
        /// </summary>
        public bool ValidationWarning {
            get {
                if (Value != null && Value.ValidationWarning) return true;
                return Children.OfType<IPresentationTreeNode>().Any(child => child.ValidationWarning);
            }
        }
        #endregion // ValidationWarning

        #region ValidationWarningMessage
        public string ValidationWarningMessage {
            get {
                if (Value != null && Value.ValidationWarning) return Value.ValidationWarningMessage;
                return Children.OfType<IPresentationTreeNode>().Any(child => child.ValidationWarning)
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
                return Children.OfType<IPresentationTreeNode>().Any(child => child.ValidationError);
            }
        }
        #endregion // ValidationError

        #region ValidationErrorMessage
        public string ValidationErrorMessage {
            get {
                if (Value != null && Value.ValidationError) return Value.ValidationErrorMessage;
                return Children.OfType<IPresentationTreeNode>().Any(child => child.ValidationError)
                           ? "Fehler in einem untergeordneten Knoten"
                           : string.Empty;
            }
        }
        #endregion // ValidationErrorMessage

        #endregion Validation specific properties

        #region IsExpanded
        private bool _isExpanded;

        public bool IsExpanded {
            get { return _isExpanded; }
            set {
                if (_isExpanded != value) {
                    _isExpanded = value;
                    OnPropertyChanged("IsExpanded");
                }
            }
        }
        #endregion

        #region IsSelected
        private bool _isSelected;

        public bool IsSelected {
            get { return _isSelected; }
            set {
                if (_isSelected != value) {
                    _isSelected = value;
                    OnPropertyChanged("IsSelected");
                }
            }
        }
        #endregion
        
        #region DecimalValue
        public decimal DecimalValue {
            get {
                if (Value != null) return Value.DecimalValue;
                else return 0;
            }
        }
        #endregion

        #region UpdateVisibility
        public void UpdateVisibility(PresentationTreeFilter filter, bool updateIsExpanded = false) {
            
            string f = (filter.Filter == null ? null : filter.Filter.Trim().ToLower());
            foreach (IPresentationTreeEntry child in Children) {
                if (!(child is IPresentationTreeNode)) continue;
                (child as IPresentationTreeNode).UpdateVisibility(filter, updateIsExpanded);
            }

            if (!IsRoot) IsVisible = filter.GetVisiblility(Document, Element, Value);

            if (Children.Where(child => (child is IPresentationTreeNode)).Any(child => (child as IPresentationTreeNode).IsVisible)) {
                IsVisible = true;
                if (updateIsExpanded) IsExpanded = IsVisible; // set to isVisible before return
                
                return;
            }

            // If there are childs that are IPresentationTreeNodes but non of them IsVisible
            if (IsRoot && !Children.OfType<IPresentationTreeNode>().Any(child => child.IsVisible)) {
                // this node itself will be visible depending on the filter configuration
                //IsVisible = false;
                IsVisible = filter.GetVisiblility(Document, Element, Value);
                if (updateIsExpanded) IsExpanded = IsVisible;
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
        #endregion UpdateVisibility

        #region IPresentationTreeNode Members
        public override void AddChildren(IPresentationTreeEntry child) {
            if (child is IBalanceListEntry) {
                var ble = child as IBalanceListEntry;
                child.AddParent(this);
                int i = 0;
                foreach (IPresentationTreeEntry actChild in _children) {
                    if (!(actChild is IBalanceListEntry)) break;
                    var actBle = actChild as IBalanceListEntry;
                    if (ble.Number.CompareTo(actBle.Number) < 0) break;
                    else i++;
                }

                _children.Insert(i, child);

                ble.PropertyChanged += BalanceListEntryPropertyChanged;           
            
            } else {
                base.AddChildren(child);
            }

            if (Value != null) Value.UpdateComputedValue(this);
        }

        public override void RemoveChildren(IPresentationTreeEntry child) {
            base.RemoveChildren(child);
            if (Value != null) Value.UpdateComputedValue(this);
            if (child is IBalanceListEntry) (child as IBalanceListEntry).PropertyChanged -= BalanceListEntryPropertyChanged;
        }

        public bool IsValueEditingAllowed {
            get {
                return Value != null && Value.IsEnabled && Element.ValueType == XbrlElementValueTypes.Monetary &&
                       !Value.HasComputedValue;
            }
        }

        public void ExpandAllChildren() {
            IsExpanded = true;

            // allow GUI to render visual
            System.Threading.Thread.Sleep(_isExpandedFirstTime ? 10 : 2);
            _isExpandedFirstTime = false;

            foreach (IPresentationTreeEntry child in Children) {
                if (!(child is IPresentationTreeNode)) continue;
                (child as IPresentationTreeNode).ExpandAllChildren();
            }
        }

        public void CollapseAllChildren() {
            IsExpanded = false;
            foreach (IPresentationTreeEntry child in Children) {
                if (!(child is IPresentationTreeNode)) continue;
                (child as IPresentationTreeNode).CollapseAllChildren();
            }
        }

        public void ResetFilter() {
            IsVisible = true;
            foreach (IPresentationTreeEntry child in Children) {
                if (!(child is IPresentationTreeNode)) continue;
                (child as IPresentationTreeNode).ResetFilter();
            }
        }

        public void UnselectAll(object selectedItem = null) {
            IsSelected = false;
            foreach (IPresentationTreeEntry child in Children) {
                if (child is IPresentationTreeNode) (child as IPresentationTreeNode).UnselectAll(selectedItem);
                else if (child != selectedItem && child is IsSelectable) (child as IsSelectable).IsSelected = false;
            }
        }

        
        public Document Document { get { return (PresentationTree as IPresentationTree).Document; } }
        #endregion

        #region ClearAssignedAccounts
        public void ClearAssignedAccounts() {
            IsExpanded = false;

            var accounts = new List<IPresentationTreeNode>();

            //foreach (IPresentationTreeNode child in this.UnfilteredChildren) {
            //if (child is IBalanceListEntry) {
            //    accounts.Add(child);
            //} else {
            //    child.ClearAssignedAccounts();
            //}
            //}

            foreach (IPresentationTreeNode account in accounts) {
                //account.Parents.Clear();
                //_children.Remove(account);
            }
        }
        #endregion

        #region Value
        private IValueTreeEntry _value;

        public IValueTreeEntry Value {
            get { return _value; }
            set {
                if (_value != value) {
                    // unregister event handler
                    if (_value != null) _value.PropertyChanged -= value_PropertyChanged;

                    _value = value;

                    // register nw event handler
                    if (_value != null) value.PropertyChanged += value_PropertyChanged;

                    // add manual (optional) computation rule, if no computation rule is available in taxonomy (sums up value to parent)
                    if (value != null && _value.IsComputationOrphanedNode) {
                        foreach (IPresentationTreeNode parent in Parents) {
                            if (parent.Value != null) {
                                IValueTreeEntry val = parent.Value;
                                if (!_value.HasSummationSource(val.Element.Id)) {
                                    _value.AddUserSummationSource(val);
                                    val.AddUserSummationTarget(_value, 1);
                                }
                            }
                        }
                    }

                    OnPropertyChanged("Value");
                    OnPropertyChanged("IsValueEditingAllowed");
                }
            }
        }

        private void value_PropertyChanged(object sender, PropertyChangedEventArgs e) {
            switch (e.PropertyName) {
                case "ValidationError":
                case "ValidationErrorMessage":
                case "ValidationWarning":
                case "ValidationWarningMessage":
                case "HideAllWarnings":
                case "HideWarning":
                    OnPropertyChanged(e.PropertyName);
                    break;

                case "IsEnabled":
                case "HasComputedValue":
                    OnPropertyChanged("IsValueEditingAllowed");
                    break;
            }
        }
        #endregion

        private void BalanceListEntryPropertyChanged(object sender, PropertyChangedEventArgs e) { if (Value != null) Value.UpdateComputedValue(this); }

        private void PresentationTree_PropertyChanged(object sender, PropertyChangedEventArgs e) {
            switch (e.PropertyName) {
                case "ShowTransferValues":
                    OnPropertyChanged("ShowTransferValues");
                    break;
            }
        }
    }
}