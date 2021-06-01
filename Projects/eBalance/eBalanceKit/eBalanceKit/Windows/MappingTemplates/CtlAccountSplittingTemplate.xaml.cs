// --------------------------------------------------------------------------------
// author: Márton Garai
// since: 2012-09-03
// copyright 2012 AvenDATA GmbH
// --------------------------------------------------------------------------------
using System.Windows.Controls;

namespace eBalanceKit.Windows.MappingTemplates
{
    /// <summary>
    /// Interaction logic for CtlAccountSplittingTemplate.xaml
    /// </summary>
    public partial class CtlAccountSplittingTemplate : UserControl
    {
        public CtlAccountSplittingTemplate()
        {
            InitializeComponent();
        }

        private void BalanceListsListBoxSelectionChanged(object sender, SelectionChangedEventArgs e) {
            balanceListsListBox.UnselectAll();
        }
    }
}
