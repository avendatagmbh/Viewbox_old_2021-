using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Windows.Threading;
using Taxonomy.Interfaces;
using Taxonomy.Interfaces.PresentationTree;
using eBalanceKitBase.Interfaces;
using eBalanceKitBusiness.Structures.GlobalSearch.Enums;
using eBalanceKitBusiness.Structures.Presentation;

namespace eBalanceKitBusiness.Structures.GlobalSearch {
    public class GlobalSearcher : SearcherBase{
        
        public GlobalSearcher(DbMapping.Document document, Action<INavigationTreeEntryBase> setSelectedNavigationTreeEntry, INavigationTree navigationTree) :base(document) {
            SetSelectedNavigationTreeEntry = setSelectedNavigationTreeEntry;

            _navigationTree = navigationTree;
        }

        public Action<INavigationTreeEntryBase> SetSelectedNavigationTreeEntry { get; set; }

        private readonly INavigationTree _navigationTree;

        /// <summary>
        /// start a search.
        /// </summary>
        /// <param name="searchString">the searched string</param>
        /// <param name="searchInId">execute the search in element.id?</param>
        /// <param name="searchInLabel">execute the search in element.label?</param>
        /// <param name="token">cancellation token for the task</param>
        public override void Search(string searchString, bool searchInId, bool searchInLabel, CancellationToken token) {
            if (string.IsNullOrEmpty(searchString)) {
                return;
            }
            if (!(searchInId || searchInLabel)) {
                return;
            }

            ResultTreeRoots.Clear();

            // RecursiveSearcher will fill the Results result list
            RecursiveSearcher searcher =
                new RecursiveSearcher(new RecursiveSearchWithValueTree(Document, searchString, searchInId,
                                                                       searchInLabel, SaveHistory, token));

            // search in GAAP presentation trees, to add search result items
            var gaapRootEntriesInList = from valueTree in Document.GaapPresentationTrees.Values
                                        // we skip the Steuerliche Modifikationen, because we won't search there anything.
                                        // the problem was with that, there is no PresentationTreeRoot, and xBrlElement is null. We can open only the navigation
                                        // trees that have one of the properties
                                        where valueTree.Role.Id != "role_transfersCommercialCodeToTax"
                                        from valueTreeRoot in valueTree.RootEntries
                                        select new List<ISearchableNode> {valueTreeRoot};

            foreach (List<ISearchableNode> oneElementPath in gaapRootEntriesInList) {
                GlobalSearcherTreeNode node = searcher.Search(oneElementPath);
                if (node != null) {
                    ResultTreeRoots.Add(node);
                }
            }

            // search in GCD presentation trees, to add search result items
            var gcdRootEntriesInList = from valueTree in Document.GcdTaxonomy.PresentationTrees
                                       from valueTreeRoot in valueTree.RootEntries
                                       select new List<ISearchableNode> {valueTreeRoot};

            foreach (List<ISearchableNode> oneElementPath in gcdRootEntriesInList) {
                GlobalSearcherTreeNode node = searcher.Search(oneElementPath);
                if (node != null) {
                    ResultTreeRoots.Add(node);
                }
            }

            ExpandAllNodes(token, MaxExpandLimit);
        }

        /// <summary>
        /// Bring the selected search element into view
        /// </summary>
        /// <param name="value">The result item of the search</param>
        public override void BringIntoView(SearchResultItem value) {
            if (value == null) {
                return;
            }

            // only search in the Report is implemented yet
            if (value.TopLevel != TopLevels.Report)
                throw new NotImplementedException();

            if (!_navigationTree.NavigationTreeReport.IsExpanded)
                _navigationTree.NavigationTreeReport.IsExpanded = true;
            List<ISearchableNode> presentationTreePath = value.PresentationTreeEntryPath;
            INavigationTreeEntryBase selectedNavigationTree =
                OpenNavigationTreeEntries(_navigationTree.NavigationTreeReport, presentationTreePath);
            // return is false, when we couldn't find the navigation tree entries from the beginning to the end in the path.
            // maybe some of the navigation tree entries are expanded, so we don't have to check all possibilities.
            // TODO: test the special cases, for example search in General Information under report. VAT ID is a good string to search for.
            if (selectedNavigationTree == null) {
                SpecialBringerIntoView bringerIntoView = new SpecialBringerIntoView(new GeneralInformationBringer());
                bringerIntoView.BringIntoView(presentationTreePath, _navigationTree.NavigationTreeReport.Children[0],
                                              SetSelectedNavigationTreeEntry);
                return;
            }
            Dispatcher.CurrentDispatcher.BeginInvoke((Action) (() => { selectedNavigationTree.IsSelected = true; }),
                                                     DispatcherPriority.SystemIdle);
            #region debug extreme path : no rootEntries, no xBrlElement
            //    foreach (INavigationTreeEntryBase problem in Problems(_navigationTree.NavigationTreeReport)) {
            //    }
            // add this function to the class
            //private IEnumerable<INavigationTreeEntryBase> Problems(INavigationTreeEntryBase actual) {
            //    if (actual.PresentationTreeRoots == null && actual.XbrlElem == null) {
            //        yield return actual;
            //        foreach (INavigationTreeEntryBase child in actual.Children) {
            //            foreach (INavigationTreeEntryBase childProblem in Problems(child)) {
            //                yield return childProblem;
            //            }
            //        }
            //    }
            //}
            #endregion
            // we need the first real IPresentationTreeEntry in the path, to unselect all it's children. 
            // If this part is skipped, every search will add a new selected item, and multiselection occurs.
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

        /// <summary>
        /// Expand the navigation tree branch that contains the root of the presentationTreePath. Recursive function.
        /// </summary>
        /// <param name="navigationTreeEntry">current branch's navigation tree root</param>
        /// <param name="presentationTreePath">the presentation tree path of the result item</param>
        /// <returns>if true, we found our branch, and we can break the traverse</returns>
        private INavigationTreeEntryBase OpenNavigationTreeEntries(INavigationTreeEntryBase navigationTreeEntry,
                                               List<ISearchableNode> presentationTreePath) {
            foreach (INavigationTreeEntryBase child in navigationTreeEntry.Children) {
                // most likely GCD navigation tree
                if (child.PresentationTreeRoots == null) {
                    // extreme case. In Steauerliche Modifikationen
                    if (child.XbrlElem == null) {
                        continue;
                    }
                    // if we don't have presentation tree we have xbrlElement. It's easier, because the Id is surely unique, there are no multiple parent possibilities.
                    if (presentationTreePath.First().Element.Id == child.XbrlElem.Id) {
                        SetSelectedNavigationTreeEntry(child);
                        if (!child.IsExpanded)
                            child.IsExpanded = true;
                        //Dispatcher.CurrentDispatcher.BeginInvoke((Action) delegate { child.IsSelected = true; },
                        //                                         DispatcherPriority.SystemIdle);
                        child.IsSelected = true;
                        // Open the right branch of this branch.
                        INavigationTreeEntryBase innerRet = OpenNavigationTreeEntries(child,
                                                                                      presentationTreePath.Skip(1).
                                                                                          ToList());
                        if (innerRet != null)
                            return innerRet;
                    }
                    continue;
                }

                // most likely GAAP navigation tree
                foreach (IPresentationTreeNode presentationTreeRoot in child.PresentationTreeRoots) {
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
                    //Dispatcher.CurrentDispatcher.BeginInvoke((Action) delegate { child.IsSelected = true; },
                    //                                         DispatcherPriority.SystemIdle);
                    // we reached a leaf, and the leaf is correct. Can't expand more branch.
                    if (child.Children.Count == 0)
                        return child;
                    // Open the right branch of this branch.
                    INavigationTreeEntryBase innerRet = OpenNavigationTreeEntries(child, presentationTreePath);
                    if (innerRet != null)
                        return innerRet;
                }
            }
            // don't break the sender's traversal.
            return null;
        }

        /// <summary>
        /// checks if the presentationTreeRoot can be traversed as going through every element in the list.
        /// </summary>
        /// <param name="path">result item's path</param>
        /// <param name="presentationTreeRoot">the root</param>
        /// <returns>true if every element is in good order in the branch</returns>
        private bool CheckEntryPath(List<ISearchableNode> path, IPresentationTreeNode presentationTreeRoot) {
            int listCount = path.Count;
            if (presentationTreeRoot.Element.Id != path[0].Element.Id) {
                return false;
            }
            if (listCount == 1) {
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