// --------------------------------------------------------------------------------
// author: Mirko Dibbert
// since: 2012-03-06
// copyright 2012 AvenDATA GmbH
// --------------------------------------------------------------------------------
using System.Collections.Generic;
using System.ComponentModel;
using Taxonomy;
using eBalanceKitBusiness.Reconciliation.Enums;
using eBalanceKitBusiness.Structures.DbMapping;
using eBalanceKitResources.Localisation;

namespace eBalanceKitBusiness.Reconciliation.Interfaces.ReconciliationTypes {

    public class SelectedChangedEventArgs : System.EventArgs {
        public bool PreviousState { get; private set; }
        public bool CurrentState { get; private set; }
        public SelectedChangedEventArgs(bool previousState, bool currentState) {
            PreviousState = previousState;
            CurrentState = currentState;
        }
    }
    public delegate void SelectedChangedEventHandler(IReconciliation reconciliation, SelectedChangedEventArgs args);

    public interface IReconciliation : INotifyPropertyChanged {
        Document Document { get; }
        
        string Name { get; set; }
        string Label { get; }
        string Comment { get; set; }
        bool IsSelected { get; set; }
        event SelectedChangedEventHandler OnIsSelectedChanged;

        Enums.ReconciliationTypes ReconciliationType { get; }

        /// <summary>
        /// Selected transfer kind.
        /// </summary>
        TransferKinds TransferKind { get; set; }

        /// <summary>
        /// Enumeration of all available transfer kinds.
        /// </summary>
        IEnumerable<TransferKinds> TransferKinds { get; }

        /// <summary>
        /// Returns an enumeration of all assigned transactions.
        /// </summary>
        IEnumerable<IReconciliationTransaction> Transactions { get; }

        /// <summary>
        /// Enumeration of existing warning messages.
        /// </summary>
        IEnumerable<string> Warnings { get; }

        bool IsValid { get; }
       
        /// <summary>
        /// Removes the specified transaction from the reconciliation.
        /// </summary>
        /// <param name="transaction"></param>
        void RemoveTransaction(IReconciliationTransaction transaction);

        bool IsAssignmentAllowed(IElement position, out string result);
        
    }
}