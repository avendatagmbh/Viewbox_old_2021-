// --------------------------------------------------------------------------------
// author: Mirko Dibbert
// since: 2012-04-02
// copyright 2012 AvenDATA GmbH
// --------------------------------------------------------------------------------
using System.Collections.Generic;

namespace eBalanceKitBusiness.Reconciliation.Interfaces {
    public interface ITransactionGroup {
        
        /// <summary>
        /// Label of the transaction group.
        /// </summary>
        string Label { get; }
        
        /// <summary>
        /// Enumeration of all assigned transactions.
        /// </summary>
        IEnumerable<IReconciliationTransaction> Transactions { get; }
        
        /// <summary>
        /// Sum of all assigned transactions.
        /// </summary>
        decimal? Sum { get; }
        
        /// <summary>
        /// Display string of Sum: (#,## €) or String.Empty if sum is null.
        /// </summary>
        string SumDisplayString { get; }

        /// <summary>
        /// Returns a value which indicates if the assigned TreeViewItem is visible.
        /// </summary>
        bool IsVisible { get; }

        /// <summary>
        /// Returns a value which indicates if the assigned TreeViewItem is selected.
        /// </summary>
        bool IsSelected { get; }

        /// <summary>
        /// Returns a value which indicates if the assigned TreeViewItem is expanded.
        /// </summary>
        bool IsExpanded { get; }
    }
}