// --------------------------------------------------------------------------------
// author: Mirko Dibbert
// since:  2011-11-18
// copyright 2011 AvenDATA GmbH
// --------------------------------------------------------------------------------
using System.Windows;

namespace eBalanceKit.Windows.MappingTemplates {
    public partial class DlgExtendTemplate {
        public DlgExtendTemplate() { InitializeComponent(); }

        private void BtnCancelClick(object sender, RoutedEventArgs e) { DialogResult = false; }
        private void BtnExtendTemplatesClick(object sender, RoutedEventArgs e) { DialogResult = true; }
    }
}