using System.Collections.Generic;
using Taxonomy.Interfaces;
using Taxonomy.Interfaces.PresentationTree;
using Utils;

namespace eBalanceKitBusiness.Structures.GlobalSearch {
    internal abstract class RecursiveSearcherStrategy {
        public abstract GlobalSearcherTreeNode RecursiveSearch(List<ISearchableNode> presentationTreeEntryPath);
    }
}