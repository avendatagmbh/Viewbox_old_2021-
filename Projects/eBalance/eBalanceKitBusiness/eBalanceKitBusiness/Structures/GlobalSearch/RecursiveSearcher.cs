using Taxonomy.Interfaces;
using Taxonomy.Interfaces.PresentationTree;
using System.Collections.Generic;

namespace eBalanceKitBusiness.Structures.GlobalSearch {
    /// <summary>
    /// Implements a Strategy design pattern.
    /// Used in the Global Searcher to use different types of recursive searcher.
    /// </summary>
    internal class RecursiveSearcher {
        private readonly RecursiveSearcherStrategy _strategy;

        public RecursiveSearcher(RecursiveSearcherStrategy strategy) { _strategy = strategy; }

        public GlobalSearcherTreeNode Search(List<ISearchableNode> presentationTreeEntryPath) { return _strategy.RecursiveSearch(presentationTreeEntryPath); }
    }
}