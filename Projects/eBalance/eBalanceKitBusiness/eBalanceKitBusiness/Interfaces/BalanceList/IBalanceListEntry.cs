// --------------------------------------------------------------------------------
// author: Mirko Dibbert
// since:  2011-06-13
// copyright 2011 AvenDATA GmbH
// --------------------------------------------------------------------------------
using System;
using System.ComponentModel;
using Taxonomy.Interfaces.PresentationTree;
using eBalanceKitBusiness.Interfaces;
using eBalanceKitBusiness.Structures;

namespace eBalanceKitBusiness {
    /// <summary>
    /// Interface for balance list entries, e.g. account or account groups.
    /// </summary>
    public interface IBalanceListEntry : IsSelectable, IPresentationTreeEntry, ITaxonomyAssignment, INotifyPropertyChanged {
        IBalanceList BalanceList { get; }
        decimal Amount { get; set; }
        bool IsChecked { get; set; }
        string Name { get; set; }
        string Number { get; set; }
        string Comment { get; set; }
        string ValueDisplayString { get; }
        bool SendBalance { get; }
        string Label { get; }
        string SortIndex { get; set; }
        bool IsVisible { get; set; }
        bool IsHidden { get; set; }
        bool IsInTray { get; set; }
        bool IsDocumentSelectedBalanceList { get; set; }
        bool DoDbUpdate { get; set; }

        /// <param name="showHiddenEntriesAnyway">For example assigned accounts should be shown even if they were invisble before.</param>
        bool GetVisibility(BalanceListFilterOptions filterOptions, bool showHiddenEntriesAnyway = false);
        void Save();
    }
}