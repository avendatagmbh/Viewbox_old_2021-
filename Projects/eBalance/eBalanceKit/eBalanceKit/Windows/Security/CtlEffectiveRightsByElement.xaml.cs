using System.Windows;
using System.Windows.Controls;
using eBalanceKit.Windows.Security.Models;

namespace eBalanceKit.Windows.Security {
    /// <summary>
    /// Interaktionslogik für CtlEffectiveRightsByElement.xaml
    /// </summary>
    public partial class CtlEffectiveRightsByElement : UserControl {
        public CtlEffectiveRightsByElement() {
            InitializeComponent();
        }

        CtlEffectiveRightsModel Model { get { return DataContext as CtlEffectiveRightsModel; } }

        private void TreeView_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e) {
            Model.SelectedItem = tvRights.SelectedItem;
        }
    }
}
