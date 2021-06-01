// --------------------------------------------------------------------------------
// author: Mirko Dibbert
// since: 2012-06-20
// copyright 2012 AvenDATA GmbH
// --------------------------------------------------------------------------------
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using eBalanceKit.Windows.Management.ManagementAssistant.Add.Models;

namespace eBalanceKit.Windows.Management.ManagementAssistant.Add {
    public partial class DlgAddSystem {
        public DlgAddSystem() {
            InitializeComponent();
            DataContext = new AddSystemModel(this);
        }

        private void BtnOkClick(object sender, RoutedEventArgs e) {
            UIHelpers.UpdateBindingSources(this, TextBox.TextProperty);
            ((AddSystemModel)DataContext).SaveSystem();
            DialogResult = true;
        }
        
        private void BtnCancelClick(object sender, RoutedEventArgs e) { DialogResult = false; }
        
        private void WindowKeyDown(object sender, KeyEventArgs e) { if (e.Key == Key.Escape) DialogResult = false; }
    }
}