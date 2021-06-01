// --------------------------------------------------------------------------------
// author: Mirko Dibbert
// since: 2012-05-14
// copyright 2012 AvenDATA GmbH
// --------------------------------------------------------------------------------
using eBalanceKit.Windows.AuditMode.Models;
using eBalanceKitBusiness.AuditCorrections;
using eBalanceKitBusiness.Reconciliation.Interfaces.ReconciliationTypes;

namespace eBalanceKit.Windows.AuditMode {
    public partial class DlgTaxAudit {
        public DlgTaxAudit() {
            DataContext = new TransferAuditCorrectionsModel(this, null, null);
            InitializeComponent();
        }

        public DlgTaxAudit(IAuditCorrection correction) {
            DataContext = new TransferAuditCorrectionsModel(this, correction, null);
            InitializeComponent();
        }

        public DlgTaxAudit(IReconciliation reconciliation) {
            DataContext = new TransferAuditCorrectionsModel(this, null, reconciliation);
            InitializeComponent();
        }

    }
}
