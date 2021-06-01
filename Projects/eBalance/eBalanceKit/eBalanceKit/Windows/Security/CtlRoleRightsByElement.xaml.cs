using System.Windows;
using System.Windows.Controls;
using eBalanceKit.Windows.Security.Models;

namespace eBalanceKit.Windows.Security {
    /// <summary>
    /// Interaktionslogik für CtlRoleRightsByElement.xaml
    /// </summary>
    public partial class CtlRoleRightsByElement : UserControl {
        public CtlRoleRightsByElement() {
            InitializeComponent();
        }

        RoleRightTreeViewModel Model { get { return DataContext as RoleRightTreeViewModel; } }

        private void TreeView_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e) {
            Model.SelectedItem = tvRights.SelectedItem;

            //string header = string.Empty;
            //object node = tvRights.SelectedItem;

            //int level = 0;
            //while (node != null) {
            //    var x = node as RoleRightTreeNode;

            //    if (x.Parent != null) {
            //        level++;
            //        if (string.IsNullOrEmpty(header)) header = x.HeaderString;
            //        else header = x.HeaderString + "\\" + header;
            //    } else {
            //        if (string.IsNullOrEmpty(header)) header = x.HeaderString;
            //    }

            //    node = x.Parent;
            //}

            //switch (level) {
            //    case 0:
            //        txtRightsHeader1.Text = header;
            //        break;

            //    case 1:
            //        txtRightsHeader1.Text = "Mandant/System";
            //        break;

            //    case 2:
            //        txtRightsHeader1.Text = "Firma";
            //        break;

            //    case 3:
            //        txtRightsHeader1.Text = "Report";
            //        break;

            //    case 4:
            //        txtRightsHeader1.Text = "Reportelement";
            //        break;
            //}

            //txtRightsHeader2.Text = header;
        }


    }
}
