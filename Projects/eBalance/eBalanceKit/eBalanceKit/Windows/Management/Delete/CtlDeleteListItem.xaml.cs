// --------------------------------------------------------------------------------
// author: Mirko Dibbert
// since: 2012-06-22
// copyright 2012 AvenDATA GmbH
// --------------------------------------------------------------------------------
using System.Windows;
using eBalanceKit.Windows.Management.Delete.Models;
using eBalanceKitBusiness.Structures.DbMapping;

namespace eBalanceKit.Windows.Management.Delete {
    public partial class CtlDeleteListItem {
        public CtlDeleteListItem() {
            InitializeComponent();
            DataContextChanged += (sender, args) => {
                var type = DataContext.GetType();
                if (type.GetProperty("Comment") == null)
                    txtComment.Text = null;

                if (DataContext is CheckableItemWrapper<Document>)
                    dgReportDetails.Visibility = Visibility.Visible;
            };
        }
    }
}