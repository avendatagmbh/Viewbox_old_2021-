// --------------------------------------------------------------------------------
// author: Mirko Dibbert
// since: 2012-05-24
// copyright 2012 AvenDATA GmbH
// --------------------------------------------------------------------------------
using System.Collections.Generic;
using System.Linq;
using eBalanceKitBusiness.AuditCorrections;

namespace eBalanceKit.Windows.AuditMode.Models {
    public class CheckableAuditCorrectionTree {
        public CheckableAuditCorrectionTree(IEnumerable<IAuditCorrection> auditCorrections) {
            RootNodes = new List<CheckableAuditCorrection>();
            foreach (var auditCorrection in auditCorrections) {
                var checkableAuditCorrection = new CheckableAuditCorrection(auditCorrection);
                foreach (var transaction in auditCorrection.Transactions.Where(t => t.Value.HasValue && t.Element != null)) {
                    checkableAuditCorrection.Transactions.Add(
                        new CheckableAuditCorrectionTransaction(transaction, checkableAuditCorrection));
                }

                if (checkableAuditCorrection.Transactions.Any())
                    RootNodes.Add(checkableAuditCorrection);
            }
        }

        public List<CheckableAuditCorrection> RootNodes { get; private set; }

        public List<IAuditCorrectionTransaction> GetCheckedTransactions() {
            return
                RootNodes.SelectMany(node => node.Transactions.Where(t => t.IsChecked).Select(t => t.Transaction)).
                    ToList();
        }

    }
}