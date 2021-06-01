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
using System.Windows.Shapes;

namespace eBalanceKit.Windows.Reconciliation {
    /// <summary>
    /// Interaktionslogik für DlgChangeReconciliation.xaml
    /// </summary>
    public partial class DlgChangeReconciliation : Window {
        public DlgChangeReconciliation() {
            InitializeComponent();
        }

        private void BtnOkClick(object sender, RoutedEventArgs e) {
            DialogResult = true;
            Close();
        }

        private void BtnCancelClick(object sender, RoutedEventArgs e) {
            DialogResult = false;
            Close();
        }
    }
}
