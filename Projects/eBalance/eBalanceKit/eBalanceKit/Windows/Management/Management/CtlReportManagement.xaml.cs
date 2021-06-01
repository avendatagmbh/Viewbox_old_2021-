// --------------------------------------------------------------------------------
// author: Mirko Dibbert
// since: 2012-06-20
// copyright 2012 AvenDATA GmbH
// --------------------------------------------------------------------------------
using System.Windows;
using AvdWpfControls;
using eBalanceKit.Windows.Management.Edit;
using eBalanceKit.Windows.Management.Management.Models;

namespace eBalanceKit.Windows.Management.Management {
    public partial class CtlReportManagement {
        public CtlReportManagement() { InitializeComponent(); }

        private void ClickableControlMouseClick(object sender, RoutedEventArgs e) {
            var node = (ReportTreeLeaf)((sender as ClickableControl).DataContext);
            new DlgEditReport(node.Document) {Owner = UIHelpers.TryFindParent<Window>(this)}.ShowDialog();
            ((ReportManagementModel) DataContext).BuildTreeView();
        }            
    }
}