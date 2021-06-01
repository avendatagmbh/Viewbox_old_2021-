using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace eBalanceKit.Windows.EditPresentationTreeDetails {
    public partial class DlgEditMonetaryDetails {
        public DlgEditMonetaryDetails() {
            InitializeComponent();
        }

        private void SaveValues() {
            txtManualValue.GetBindingExpression(TextBox.TextProperty).UpdateSource();
            txtComment.GetBindingExpression(TextBox.TextProperty).UpdateSource();
        }

        private void Window_PreviewKeyDown(object sender, KeyEventArgs e) {
            if (e.Key == Key.Escape) {
                e.Handled = true;
                DialogResult = false;
            } else if (e.Key == Key.F2) {
                e.Handled = true;
                SaveValues();
                DialogResult = true;
            }
        }

        private void btnOk_Click(object sender, RoutedEventArgs e) {
            SaveValues();
            DialogResult = true;
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e) {
            DialogResult = false;
        }

        private void btnAcceptChanges_Click(object sender, RoutedEventArgs e) {
            SaveValues();
        }

    }
}
