// --------------------------------------------------------------------------------
// author: Mirko Dibbert
// since: 2012-03-29
// copyright 2012 AvenDATA GmbH
// --------------------------------------------------------------------------------
using System.Windows;
using eBalanceKitBusiness.Reconciliation.Interfaces;
using eBalanceKitResources.Localisation;

namespace eBalanceKit.Windows.Reconciliation.DragDropHelperItems {
    public partial class DeltaReconciliationItem {
        public DeltaReconciliationItem() { InitializeComponent(); }

        private void ImageButtonClick(object sender, RoutedEventArgs e) {
            var transaction = DataContext as IReconciliationTransaction;
            if (transaction == null) return;
            if (MessageBox.Show(
                string.Format(ResourcesReconciliation.RequestDeletePosition, transaction.Position.Label),
                ResourcesReconciliation.DeletePosition,
                MessageBoxButton.YesNo,
                MessageBoxImage.Question,
                MessageBoxResult.Yes) == MessageBoxResult.Yes) {

                transaction.Remove();
            }
        }
    }
}