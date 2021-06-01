// --------------------------------------------------------------------------------
// author: Márton Garai
// since: 2012-09-07
// copyright 2012 AvenDATA GmbH
// --------------------------------------------------------------------------------
using System.Windows;
using System.Windows.Controls;
using eBalanceKitBusiness.MappingTemplate;

namespace eBalanceKit.Windows.MappingTemplates
{
    /// <summary>
    /// Interaction logic for CtlBalanceListSelection.xaml
    /// </summary>
    public partial class CtlBalanceListSelection : UserControl
    {
        public CtlBalanceListSelection()
        {
            InitializeComponent();
        }

        public Window Owner { get; set; }

        private void SelectBalanceListClick(object sender, RoutedEventArgs e) {
            Button clickedSender = (Button)sender;
            AccountGroupingModel model = (AccountGroupingModel) DataContext;
            model.SelectedBalanceListInfo = (AccountGroupingModel.BalanceListGroupListInfo)clickedSender.DataContext;
            // the button is not visible if there is no AccountGroupByBalanceList in the SelectedBalanceListInfo
            model.SelectedBalanceList = model.SelectedBalanceListInfo.AccountGroupByBalanceList[0].MaybeBalanceList;
            // the two info line on the bottom have to be refreshed.
            model.SelectedBalanceList.AccountsFilter.UpdatableItems.Add(model);
            DlgApplyGroupingTemplate dlgApplyGroupingTemplate = new DlgApplyGroupingTemplate {
                DataContext = model
            };
            dlgApplyGroupingTemplate.ShowDialog();
        }

        /// <summary>
        /// takes care of grey selection borders
        /// </summary>
        private void ListBoxSelectionChanged(object sender, SelectionChangedEventArgs e) {
            listBox.UnselectAll();
        }
    }
}
