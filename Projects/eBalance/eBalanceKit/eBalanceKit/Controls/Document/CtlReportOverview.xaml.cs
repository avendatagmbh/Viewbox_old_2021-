using System.Windows;

namespace eBalanceKit.Controls.Document {
    public partial class CtlReportOverview {
        public CtlReportOverview() { 
            InitializeComponent(); 
        }

        #region Owner
        public Window Owner {
            set {
                ctlBalanceListManager.Owner = value;
            }
        }
        #endregion Owner
    }
}
