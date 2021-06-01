// --------------------------------------------------------------------------------
// author: Mirko Dibbert
// since: 2012-06-20
// copyright 2012 AvenDATA GmbH
// --------------------------------------------------------------------------------
using System.Windows;
using System.Windows.Input;
using eBalanceKit.Windows.Management.Add.Models;

namespace eBalanceKit.Windows.Management.Add {
    public partial class DlgAddCompany {
        public DlgAddCompany() {
            InitializeComponent();
            DataContext = new AddCompanyModel(this);
        }

        private void BtnOkClick(object sender, RoutedEventArgs e) { DialogResult = true; }
        
        private void BtnCancelClick(object sender, RoutedEventArgs e) {
            ((AddCompanyModel) DataContext).Cancel();
            DialogResult = false;
        }

        private void WindowKeyDown(object sender, KeyEventArgs e) {
            if (e.Key == Key.Escape) {
                ((AddCompanyModel)DataContext).Cancel();
                DialogResult = false;
            }
        }
    }
}