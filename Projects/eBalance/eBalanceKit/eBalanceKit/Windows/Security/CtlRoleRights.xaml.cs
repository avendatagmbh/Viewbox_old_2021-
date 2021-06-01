using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using eBalanceKit.Windows.Security.Models;

namespace eBalanceKit.Windows.Security {
    /// <summary>
    /// Interaktionslogik für CtlRoleRights.xaml
    /// </summary>
    public partial class CtlRoleRights : UserControl {
        public CtlRoleRights() {
            InitializeComponent();
        }

        RoleRightTreeViewModel Model { get { return DataContext as RoleRightTreeViewModel; } }

        #region events
        //--------------------------------------------------------------------------------
        public event RoutedEventHandler Cancel;
        internal void OnCancel() {
            if (Cancel != null) Cancel(this, new RoutedEventArgs());
        }

        public event RoutedEventHandler Ok;
        private void OnOk() {
            Model.Role.Save();
            if (Ok != null) Ok(this, new RoutedEventArgs());
        }

        private void btnOk_Click(object sender, RoutedEventArgs e) { OnOk(); }
        private void btnAcceptChanges_Click(object sender, RoutedEventArgs e) { Model.Role.Save(); }
        private void btnCancel_Click(object sender, RoutedEventArgs e) { OnCancel(); }
        private void UserControl_PreviewKeyDown(object sender, KeyEventArgs e) { if (e.Key == Key.Escape) { e.Handled = true; OnCancel(); } }


        //--------------------------------------------------------------------------------
        #endregion events


        private void btnCollapseAllNodes_Click(object sender, RoutedEventArgs e) {
            foreach (var node in Model.RootNodes) SetIsExpanded(node, false);
        }

        private void btnExpandAllNodes_Click(object sender, RoutedEventArgs e) {
            foreach (var node in Model.RootNodes) SetIsExpanded(node, true);
        }

        private void SetIsExpanded(eBalanceKitBusiness.IRightTreeNode node, bool state) {
            node.IsExpanded = state;
            foreach (var child in node.Children) SetIsExpanded(child, state);
        }

    }
}
