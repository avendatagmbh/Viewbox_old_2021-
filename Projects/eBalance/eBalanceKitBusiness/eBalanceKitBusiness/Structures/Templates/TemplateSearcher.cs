using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using eBalanceKitBusiness.Structures.GlobalSearch;
using Taxonomy.Interfaces.PresentationTree;
using Taxonomy.Interfaces;
using eBalanceKitBase.Interfaces;
using eBalanceKitBusiness.Structures.GlobalSearch.Enums;
using System.Windows.Threading;
using eBalanceKitBusiness.Structures.Presentation;
using eBalanceKitBusiness.Export;


namespace eBalanceKitBusiness.Structures.Templates
{
    public class TemplateSearcher : SearcherBase
    {
        private List<Taxonomy.Interfaces.PresentationTree.IPresentationTree> _presTree;
        private readonly INavigationTree _navigationTree;

        public Action<INavigationTreeEntryBase> SetSelectedNavigationTreeEntry { get; set; }

        public TemplateSearcher(List<Taxonomy.Interfaces.PresentationTree.IPresentationTree> presTree, Action<INavigationTreeEntryBase> setSelectedNavigationTreeEntry, INavigationTree navigationTree)
            : base(null)
        {
            _presTree = presTree;
            _navigationTree = navigationTree;
            SetSelectedNavigationTreeEntry = setSelectedNavigationTreeEntry;
        }

        public override void Search(string searchString, bool searchInId, bool searchInLabel, System.Threading.CancellationToken token)
        {
            if (string.IsNullOrEmpty(searchString) || !(searchInId || searchInLabel))
                return;

            ResultTreeRoots.Clear();

            RecursiveSearcher searcher =
                new RecursiveSearcher(new RecursiveTemplateSearch(searchString, searchInId, searchInLabel, SaveHistory, token));

            Dictionary<string, bool> searchInAndRoleUriMapping = new Dictionary<string, bool> {
                {
                    "balanceSheet"
                    , SearchInBalanceSheet
                    },
                {
                    "contingentLiabilities"
                    , SearchInContingentLiabilities
                    },
                {
                    "incomeStatement"
                    , SearchInIncomeStatement
                    },
                {
                    "appropriationProfits"
                    , SearchInAppropriationProfit
                    },
                {
                    "cashFlowStatement"
                    , SearchInCashFlowStatement
                    },
                {
                    "notes"
                    , SearchInNotes
                    },
                {
                    "managementReport"
                    , SearchInManagementReport
                    },
                {
                    "OtherReportElements"
                    , SearchInOtherReportElements
                    },
                {
                    "adjustmentOfIncome"
                    , SearchInAdjustmentOfIncome
                    },
                {
                    "determinationOfTaxableIncome"
                    , SearchInDeterminationOfTaxableIncome
                    },
                {
                    "determinationOfTaxableIncomeBusinessPartnership"
                    ,
                    SearchInDeterminationOfTaxableIncomeBusinessPartnership
                    },
                {
                    "determinationOfTaxableIncomeSpecialCases"
                    ,
                    SearchInDeterminationOfTaxableIncomeSpecialCases
                    },
                {
                    "changesEquityAccounts"
                    ,
                    SearchInChangesEquityAccounts
                    }
            };

            foreach (KeyValuePair<string, bool> pair in searchInAndRoleUriMapping)
            {
                if (!pair.Value) continue;
                string uri = ExportHelper.GetUriInPresentationTreeList(pair.Key, _presTree);

                if (!string.IsNullOrEmpty(uri))
                {
                    foreach (IPresentationTree child in _presTree)
                    {
                        if (child.Role.RoleUri == uri)
                        {
                            GlobalSearcherTreeNode node =
                                searcher.Search(new List<ISearchableNode> {child.Nodes.First()});
                            if (node != null)
                                ResultTreeRoots.Add(node);
                        }
                    }
                }
            }

            ExpandAllNodes(token, MaxExpandLimit);
        }

        public override void BringIntoView(SearchResultItem value)
        {
            if (value == null)
                return;

            if (value.TopLevel != TopLevels.Report)
                throw new NotImplementedException();

            if (!_navigationTree.NavigationTreeReport.IsExpanded)
                _navigationTree.NavigationTreeReport.IsExpanded = true;

            List<ISearchableNode> presentationTreePath = value.PresentationTreeEntryPath;
            INavigationTreeEntryBase selectedNavigationTree =
                OpenNavigationTreeEntries(_navigationTree.NavigationTreeReport, presentationTreePath);


            Dispatcher.CurrentDispatcher.BeginInvoke((Action)(() => { if (!selectedNavigationTree.IsSelected) selectedNavigationTree.IsSelected = true; }),
                                         DispatcherPriority.SystemIdle);

            presentationTreePath = presentationTreePath.SkipWhile(element => !(element is PresentationTreeNode)).ToList();
            PresentationTreeNode firstPathElement = presentationTreePath[0] as PresentationTreeNode; 
            // If there is no PresentationTreeNode, than we don't have to bring anything into view. We are finished.
            if (firstPathElement == null) {
                return;
            }
            // all item count - 1 elements and root entries should be eBal....IPresentationTreeNode.
            foreach (PresentationTreeNode percussiveRootEntries in firstPathElement.PresentationTree.RootEntries) {
                percussiveRootEntries.UnselectAll();
            }
            // last element can be IPresentationTreeEntry, that's why we won't select or expand it. For example a BalanceListEntry
            foreach (
                Interfaces.PresentationTree.IPresentationTreeNode presentationTreeEntry in
                    presentationTreePath.Take(presentationTreePath.Count - 1)) {
                if (!presentationTreeEntry.IsExpanded)
                    presentationTreeEntry.IsExpanded = true;
            }
            ISearchableNode last = presentationTreePath[presentationTreePath.Count - 1];
            Interfaces.PresentationTree.IPresentationTreeNode lastPresentationTreeEntry =
                last as Interfaces.PresentationTree.IPresentationTreeNode;
            if (lastPresentationTreeEntry != null && !lastPresentationTreeEntry.IsExpanded)
                lastPresentationTreeEntry.IsExpanded = true;
            // fire event scroll into view. NodeScrollIntoViewRequested will run in TaxonomyViewModel.cs or TaxonomyUIElements.cs
            // if the last element can be selected, we select it. For example a BalanceListEntry can't be selected.
            // all ScrollIntoView implementation should contaion UIElement.IsSelected = true;
            Dispatcher.CurrentDispatcher.BeginInvoke((Action) (() => last.ScrollIntoView(presentationTreePath)),
                                                     DispatcherPriority.SystemIdle);

        }

        internal INavigationTreeEntryBase OpenNavigationTreeEntries(INavigationTreeEntryBase navigationTreeEntry,
                                               List<ISearchableNode> presentationTreePath)
        {
            foreach (INavigationTreeEntryBase child in navigationTreeEntry.Children)
            {
                //// most likely GCD navigation tree
                //if (child.PresentationTreeRoots == null)
                //{
                //    // extreme case. In Steauerliche Modifikationen
                //    if (child.XbrlElem == null)
                //    {
                //        continue;
                //    }
                //    // if we don't have presentation tree we have xbrlElement. It's easier, because the Id is surely unique, there are no multiple parent possibilities.
                //    if (presentationTreePath.First().Element.Id == child.XbrlElem.Id)
                //    {
                //        SetSelectedNavigationTreeEntry(child);
                //        if (!child.IsExpanded)
                //            child.IsExpanded = true;
                //        if (!child.IsSelected)
                //            child.IsSelected = true;
                //        //Dispatcher.CurrentDispatcher.BeginInvoke((Action) delegate { child.IsSelected = true; },
                //        //                                         DispatcherPriority.SystemIdle);
                //        child.IsSelected = true;
                //        // Open the right branch of this branch.
                //        INavigationTreeEntryBase innerRet = OpenNavigationTreeEntries(child,
                //                                                                      presentationTreePath.Skip(1).
                //                                                                          ToList());
                //        if (innerRet != null)
                //            return innerRet;
                //    }
                //    continue;
                //}

                // most likely GAAP navigation tree
                foreach (IPresentationTreeNode presentationTreeRoot in child.PresentationTreeRoots)
                {
                    // Check the whole path is inside the actual child branch. If it is there, we are finished.
                    // TODO : Test extreme case : We search for a label that is in the "C" PresentationNode. 
                    // TODO : The Notes navigation tree root is below of Bilanz. Ordering is important.
                    //      navigation tree looks like : Bilanz/A/B/C           Stored SearchResultItem's path = A/B/C
                    //      next node looks like this :  Notes/A/C              Stored SearchResultItem's path = A/C
                    // I tried to avoid error when we try to choose the second SearchResultItem. I don't know if it works, and I can't test it.
                    if (!CheckEntryPath(presentationTreePath, presentationTreeRoot))
                        continue;
                    SetSelectedNavigationTreeEntry(child);
                    if (!child.IsExpanded)
                        child.IsExpanded = true;
                    if (!child.IsSelected)
                        child.IsSelected = true;

                    return child;


                    ////Dispatcher.CurrentDispatcher.BeginInvoke((Action) delegate { child.IsSelected = true; },
                    ////                                         DispatcherPriority.SystemIdle);
                    //// we reached a leaf, and the leaf is correct. Can't expand more branch.
                    //if (child.Children.Count == 0)
                    //    return child;
                    //// Open the right branch of this branch.
                    //INavigationTreeEntryBase innerRet = OpenNavigationTreeEntries(child, presentationTreePath);
                    //if (innerRet != null)
                    //    return innerRet;
                }
            }
            // don't break the sender's traversal.
            return null;
        }

        private bool CheckEntryPath(List<ISearchableNode> path, IPresentationTreeNode presentationTreeRoot)
        {
            int listCount = path.Count;
            if (presentationTreeRoot.Element.Id != path[0].Element.Id)
            {
                return false;
            }
            if (listCount == 1)
            {
                return true;
            }
            List<ISearchableNode> oneLess = path.GetRange(1, listCount - 1);
            foreach (IPresentationTreeNode presentationTreeNode in presentationTreeRoot.Children)
                if (CheckEntryPath(oneLess, presentationTreeNode))
                    return true;
            return false;
        }

    }
}
