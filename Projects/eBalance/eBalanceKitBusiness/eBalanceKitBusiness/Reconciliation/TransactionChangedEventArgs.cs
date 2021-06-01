// --------------------------------------------------------------------------------
// author: Mirko Dibbert
// since: 2012-04-02
// copyright 2012 AvenDATA GmbH
// --------------------------------------------------------------------------------
using eBalanceKitBusiness.Reconciliation.Interfaces;

namespace eBalanceKitBusiness.Reconciliation {
    public class TransactionChangedEventArgs : System.EventArgs {

        internal TransactionChangedEventArgs(IReconciliationTransaction transaction) { Transaction = transaction; }

        public IReconciliationTransaction Transaction{ get; private set; }
    }
}