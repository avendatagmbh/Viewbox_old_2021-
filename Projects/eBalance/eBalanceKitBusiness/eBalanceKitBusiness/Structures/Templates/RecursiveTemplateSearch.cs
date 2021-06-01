using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using eBalanceKitBusiness.Structures.GlobalSearch;
using System.Threading;
using Taxonomy.Interfaces;
using eBalanceKitBusiness.Structures.GlobalSearch.Enums;
using Taxonomy.Interfaces.PresentationTree;
using Taxonomy.PresentationTree;

namespace eBalanceKitBusiness.Structures.Templates
{
    internal class RecursiveTemplateSearch : RecursiveSearcherStrategy
    {
        private readonly string _searchString;
        private readonly bool _searchInId;
        private readonly CancellationToken _token;
        private readonly Action<string, CancellationToken> _saveHistory;
        private readonly bool _searchInLabel;

        public RecursiveTemplateSearch(string searchString, bool searchInId,
                                            bool searchInLabel, Action<string, CancellationToken> saveHistory, CancellationToken token)
        {
            _searchString = searchString;
            _searchInId = searchInId;
            _searchInLabel = searchInLabel;
            _saveHistory = saveHistory;
            _token = token;
        }
        public override GlobalSearcherTreeNode RecursiveSearch(List<Taxonomy.Interfaces.ISearchableNode> presentationTreeEntryPath)
        {
            GlobalSearcherTreeNode ret = null;

            ISearchableNode presentationTreeEntry = presentationTreeEntryPath.Last();

            if (presentationTreeEntry.Element != null && !presentationTreeEntry.Element.IsAbstract &&
                ((_searchInLabel &&
                  (presentationTreeEntry.Element.Label.IndexOf(_searchString,
                                                               StringComparison.CurrentCultureIgnoreCase) != -1 ||
                   presentationTreeEntry.Element.LabelAlternative.IndexOf(_searchString,
                                                                          StringComparison.CurrentCultureIgnoreCase) !=
                   -1)) ||
                 (_searchInId &&
                  presentationTreeEntry.Element.Id.IndexOf(_searchString, StringComparison.CurrentCultureIgnoreCase) !=
                  -1)))
            {
                SearchResultItem item = new SearchResultItem
                                            {
                                                PresentationTreeEntryPath = presentationTreeEntryPath,
                                                TopLevel = TopLevels.Report,
                                            };
                // ensure that the saving in GlobalSearcherTreeNode.cs in IsSelected setter, the history can be saved, and if it's cancelled the saving is not starting.
                item.BindSaveHistory(_saveHistory, _searchString, _token);
                ret = item;
            }

            IPresentationTreeNode treeNode = presentationTreeEntry as IPresentationTreeNode;

            if (treeNode == null)
                return ret;

            var innerPaths = from child in treeNode.Children
                             // skip the children of lists.
                             where child.Element == null || (!child.Element.IsList && !child.Element.IsSelectionListEntry && (!(child is PresentationTreeNode) || ((PresentationTreeNode)child).IsVisible))
                             // add new entry in the path.
                             select new List<ISearchableNode>(presentationTreeEntryPath) { child };
            // continue search with the extended path.
            foreach (List<ISearchableNode> innerPath in innerPaths)
            {
                GlobalSearcherTreeNode innerRet = RecursiveSearch(innerPath);
                if (innerRet != null)
                {
                    if (ret == null)
                    {
                        ret = new GlobalSearcherTreeNode(treeNode.Element.Label);
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
