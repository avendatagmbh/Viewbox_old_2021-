// --------------------------------------------------------------------------------
// author: Mirko Dibbert
// since:  2011-09-26
// copyright 2011 AvenDATA GmbH
// --------------------------------------------------------------------------------
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace eBalanceKit.Windows.EditPresentationTreeDetails {
    /// <summary>
    /// Interaktionslogik für DlgEditTextDetails.xaml
    /// </summary>
    public partial class DlgEditTextDetails : Window {
        public DlgEditTextDetails() {
            InitializeComponent();
            txtValue.Focus();
        }

        private void SaveValues() {
            txtValue.GetBindingExpression(TextBox.TextProperty).UpdateSource();
            txtComment.GetBindingExpression(TextBox.TextProperty).UpdateSource();
        }

        private void Window_PreviewKeyDown(object sender, KeyEventArgs e) {
            switch (e.Key) {
                case Key.Escape:
                    e.Handled = true;
                    DialogResult = false;
                    break;
                case Key.F2:
                    e.Handled = true;
                    SaveValues();
                    DialogResult = true;
                    break;
            }
        }

        private void btnOk_Click(object sender, RoutedEventArgs e) {
            SaveValues();
            DialogResult = true;
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e) { DialogResult = false; }

        private void btnAcceptChanges_Click(object sender, RoutedEventArgs e) { SaveValues(); }
    }
}