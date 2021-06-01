// --------------------------------------------------------------------------------
// author: Mirko Dibbert
// since: 2012-03-22
// copyright 2012 AvenDATA GmbH
// --------------------------------------------------------------------------------
using System.Windows;
using eBalanceKitBusiness.Reconciliation.Interfaces.ReconciliationTypes;
using eBalanceKitResources.Localisation;

namespace eBalanceKit.Windows.Reconciliation.DragDropHelperItems {
    public partial class ReclassificationDest {

        public ReclassificationDest() { InitializeComponent(); }

        private void ImageButtonClick(object sender, RoutedEventArgs e) {
            var reclassification = DataContext as IReclassification;
            if (reclassification == null) return;
            if (MessageBox.Show(
                string.Format(ResourcesReconciliation.RequestDeletePosition, reclassification.DestinationElement.Label),
                ResourcesReconciliation.DeletePosition,
                MessageBoxButton.YesNo,
                MessageBoxImage.Question,
                MessageBoxResult.Yes) == MessageBoxResult.Yes) {

                reclassification.DestinationElement = null;
            }
        }

        private void ClickableControlMouseClick(object sender, RoutedEventArgs e) {
            var reclassification = (IReclassification)DataContext;
            reclassification.SourceTransaction.IsSelected = false;
            reclassification.DestinationTransaction.IsSelected = true;
        }

    }
}