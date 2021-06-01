// --------------------------------------------------------------------------------
// author: Mirko Dibbert
// since:  2012-03-25
// copyright 2012 AvenDATA GmbH
// --------------------------------------------------------------------------------
using eBalanceKitBusiness.Reconciliation.Import;
using eBalanceKitBusiness.Reconciliation.Interfaces.ReconciliationTypes;

namespace eBalanceKitBusiness.Reconciliation.Interfaces {
    internal interface IReconciliationManagerInternal : IReconciliationManager {
        /// <summary>
        /// Assigns all existing transactions to the MainTaxonomy value tree of the assigned document.
        /// </summary>
        void AssignTransactions();

        /// <summary>
        /// Assigns the specified transaction to the MainTaxonomy value tree of the assigned document.
        /// </summary>
        /// <param name="transaction">Transaction which should be added.</param>
        /// <param name="addTransaction"><b>true</b> if the transaction should be added to ReconciliationInfo.Transactions.</param>
        void AssignTransaction(IReconciliationTransaction transaction, bool addTransaction = true);

        /// <summary>
        /// Removes the assignment of the specified transaction from the MainTaxonomy value tree of the assigned document.
        /// </summary>
        /// <param name="transaction">Transaction for which the assignment should be removed.</param>
        void UnassignTransaction(IReconciliationTransaction transaction);

        /// <summary>
        /// Removes all existing reconciliations.
        /// </summary>
        void DeleteAllReconciliations();

        void DeleteAllPreviousYearValues();

        void DeleteAllPreviousYearCorrectionValues();

        void ImportPreviousYearValues(PreviousYearValues previousYearValues);

        IReconciliationInfo GetReconciliationInfo(string id);

        void MergePreviousYearValues(PreviousYearValues previousYearValues);

        event SelectedChangedEventHandler OnIsSelectedChanged;
    }
}