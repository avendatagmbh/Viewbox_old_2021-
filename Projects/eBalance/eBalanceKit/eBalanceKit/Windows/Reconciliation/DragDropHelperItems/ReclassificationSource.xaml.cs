// --------------------------------------------------------------------------------
// author: Mirko Dibbert
// since: 2012-03-22
// copyright 2012 AvenDATA GmbH
// --------------------------------------------------------------------------------
using System.Windows;
using eBalanceKitBusiness.Reconciliation.Interfaces.ReconciliationTypes;
using eBalanceKitResources.Localisation;

namespace eBalanceKit.Windows.Reconciliation.DragDropHelperItems {
    public partial class ReclassificationSource {
        public ReclassificationSource() { InitializeComponent(); }

        private void ImageButtonClick(object sender, RoutedEventArgs e) {
            var reclassification = DataContext as IReclassification;
            if (reclassification == null) return;
            if (MessageBox.Show(
                string.Format(ResourcesReconciliation.RequestDeletePosition, reclassification.SourceElement.Label),
                ResourcesReconciliation.DeletePosition,
                MessageBoxButton.YesNo,
                MessageBoxImage.Question,
                MessageBoxResult.Yes) == MessageBoxResult.Yes) {

                reclassification.SourceElement = null;
            }
        }

        private void ClickableControlMouseClick(object sender, RoutedEventArgs e) {
            var reclassification = (IReclassification) DataContext;
            reclassification.DestinationTransaction.IsSelected = false;
            reclassification.SourceTransaction.IsSelected = true;
        }

    }
}