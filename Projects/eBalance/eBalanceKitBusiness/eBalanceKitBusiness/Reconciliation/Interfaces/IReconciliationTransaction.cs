// --------------------------------------------------------------------------------
// author: Mirko Dibbert
// since:  2012-03-22
// copyright 2012 AvenDATA GmbH
// --------------------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.ComponentModel;
using Taxonomy;
using eBalanceKitBusiness.Reconciliation.Enums;
using eBalanceKitBusiness.Reconciliation.Interfaces.ReconciliationTypes;
using eBalanceKitBusiness.Structures.DbMapping;

namespace eBalanceKitBusiness.Reconciliation.Interfaces {
    public interface IReconciliationTransaction : INotifyPropertyChanged, IReconciliationTreeEntry {

        event EventHandler Validated;

        Document Document { get; }
        
        string Label { get; }

        /// <summary>
        /// Gets the assigned Reconciliation element.
        /// </summary>
        IReconciliation Reconciliation { get; }

        /// <summary>
        /// Gets or sets the assigned position.
        /// </summary>
        IElement Position { get; set; }

        /// <summary>
        /// Gets or sets the assigned value.
        /// </summary>
        decimal? Value { get; set; }

        string ValueDisplayString { get; }

        ReconciliationInfo ReconciliationInfo { get; set; }

        TransactionTypes TransactionType { get; }

        IEnumerable<string> Warnings { get; }

        bool IsValid { get; }

        /// <summary>
        /// Returns true iif. this transaction is selected.
        /// </summary>
        //bool IsSelected { get; set; }

        /// <summary>
        /// Returns true iif. the assigned reconciliation is selected.
        /// </summary>
        bool IsReconciliationSelected { get; }

        bool IsAssignmentAllowed(IElement position);

        /// <summary>
        /// Removes this transaction from the assigned reconciliation.
        /// </summary>
        void Remove();

        /// <summary>
        /// Validates the reconciliation transaction.
        /// </summary>
        void Validate();

        void SetParent(IReconciliationTreeNode parent);
        void ResetParent();

    }
}