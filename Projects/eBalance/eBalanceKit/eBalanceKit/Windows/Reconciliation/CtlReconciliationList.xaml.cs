// --------------------------------------------------------------------------------
// author: Mirko Dibbert
// since: 2012-03-08
// copyright 2012 AvenDATA GmbH
// --------------------------------------------------------------------------------
using System.Windows;
using System.Windows.Controls;
using eBalanceKitBusiness.Reconciliation.ViewModels;
using eBalanceKitResources.Localisation;

namespace eBalanceKit.Windows.Reconciliation {
    public partial class CtlReconciliationList {
        public CtlReconciliationList() { InitializeComponent(); }        
        private ReconciliationsModel Model { get { return DataContext as ReconciliationsModel; } }
        
        private void BtnNewReconciliation_OnClick(object sender, RoutedEventArgs e) {
            Model.AddReconciliation(Model.DisplayedReconciliationType);
        }
        
        private void BtnDeleteReconciliation_OnClick(object sender, RoutedEventArgs e) {
            var reconciliation = ((ReconciliationsModel)((Button) sender).DataContext).SelectedReconciliation;
            if (MessageBox.Show(
                string.Format(ResourcesReconciliation.RequestDeleteReconciliation, reconciliation.Label),
                ResourcesReconciliation.DeletePosition,
                MessageBoxButton.YesNo,
                MessageBoxImage.Question,
                MessageBoxResult.Yes) == MessageBoxResult.Yes) {

                Model.DeleteSelectedReconciliation();
            }
        }
    }
}