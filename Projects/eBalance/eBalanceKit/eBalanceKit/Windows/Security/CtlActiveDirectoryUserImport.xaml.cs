// --------------------------------------------------------------------------------
// author: Solueman Hussain / Mirko Dibbert
// since:  2011-11-04
// copyright 2011 AvenDATA GmbH
// --------------------------------------------------------------------------------
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using eBalanceKit.Windows.Security.Models;
using eBalanceKitBusiness.Manager;
using eBalanceKitBusiness.Structures;

namespace eBalanceKit.Windows.Security {
    public partial class CtlActiveDirectoryUserImport : UserControl {

        public CtlActiveDirectoryUserImport() {
            InitializeComponent();
        }
        

        private Window Owner { get { return UIHelpers.TryFindParent<Window>(this); } }
        private ActiveDirectoryUserImportModel Model { get { return DataContext as ActiveDirectoryUserImportModel; } }
        private List<string> _domainList = new List<string>();

        private void BtnCancelClick(object sender, RoutedEventArgs e) { Owner.Close(); }

        private void BtnOkClick(object sender, RoutedEventArgs e) {
            Model.ImportUsers();
            Owner.Close();
        }

        private void OptBtnSortNameChecked(object sender, RoutedEventArgs e) {
            lstADuser.Items.SortDescriptions.Clear();
            lstADuser.Items.SortDescriptions.Add(new SortDescription("Name", ListSortDirection.Ascending));
        }

        private void OptBtnSortLoginNameChecked(object sender, RoutedEventArgs e) {
            lstADuser.Items.SortDescriptions.Clear();
            lstADuser.Items.SortDescriptions.Add(new SortDescription("LoginName", ListSortDirection.Ascending));
        }

        private void BorderMouseLeftButtonUp(object sender, MouseButtonEventArgs e) {
            var user = ((sender as Border).DataContext as UserInfoActiveDirectory);
            user.IsChecked = !user.IsChecked;
        }

        private void LstADuserKeyDown(object sender, KeyEventArgs e) {
            if (e.Key == Key.Space) {
                var user = lstADuser.SelectedItem as UserInfoActiveDirectory;
                if (user != null) {
                    user.IsChecked = !user.IsChecked;
                    e.Handled = true;
                }
            }
        }
    }
}