// --------------------------------------------------------------------------------
// author: Mirko Dibbert
// since: 2012-04-24
// copyright 2012 AvenDATA GmbH
// --------------------------------------------------------------------------------
using System.Windows.Controls;
using System.Windows.Input;
using eBalanceKitBusiness.Interfaces.PresentationTree;

namespace eBalanceKit.Controls {
    public partial class MonetaryNodeInfoIcons {
        public MonetaryNodeInfoIcons() { InitializeComponent(); }

        private void Image_MouseLeftButtonUp(object sender, MouseButtonEventArgs e) {
            var img = (Image)sender;
            var node = img.DataContext as IPresentationTreeNode;
            if (node != null) node.Value.SupressWarningMessages = !node.Value.SupressWarningMessages;
        }
    }
}
