// --------------------------------------------------------------------------------
// author: Mirko Dibbert
// since:  2011-11-15
// copyright 2011 AvenDATA GmbH
// --------------------------------------------------------------------------------
using System.Windows;

namespace eBalanceKit.Windows.MappingTemplates {
    public partial class CtlCreateTemplate {
        public CtlCreateTemplate() { InitializeComponent(); }

        private void BtnCancelClick(object sender, RoutedEventArgs e) { UIHelpers.TryFindParent<Window>(this).DialogResult = false; }
        private void BtnCreateNewTemplateClick(object sender, RoutedEventArgs e) { UIHelpers.TryFindParent<Window>(this).DialogResult = true; }
    }
}