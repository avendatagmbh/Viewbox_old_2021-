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
using eBalanceKitBusiness.Manager;

namespace eBalanceKit.Windows.Security {
    /// <summary>
    /// Interaktionslogik für DlgRoleRights.xaml
    /// </summary>
    public partial class DlgRoleRights : Window {
        public DlgRoleRights() { InitializeComponent(); }
        private void CtlRoleRights_Ok(object sender, RoutedEventArgs e) { this.Close(); }
        private void CtlRoleRights_Cancel(object sender, RoutedEventArgs e) { this.Close(); }

        private void Window_PreviewKeyDown(object sender, KeyEventArgs e) {
            if (e.Key == Key.Escape) {
                e.Handled = true;
                ctlRoleRights.OnCancel();
            }
        }   
    }
}
