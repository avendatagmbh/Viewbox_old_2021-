// --------------------------------------------------------------------------------
// author: Mirko Dibbert
// since: 2012-04-20
// copyright 2012 AvenDATA GmbH
// --------------------------------------------------------------------------------
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using eBalanceKitBusiness.Reconciliation.ViewModels;
using eBalanceKitResources.Localisation;

namespace eBalanceKit.Windows.Reconciliation.DetailViews {
	public partial class PreviousYearDetails {
		public PreviousYearDetails() { InitializeComponent(); }

        private void HandlePreviewMouseWheel(object sender, MouseWheelEventArgs e) {
            if (e.Handled) return;
            e.Handled = true;
            var eventArg = new MouseWheelEventArgs(e.MouseDevice, e.Timestamp, e.Delta) { RoutedEvent = UIElement.MouseWheelEvent, Source = sender };
            var parent = ((Control)sender).Parent as UIElement;
            if (parent == null) return;
            parent.RaiseEvent(eventArg);
        }
        
        private void DeleteOnClick(object sender, RoutedEventArgs e) {

            if (MessageBox.Show(
                string.Format(ResourcesReconciliation.RequestDeleteAllPreviousYearValues),
                ResourcesReconciliation.DeletePosition,
                MessageBoxButton.YesNo,
                MessageBoxImage.Question,
                MessageBoxResult.No) == MessageBoxResult.Yes) {

                var model = (ReconciliationsModel)DataContext;
                model.DeleteAllPreviousYearValues();
            }
        }

        private void ImportOnClick(object sender, RoutedEventArgs e) {
            var model = new ImportPreviousYearValuesModel();
            if (model.PreviousYearReports.Any())
                new DlgImportPreviousYearValues {Owner = UIHelpers.TryFindParent<Window>(this), DataContext = model}.
                    ShowDialog();
            else {
                MessageBox.Show(ResourcesReconciliation.NoExistingReportsWithPreviousYearValues,
                                ResourcesReconciliation.ImportNotPossible, MessageBoxButton.OK, MessageBoxImage.Error);
            } 
        }

        private void DeleteAuditCorrectionsOnClick(object sender, RoutedEventArgs e) {
            if (MessageBox.Show(
                string.Format(ResourcesReconciliation.RequestDeleteAllPreviousYearCorrectionValues),
                ResourcesReconciliation.DeletePosition,
                MessageBoxButton.YesNo,
                MessageBoxImage.Question,
                MessageBoxResult.No) == MessageBoxResult.Yes) {

                var model = (ReconciliationsModel)DataContext;
                model.DeleteAllPreviousYearCorrectionValues();
            }
        }
	}
}