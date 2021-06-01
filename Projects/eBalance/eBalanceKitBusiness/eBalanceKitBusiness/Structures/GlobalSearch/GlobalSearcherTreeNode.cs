// --------------------------------------------------------------------------------
// author: Mirko Dibbert
// since: 2012-05-11
// copyright 2012 AvenDATA GmbH
// --------------------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Windows.Threading;
using Taxonomy;
using Utils;

namespace eBalanceKitBusiness.Structures.GlobalSearch {
    public class GlobalSearcherTreeNode : NotifyPropertyChangedBase, IGlobalSearcherTreeNode {

        internal GlobalSearcherTreeNode() { }
        internal GlobalSearcherTreeNode(IElement element) { 
            Element = element;
        }

        private IElement Element { get; set; }
        
        public virtual string Label { get { return Element.Label; } }

        #region Children
        private readonly Dictionary<IElement, IGlobalSearcherTreeNode> _childrenByElement =
            new Dictionary<IElement, IGlobalSearcherTreeNode>();

        private readonly ObservableCollectionAsync<IGlobalSearcherTreeNode> _children =
            new ObservableCollectionAsync<IGlobalSearcherTreeNode>();

        public IEnumerable<IGlobalSearcherTreeNode> Children { get { return _children; } }
        #endregion // Children

        private Action<string, CancellationToken> _saveHistory;
        private string _searchedString;
        private CancellationToken _token;

        public void BindSaveHistory(Action<string, CancellationToken> saveHistory, string searchedString, CancellationToken token) { 
            _saveHistory = saveHistory;
            _searchedString = searchedString;
            _token = token;
        }

        #region IsSelected
        private bool _isSelected;

        public bool IsSelected {
            get { return _isSelected; }
            set {
                if (_isSelected == value) return;
                _isSelected = value;
                // when building the tree in RecursiveSearchWithValueTree.cs there will be a BindSaveHistory(...) call to ensure that this code run:
                // if an element is selected we save the search (if the save's task is not cancelled).
                _saveHistory(_searchedString, _token);
                OnPropertyChanged("IsSelected");
            }
        }
        #endregion // IsSelected

        #region IsExpanded
        private bool _isExpanded;

        public bool IsExpanded {
            get { return _isExpanded; }
            set {
                if (_isExpanded == value) return;
                _isExpanded = value;
                OnPropertyChanged("IsExpanded");
            }
        }
        #endregion // IsExpanded

        internal void AddChildren(IGlobalSearcherTreeNode child) {
            _children.Add(child);

            if (child is SearchResultItem) {
                _childrenByElement[((SearchResultItem) child).PresentationTreeEntryPath.Last().Element] =
                    child;
            } else {
                _childrenByElement[((GlobalSearcherTreeNode) child).Element] = child;
            }
        }

        internal void RemoveChildren(IGlobalSearcherTreeNode child) {
            _children.Remove(child);

            if (child is SearchResultItem) {
                _childrenByElement.Remove(
                    ((SearchResultItem) child).PresentationTreeEntryPath.Last().Element);
            } else {
                _childrenByElement.Remove(((GlobalSearcherTreeNode) child).Element);
            }
        }

        internal IGlobalSearcherTreeNode GetChildren(IElement element) {
            IGlobalSearcherTreeNode result;
            _childrenByElement.TryGetValue(element, out result);
            return result;
        }

        public override string ToString() { return Label; }
        
        public int ExpandAllSubnodes(int actual, int limit, CancellationToken token) {
            if (token.IsCancellationRequested) {
                //token.ThrowIfCancellationRequested();
                return actual;
            }
            foreach (var item in Children) {
                if (((ObservableCollectionAsync<IGlobalSearcherTreeNode>) item.Children).Count <= 0) continue;
                if (token.IsCancellationRequested) {
                    //token.ThrowIfCancellationRequested();
                    break;
                }
                Dispatcher.CurrentDispatcher.Invoke((Action<object>) (state => {
                    var passedItem = (IGlobalSearcherTreeNode)state;
                    if (token.IsCancellationRequested) {
                        //token.ThrowIfCancellationRequested();
                        return;
                    }
                    passedItem.IsExpanded = true;
                }), DispatcherPriority.SystemIdle, item);
                actual++;
                actual = ((GlobalSearcherTreeNode) item).ExpandAllSubnodes(actual, limit, token);
                if (limit > -1 && actual > limit) {
                    break;
                }
            }
            return actual;
        }

        public void CollapseAllSubnodes() {
            foreach (var item in Children) {
                if (((ObservableCollectionAsync<IGlobalSearcherTreeNode>) item.Children).Count <= 0) continue;
                ((GlobalSearcherTreeNode)item).CollapseAllSubnodes();
                item.IsExpanded = false;
            }
        }

    }
}