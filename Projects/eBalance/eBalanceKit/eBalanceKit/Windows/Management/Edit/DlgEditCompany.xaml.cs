// --------------------------------------------------------------------------------
// author: Mirko Dibbert
// since: 2012-06-22
// copyright 2012 AvenDATA GmbH
// --------------------------------------------------------------------------------
using System.Windows;
using System.Windows.Input;
using eBalanceKitBusiness.Structures.DbMapping;

namespace eBalanceKit.Windows.Management.Edit {
    public partial class DlgEditCompany {
        public DlgEditCompany(Company company) {
            InitializeComponent();
            DataContext = company;
        }

        private void BtnOkClick(object sender, RoutedEventArgs e) { DialogResult = true; }
        private void WindowKeyDown(object sender, KeyEventArgs e) { if (e.Key == Key.Escape) DialogResult = false; }
    }
}