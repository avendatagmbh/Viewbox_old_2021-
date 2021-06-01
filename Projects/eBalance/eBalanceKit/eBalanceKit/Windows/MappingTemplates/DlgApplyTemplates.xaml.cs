// --------------------------------------------------------------------------------
// author: Mirko Dibbert
// since:  2011-11-14
// copyright 2011 AvenDATA GmbH
// --------------------------------------------------------------------------------
using System.Windows;

namespace eBalanceKit.Windows.MappingTemplates {
    public partial class DlgApplyTemplates {
        public DlgApplyTemplates() { InitializeComponent(); }

        private void BtnCancelClick(object sender, RoutedEventArgs e) { DialogResult = false; }
        private void BtnApplyTemplateClick(object sender, RoutedEventArgs e) { DialogResult = true; }
    }
}