using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using eBalanceKitBusiness.Reconciliation.ViewModels;
using eBalanceKitResources.Localisation;

namespace eBalanceKit.Windows.Reconciliation {
    public partial class CtlImportPreviousYearValues {
        public CtlImportPreviousYearValues() {
            InitializeComponent();
        }

        private void HandlePreviewMouseWheel(object sender, MouseWheelEventArgs e) {
            if (e.Handled) return;
            e.Handled = true;
            var eventArg = new MouseWheelEventArgs(e.MouseDevice, e.Timestamp, e.Delta) { RoutedEvent = UIElement.MouseWheelEvent, Source = sender };
            var parent = ((Control)sender).Parent as UIElement;
            if (parent == null) return;
            parent.RaiseEvent(eventArg);
        }

        private void AssistantControlOk(object sender, RoutedEventArgs e) {
            var model = (ImportPreviousYearValuesModel) DataContext;

            if (model.HasPreviousYearValues &&
                MessageBox.Show(ResourcesReconciliation.ExistingPreviousValuesWarning, ResourcesReconciliation.ExistingPreviousValuesWarningCaption, 
                MessageBoxButton.YesNo, MessageBoxImage.Warning, MessageBoxResult.No) != MessageBoxResult.Yes) return;
            
            model.Import();
            MessageBox.Show(UIHelpers.TryFindParent<Window>(this),
                            "Überleitungswerte wurden erfolgreich aus dem Vorjahr übernommen.", "", MessageBoxButton.OK,
                            MessageBoxImage.Information);
            UIHelpers.TryFindParent<Window>(this).Close();
        }
    }
}
