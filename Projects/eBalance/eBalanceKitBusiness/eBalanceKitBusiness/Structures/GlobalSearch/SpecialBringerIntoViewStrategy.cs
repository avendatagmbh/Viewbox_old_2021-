using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Taxonomy.Interfaces;
using Taxonomy.Interfaces.PresentationTree;
using eBalanceKitBase.Interfaces;

namespace eBalanceKitBusiness.Structures.GlobalSearch
{
    internal abstract class SpecialBringerIntoViewStrategy
    {
        public abstract void BringIntoView(List<ISearchableNode> presentationTreeEntryPath, INavigationTreeEntryBase navigationTreeEntry, Action<INavigationTreeEntryBase> setSelectedEntry);
    }
}
