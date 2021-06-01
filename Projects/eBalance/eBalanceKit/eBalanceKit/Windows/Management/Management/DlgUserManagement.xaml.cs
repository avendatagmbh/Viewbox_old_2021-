// --------------------------------------------------------------------------------
// author: Mirko Dibbert
// since: 2012-06-20
// copyright 2012 AvenDATA GmbH
// --------------------------------------------------------------------------------
using System.Windows;
using System.Windows.Input;

namespace eBalanceKit.Windows.Management.Management {
    public partial class DlgUserManagement {
        public DlgUserManagement() { InitializeComponent(); }
        private void BtnOkClick(object sender, RoutedEventArgs e) { Close(); }
        private void WindowKeyDown(object sender, KeyEventArgs e) { if (e.Key == Key.Escape) DialogResult = false; }
    }
}