// --------------------------------------------------------------------------------
// author: Mirko Dibbert
// since: 2012-07-02
// copyright 2012 AvenDATA GmbH
// --------------------------------------------------------------------------------
using eBalanceKit.Windows.Reconciliation.Models;
using eBalanceKitBusiness.Reconciliation.Enums;
using eBalanceKitBusiness.Reconciliation.ViewModels;
using eBalanceKitBusiness.Structures.DbMapping;

namespace eBalanceKit.Windows.Reconciliation {
    public partial class DlgAddReconciliationAssistant {
        public DlgAddReconciliationAssistant(ReconciliationsModel reconciliationsModel) {
            InitializeComponent();
            DataContext = new AddReconciliationModel(this, reconciliationsModel.Document);
            if (reconciliationsModel.Document.ReconciliationMode == ReconciliationMode.TaxBalanceSheetValues)
                NewReconciliationAssistant.NavigateNext();
        }

        AddReconciliationModel Model { get { return DataContext as AddReconciliationModel; } }

        private void AssistantControl_Ok(object sender, System.Windows.RoutedEventArgs e) {
            if (!Model.Validate())
                return;
            Model.AddReconciliation();
            Close();
        }
    }
}