// --------------------------------------------------------------------------------
// author: Mirko Dibbert
// since: 2012-06-20
// copyright 2012 AvenDATA GmbH
// --------------------------------------------------------------------------------
using System.Windows.Input;
using eBalanceKit.Windows.Management.Management.Models;

namespace eBalanceKit.Windows.Management.Management {
    public partial class DlgReportManagement {
        public DlgReportManagement() {
            InitializeComponent();
            DataContext = new ReportManagementModel(this);
        }

        private void BtnOkClick(object sender, System.Windows.RoutedEventArgs e) { Close(); }
        private void WindowKeyDown(object sender, KeyEventArgs e) { if (e.Key == Key.Escape) DialogResult = false; }
    }
}