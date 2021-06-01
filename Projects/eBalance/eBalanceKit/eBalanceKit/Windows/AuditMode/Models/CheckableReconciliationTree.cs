// --------------------------------------------------------------------------------
// author: Mirko Dibbert
// since: 2012-05-23
// copyright 2012 AvenDATA GmbH
// --------------------------------------------------------------------------------
using System.Collections.Generic;
using System.Linq;
using eBalanceKitBusiness.Reconciliation.Interfaces;
using eBalanceKitBusiness.Reconciliation.Interfaces.ReconciliationTypes;

namespace eBalanceKit.Windows.AuditMode.Models {
    public class CheckableReconciliationTree {
        public CheckableReconciliationTree(IEnumerable<IReconciliation> reconciliations) {
            RootNodes = new List<CheckableReconciliation>();
            foreach (var reconciliation in reconciliations) {
                var checkableReconciliation = new CheckableReconciliation(reconciliation);
                foreach (var transaction in reconciliation.Transactions.Where(t => t.Value.HasValue && t.Position != null)) {
                    checkableReconciliation.Transactions.Add(
                        new CheckableReconciliationTransaction(transaction, checkableReconciliation));
                }

                if (checkableReconciliation.Transactions.Any())
                    RootNodes.Add(checkableReconciliation);
            }
        }

        public List<CheckableReconciliation> RootNodes { get; private set; }

        public List<IReconciliationTransaction> GetCheckedTransactions() {
            return
                RootNodes.SelectMany(node => node.Transactions.Where(t => t.IsChecked).Select(t => t.Transaction)).
                    ToList();
        }

        public int CheckedTransactionsCount { get {
            return RootNodes.Count(n => n.IsChecked.HasValue && !n.IsChecked.Value); //GetCheckedTransactions().Count; 
        } }

    }
}