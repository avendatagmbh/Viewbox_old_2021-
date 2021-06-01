using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using Taxonomy.Interfaces;
using Taxonomy.Interfaces.PresentationTree;
using Taxonomy.PresentationTree;
using eBalanceKitBusiness.Structures.GlobalSearch.Enums;

namespace eBalanceKitBusiness.Structures.GlobalSearch {
    /// <summary>
    /// Search strategy for using the value trees as well as presentation trees. 
    /// </summary>
    internal class RecursiveSearchWithValueTree : RecursiveSearcherStrategy {
        private readonly DbMapping.Document _document;

        private readonly string _searchString;

        public RecursiveSearchWithValueTree(DbMapping.Document document, string searchString, bool searchInId,
                                            bool searchInLabel, Action<string, CancellationToken> saveHistory, CancellationToken token) {
            _document = document;
            _searchString = searchString;
            _searchInId = searchInId;
            _searchInLabel = searchInLabel;
            _saveHistory = saveHistory;
            _token = token;
        }

        /// <summary>
        /// search in ids or not.
        /// </summary>
        private readonly bool _searchInId;

        private readonly CancellationToken _token;

        private readonly Action<string, CancellationToken> _saveHistory;

        /// <summary>
        /// search in labels or not.
        /// </summary>
        private readonly bool _searchInLabel;

        /// <summary>
        /// add the results into output. We search in the Element.Id, and in the Element.Label.
        /// </summary>
        /// <param name="presentationTreeEntryPath">root</param>
        public override GlobalSearcherTreeNode RecursiveSearch(List<ISearchableNode> presentationTreeEntryPath) {
            Debug.Assert(presentationTreeEntryPath.Count != 0);

            ISearchableNode presentationTreeEntry = presentationTreeEntryPath.Last();

            GlobalSearcherTreeNode ret = null;

            // this will store the value with the given element id, if we search was successful in the valuetrees.
            IValueTreeEntry valueTreeEntry = null;
            bool haveValue = false;
            if (presentationTreeEntry.Element != null) {
                // only put search first in the ValueTreeMain because the ValueTreeGcd is more specific.
                if (_document.ValueTreeMain.Root.Values.TryGetValue(presentationTreeEntry.Element.Id, out valueTreeEntry)) {
                    haveValue = true;
                } else {
                    if (_document.ValueTreeGcd.Root.Values.TryGetValue(presentationTreeEntry.Element.Id,
                                                                       out valueTreeEntry)) {
                        haveValue = true;
                    }
                }
            }

            // TODO: set the Culture. Warning set it four occasions.
            // haveValue => search in the valueTreeEntry, not in the presentationTreeEntry. Also add in the result the ValueTreeEntry.
            if (haveValue && !valueTreeEntry.Element.IsAbstract &&
                ((_searchInId &&
                  valueTreeEntry.Element.Id.IndexOf(_searchString, StringComparison.CurrentCultureIgnoreCase) != -1) ||
                 (_searchInLabel &&
                  valueTreeEntry.Element.Label.IndexOf(_searchString, StringComparison.CurrentCultureIgnoreCase) != -1))) {
                SearchResultItem item = new SearchResultItem {
                    PresentationTreeEntryPath = presentationTreeEntryPath,
                    ValueTreeEntry = valueTreeEntry,
                    TopLevel = TopLevels.Report,
                };
                // ensure that the saving in GlobalSearcherTreeNode.cs in IsSelected setter, the history can be saved, and if it's cancelled the saving is not starting.
                item.BindSaveHistory(_saveHistory, _searchString, _token);
                ret = item;
            } else {
                if (presentationTreeEntry.Element != null && !presentationTreeEntry.Element.IsAbstract &&
                    ((_searchInLabel &&
                      presentationTreeEntry.Element.Label.IndexOf(_searchString,
                                                                  StringComparison.CurrentCultureIgnoreCase) != -1) ||
                     (_searchInId &&
                      presentationTreeEntry.Element.Id.IndexOf(_searchString, StringComparison.CurrentCultureIgnoreCase) !=
                      -1))) {
                    SearchResultItem item = new SearchResultItem {
                        PresentationTreeEntryPath = presentationTreeEntryPath,
                        ValueTreeEntry = null,
                        TopLevel = TopLevels.Report,
                    };
                    // ensure that the saving in GlobalSearcherTreeNode.cs in IsSelected setter, the history can be saved, and if it's cancelled the saving is not starting.
                    item.BindSaveHistory(_saveHistory, _searchString, _token);
                    ret = item;
                }
            }

            IPresentationTreeNode treeNode = presentationTreeEntry as IPresentationTreeNode;

            if (treeNode == null) {
                return ret;
            }

            var innerPaths = from child in treeNode.Children
                             // skip the children of lists.
                             where child.Element == null || (!child.Element.IsList && !child.Element.IsSelectionListEntry && (!(child is PresentationTreeNode) || ((PresentationTreeNode)child).IsVisible))
                             // add new entry in the path.
                             select new List<ISearchableNode>(presentationTreeEntryPath) {child};
            // continue search with the extended path.
            foreach (List<ISearchableNode> innerPath in innerPaths) {
                GlobalSearcherTreeNode innerRet = RecursiveSearch(innerPath);
                if (innerRet != null) {
                    if (ret == null) {
                        ret = new GlobalSearcherTreeNode(presentationTreeEntry.Element);
                        // ensure that the saving in GlobalSearcherTreeNode.cs in IsSelected setter, the history can be saved, and if it's cancelled the saving is not starting.
                        ret.BindSaveHistory(_saveHistory, _searchString, _token);
                    }
                    ret.AddChildren(innerRet);
                }
            }
            return ret;
        }
    }
}