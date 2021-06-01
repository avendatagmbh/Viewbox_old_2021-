// --------------------------------------------------------------------------------
// author: Mirko Dibbert
// since:  2011-08-23
// copyright 2011 AvenDATA GmbH
// --------------------------------------------------------------------------------
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using eBalanceKit.Windows.Security.Models;
using eBalanceKitResources.Localisation;

namespace eBalanceKit.Windows.Security {
    /// <summary>
    /// Interaktionslogik für CtrRightObjectTree.xaml
    /// </summary>
    public partial class CtlRightObjectTree : UserControl {
        public static readonly DependencyProperty RightManipulationAllowedProperty =
            DependencyProperty.Register("RightManipulationAllowed", typeof (bool), typeof (CtlRightObjectTree),
                                        new UIPropertyMetadata(true));

        public CtlRightObjectTree() {
            InitializeComponent();
        }

        public bool RightManipulationAllowed {
            get { return (bool) GetValue(RightManipulationAllowedProperty); }
            set { SetValue(RightManipulationAllowedProperty, value); }
        }

        // Using a DependencyProperty as the backing store for RightManipulationAllowed.  This enables animation, styling, binding, etc...

        private RoleRightTreeViewModel Model {
            get { return DataContext as RoleRightTreeViewModel; }
        }

        private void tvRights_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e) {
            if (!RightManipulationAllowed) return;

            var toggleButton = UIHelpers.TryFindFromPoint<ToggleButton>(this, e.GetPosition(this));
            if (toggleButton != null) return;

            var tvi = UIHelpers.TryFindFromPoint<TreeViewItem>(this, e.GetPosition(this));
            if (tvi == null) return;
            var item = tvi.Header as RightObjectTreeNode;
            if (item == null) return;

            if (item.IsSpecialRight) {
                if ((item.RightType != eBalanceKitBusiness.Structures.DbMapping.Rights.DbRight.RightTypes.Write) &&
                    (item.RightType != eBalanceKitBusiness.Structures.DbMapping.Rights.DbRight.RightTypes.Read)) {
                    MessageBox.Show(ResourcesCommon.ModifyRightNotPossible);
                    return;
                }
            }

            if (!item.IsEditAllowed) {
                MessageBox.Show(ResourcesCommon.ModifyRightNotAllowed);
                return;
            }

            if (item.IsChecked.HasValue) {
                if (item.IsChecked.Value) item.IsChecked = false;
                else item.IsChecked = null;
            } else {
                item.IsChecked = true;
            }

            e.Handled = true;
        }
    }
}