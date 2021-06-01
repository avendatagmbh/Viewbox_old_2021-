using System;
using System.Collections.Generic;
using Taxonomy.Interfaces;
using Taxonomy.Interfaces.PresentationTree;
using eBalanceKitBase.Interfaces;

namespace eBalanceKitBusiness.Structures.GlobalSearch
{
    /// <summary>
    /// Implements a Strategy design pattern.
    /// Used in the Global Searcher to use different types of special bring into view.
    /// </summary>
    internal class SpecialBringerIntoView
    {
        private readonly SpecialBringerIntoViewStrategy _strategy;

        public SpecialBringerIntoView(SpecialBringerIntoViewStrategy strategy) { _strategy = strategy; }

        public void BringIntoView(List<ISearchableNode> presentationTreeEntryPath, INavigationTreeEntryBase navigationTreeEntry, Action<INavigationTreeEntryBase> setSelectedEntry) { _strategy.BringIntoView(presentationTreeEntryPath, navigationTreeEntry, setSelectedEntry); }
    }

}
