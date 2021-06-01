// --------------------------------------------------------------------------------
// author: Mirko Dibbert
// since: 2012-06-12
// copyright 2012 AvenDATA GmbH
// --------------------------------------------------------------------------------
using System.Windows;

namespace eBalanceKit.Windows.GlobalSearch {
    public partial class CtlGlobalSearch {
        public CtlGlobalSearch() { InitializeComponent(); }

        private void AvdSlideOutDialogExpanded(object sender, RoutedEventArgs e) {
            ctlGlobalSearchContent.SearchComboBox.Focus();
            // changed from textbox to combo box
            //ctlGlobalSearchContent.SearchTextBox.SelectAll();
        }
    }
}
