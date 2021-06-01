// --------------------------------------------------------------------------------
// author: Mirko Dibbert
// since: 2012-06-20
// copyright 2012 AvenDATA GmbH
// --------------------------------------------------------------------------------
using System.Windows;
using System.Windows.Input;
using eBalanceKit.Windows.Management.Delete.Models;

namespace eBalanceKit.Windows.Management.Delete {
    public partial class DlgDeleteReport {
        public DlgDeleteReport() {
            InitializeComponent();
            DataContext = new DeleteReportModel(this);
        }

        private void BtnOkClick(object sender, RoutedEventArgs e) { DialogResult = true; }
        private void BtnCancelClick(object sender, RoutedEventArgs e) { DialogResult = false; }
        private void WindowKeyDown(object sender, KeyEventArgs e) { if (e.Key == Key.Escape) DialogResult = false; }

    }
}