// --------------------------------------------------------------------------------
// author: Mirko Dibbert
// since: 2012-06-21
// copyright 2012 AvenDATA GmbH
// --------------------------------------------------------------------------------
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using eBalanceKit.Windows.Management.Edit.Models;

namespace eBalanceKit.Windows.Management.Edit {
    public partial class DlgEditSystem {
        public DlgEditSystem(eBalanceKitBusiness.Structures.DbMapping.System system) {
            InitializeComponent();
            DataContext = new EditSystemModel(this, system);
        }

        private void BtnOkClick(object sender, RoutedEventArgs e) {
            UIHelpers.UpdateBindingSources(this, TextBox.TextProperty);
            ((EditSystemModel)DataContext).SaveSystem();
            DialogResult = true;
        }

        private void BtnCancelClick(object sender, RoutedEventArgs e) { DialogResult = false; }

        private void WindowKeyDown(object sender, KeyEventArgs e) { if (e.Key == Key.Escape) DialogResult = false; }
    }
}