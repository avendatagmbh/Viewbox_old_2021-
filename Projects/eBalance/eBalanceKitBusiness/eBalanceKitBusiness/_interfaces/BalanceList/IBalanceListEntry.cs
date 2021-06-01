using System;
using Taxonomy.Interfaces.PresentationTree;
using eBalanceKitBusiness.Structures.DbMapping;
using System.Collections.Generic;
using eBalanceKitBusiness.Interfaces;
using eBalanceKitBusiness.Structures.DbMapping.BalanceList;
using System.ComponentModel;
using eBalanceKitBusiness.Structures;

namespace eBalanceKitBusiness {

    /// <summary>
    /// Interface for balance list entries, e.g. account or account groups.
    /// </summary>
    /// <author>Mirko Dibbert</author>
    /// <since>2011-06-13</since>
    public interface IBalanceListEntry : IPresentationTreeEntry, ITaxonomyAssignment, INotifyPropertyChanged {       
        IBalanceList BalanceList { get; }
        decimal Amount { get; set; }
        bool IsChecked { get; set; }
        bool IsSelected { get; set; }
        string Name { get; set; }
        string Number { get; set; }
        string Comment { get; set; }
        string ValueDisplayString { get; }
        bool SendBalance { get; }
        long Id { get; }
        string Label { get; }
        string SortIndex { get; set; }
        bool IsVisible { get; set; }
        bool IsHidden { get; set; }
        bool IsInTray { get; set; }
        
        bool DoDbUpdate { get; set; }

        bool GetVisibility(BalanceListFilterOptions filterOptions);
        void Save();
    }
}
