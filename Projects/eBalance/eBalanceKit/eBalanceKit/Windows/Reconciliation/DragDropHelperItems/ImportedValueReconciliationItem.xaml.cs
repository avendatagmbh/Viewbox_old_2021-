using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using eBalanceKitBusiness.Reconciliation.Interfaces;
using eBalanceKitResources.Localisation;

namespace eBalanceKit.Windows.Reconciliation.DragDropHelperItems
{
    /// <summary>
    /// Interaction logic for ImportedValueReconciliationItem.xaml
    /// </summary>
    public partial class ImportedValueReconciliationItem : UserControl {
        public ImportedValueReconciliationItem() { InitializeComponent(); }

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
