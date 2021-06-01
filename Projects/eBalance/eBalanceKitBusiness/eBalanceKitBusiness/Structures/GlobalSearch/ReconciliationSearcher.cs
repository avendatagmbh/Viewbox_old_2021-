using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Windows.Threading;
using Taxonomy.Interfaces;
using eBalanceKitBusiness.Reconciliation;
using eBalanceKitBusiness.Reconciliation.Interfaces;
using eBalanceKitBusiness.Reconciliation.ViewModels;
using eBalanceKitBusiness.Structures.DbMapping;
using eBalanceKitBusiness.Structures.GlobalSearch.Enums;

namespace eBalanceKitBusiness.Structures.GlobalSearch {
    /// <summary>
    /// class for search inside reconciliations.
    /// </summary>
    public class ReconciliationSearcher : SearcherBase {
        public ReconciliationSearcher(Document document, ReconciliationsModel model) : base(document) { _reconciliationsModel = model; }

        private readonly ReconciliationsModel _reconciliationsModel;

        private string _searchString;
        private bool _searchInId;
        private bool _searchInLabel;
        private CancellationToken _token;

        /// <summary>
        /// Start the search. Build the ResultTreeRoots. 
        /// </summary>
        /// <param name="searchString">the searched string</param>
        /// <param name="searchInId">search in element.id</param>
        /// <param name="searchInLabel">search in element.label</param>
        /// <param name="token">cancellation token for the search task</param>
        public override void Search(string searchString, bool searchInId, bool searchInLabel, CancellationToken token) {

            if (string.IsNullOrEmpty(searchString)) {
                return;
            }
            if (!(searchInId || searchInLabel)) {
                return;
            }

            _searchString = searchString;
            _searchInId = searchInId;
            _searchInLabel = searchInLabel;
            _token = token;

            ResultTreeRoots.Clear();

            // search for each tab item in CtlReconciliations
            foreach (
                IReconciliationTreeNode reconciliationTreeNode in
                    _reconciliationsModel.PresentationTreeBalanceSheetLiabilities.RootEntries) {
                GlobalSearcherTreeNode node =
                    CheckReconciliationNodeRecursively(new List<ISearchableNode> {reconciliationTreeNode});
                if (node != null)
                    ResultTreeRoots.Add(node);
            }
            foreach (
                IReconciliationTreeNode reconciliationTreeNode in
                    _reconciliationsModel.PresentationTreeBalanceSheetTotalAssets.RootEntries) {
                GlobalSearcherTreeNode node =
                    CheckReconciliationNodeRecursively(new List<ISearchableNode> {reconciliationTreeNode});
                if (node != null)
                    ResultTreeRoots.Add(node);
            }
            foreach (
                IReconciliationTreeNode reconciliationTreeNode in
                    _reconciliationsModel.PresentationTreeIncomeStatement.RootEntries) {
                GlobalSearcherTreeNode node =
                    CheckReconciliationNodeRecursively(new List<ISearchableNode> {reconciliationTreeNode});
                if (node != null)
                    ResultTreeRoots.Add(node);
            }

            // expand the result tree items limitedly
            ExpandAllNodes(token, MaxExpandLimit);
        }

        /// <summary>
        /// searches under a node if the given search string is inside the element.label/element.id. If it's there add a search result item to the 
        /// return value, and continue search.
        /// </summary>
        /// <param name="reconciliationTreeNodePath">the actual path for the depth.</param>
        /// <returns>depth first valid tree. (searched elements are in the return value)</returns>
        private GlobalSearcherTreeNode CheckReconciliationNodeRecursively(List<ISearchableNode> reconciliationTreeNodePath) {
            GlobalSearcherTreeNode ret = null;
            Debug.Assert(reconciliationTreeNodePath.Count != 0);

            ISearchableNode reconciliationTreeNode = reconciliationTreeNodePath.Last();

            // skip abstract elements
            if (reconciliationTreeNode.Element != null && !reconciliationTreeNode.Element.IsAbstract &&
                ((_searchInLabel &&
                  reconciliationTreeNode.Element.Label.IndexOf(_searchString,
                                                              StringComparison.CurrentCultureIgnoreCase) != -1) ||
                 (_searchInId &&
                  reconciliationTreeNode.Element.Id.IndexOf(_searchString, StringComparison.CurrentCultureIgnoreCase) !=
                  -1))) {
                SearchResultItem item = new SearchResultItem {
                    PresentationTreeEntryPath = reconciliationTreeNodePath,
                    ValueTreeEntry = null,
                    TopLevel = TopLevels.Reconciliation,
                };
                // ensure that the saving in GlobalSearcherTreeNode.cs in IsSelected setter, the history can be saved, and if it's cancelled the saving is not starting.
                item.BindSaveHistory(SaveHistory, _searchString, _token);
                ret = item;
            }

            IReconciliationTreeNode treeNode = reconciliationTreeNode as IReconciliationTreeNode;

            if (treeNode == null) {
                return ret;
            }

            var innerPaths = from child in treeNode.Children
                             // skip the children of lists.
                             where child.Element == null || (!child.Element.IsList && !child.Element.IsSelectionListEntry && (!(child is ReconciliationTreeNode) || ((ReconciliationTreeNode)child).IsVisible))
                             // add new entry in the path.
                             select new List<ISearchableNode>(reconciliationTreeNodePath) {child};
            // continue search with the extended path.
            foreach (List<ISearchableNode> innerPath in innerPaths) {
                GlobalSearcherTreeNode innerRet = CheckReconciliationNodeRecursively(innerPath);
                if (innerRet != null) {
                    if (ret == null) {
                        ret = new GlobalSearcherTreeNode(reconciliationTreeNode.Element);
                        // ensure that the saving in GlobalSearcherTreeNode.cs in IsSelected setter, the history can be saved, and if it's cancelled the saving is not starting.
                        ret.BindSaveHistory(SaveHistory, _searchString, _token);
                    }
                    ret.AddChildren(innerRet);
                }
            }
            return ret;
        }

        /// <summary>
        /// bring the searched element into view
        /// </summary>
        /// <param name="value">the searched element</param>
        public override void BringIntoView(SearchResultItem value) {
            if (value == null)
                return;

            if (value.TopLevel != TopLevels.Reconciliation) {
                throw new NotImplementedException();
            }

            List<ISearchableNode> reconciliationTreePath = value.PresentationTreeEntryPath;
            ISearchableNode last = reconciliationTreePath[reconciliationTreePath.Count - 1];

            // check what is the main tab in CtlReconciliations.xaml
            foreach (IReconciliationTreeNode reconciliationTreeNode in _reconciliationsModel.PresentationTreeBalanceSheetLiabilities.RootEntries) {
                if (CheckFullPath(reconciliationTreeNode, reconciliationTreePath))
                    _reconciliationsModel.SelectedTreeIndex = 1;
            }
            foreach (IReconciliationTreeNode reconciliationTreeNode in _reconciliationsModel.PresentationTreeBalanceSheetTotalAssets.RootEntries) {
                if (CheckFullPath(reconciliationTreeNode, reconciliationTreePath))
                    _reconciliationsModel.SelectedTreeIndex = 0;
            }
            foreach (IReconciliationTreeNode reconciliationTreeNode in _reconciliationsModel.PresentationTreeIncomeStatement.RootEntries) {
                if (CheckFullPath(reconciliationTreeNode, reconciliationTreePath))
                    _reconciliationsModel.SelectedTreeIndex = 2;
            }
            IReconciliationTreeNode firstPathElement = reconciliationTreePath[0] as IReconciliationTreeNode; 
            // If there is no PresentationTreeNode, than we don't have to bring anything into view. We are finished.
            if (firstPathElement == null) {
                return;
            }
            // close all percussive nodes
            firstPathElement.UnselectAllNodes();
            // last element can be IReconciliationTreeEntry, that's why we won't select or expand it. For example a ReconciliationTransaction
            foreach (
                IReconciliationTreeEntry reconciliationTreeEntry in
                    reconciliationTreePath.Take(reconciliationTreePath.Count - 1)) {
                if (!reconciliationTreeEntry.IsExpanded)
                    reconciliationTreeEntry.IsExpanded = true;
            }
            IReconciliationTreeEntry lastReconciliationTreeEntry =
                last as IReconciliationTreeEntry;
            if (lastReconciliationTreeEntry != null && !lastReconciliationTreeEntry.IsExpanded)
                lastReconciliationTreeEntry.IsExpanded = true;
            // fire event scroll into view. NodeScrollIntoViewRequested will run in TreeViewItemTemplateSelector.cs
            // if the last element can be selected, we select it. For esample IReconciliationTransaction can't be selected.
            // all ScrollIntoView implementation should contaion UIElement.IsSelected = true;
            Dispatcher.CurrentDispatcher.BeginInvoke((Action) (
                () => last.ScrollIntoView(reconciliationTreePath)),
                                                     DispatcherPriority.SystemIdle);
        }

        /// <summary>
        /// checks if the presentationTreeRoot can be traversed as going through every element in the list.
        /// </summary>
        private bool CheckFullPath(IReconciliationTreeNode checkedNode, List<ISearchableNode> path) {
            if (path[0].Element.Id != checkedNode.Element.Id) {
                return false;
            }

            if (path.Count == 1) {
                return true;
            }

            List<ISearchableNode> oneLess = path.GetRange(1, path.Count - 1);
            foreach (IReconciliationTreeEntry reconciliationTreeEntry in checkedNode.Children)
                if (CheckFullPath(reconciliationTreeEntry as IReconciliationTreeNode, oneLess))
                    return true;
            return false;
        }
    }
}