using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using eBalanceKit.Windows.Security.Models;

namespace eBalanceKit.Windows.Security {
    
    /// <summary>
    /// Interaktionslogik für CtlEffectiveRights.xaml
    /// </summary>
    public partial class CtlEffectiveRights : UserControl {
        public CtlEffectiveRights() {
            InitializeComponent();
        }

        CtlEffectiveRightsModel Model { get { return DataContext as CtlEffectiveRightsModel; } }

        public event RoutedEventHandler Ok;
        private void OnOk() { if (Ok != null) Ok(this, new RoutedEventArgs()); }
        private void btnOk_Click(object sender, RoutedEventArgs e) { OnOk(); }
        private void UserControl_PreviewKeyDown(object sender, KeyEventArgs e) {
            if (e.Key == Key.Escape) {
                e.Handled = true;
                OnOk();
            }
        }

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
