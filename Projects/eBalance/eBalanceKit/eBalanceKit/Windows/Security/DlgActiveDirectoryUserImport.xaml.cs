// --------------------------------------------------------------------------------
// author: Solueman Hussain / Mirko Dibbert
// since:  2011-11-04
// copyright 2011 AvenDATA GmbH
// --------------------------------------------------------------------------------
using System.Windows;
using eBalanceKit.Windows.Security.Models;

namespace eBalanceKit.Windows.Security {
    public partial class DlgActiveDirectoryUserImport : Window {
        public DlgActiveDirectoryUserImport() {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e) {
            DataContext = new ActiveDirectoryUserImportModel(this);
        }
    }
}