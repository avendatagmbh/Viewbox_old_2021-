// --------------------------------------------------------------------------------
// author: Mirko Dibbert
// since: 2012-05-14
// copyright 2012 AvenDATA GmbH
// --------------------------------------------------------------------------------
namespace eBalanceKit.Controls {
    public partial class CtlMonetarySources {
        public CtlMonetarySources() { InitializeComponent(); }
        
        #region properties
        private eBalanceKitBusiness.Structures.DbMapping.Document Document { get { return DataContext as eBalanceKitBusiness.Structures.DbMapping.Document; } }
        #endregion properties

        private void optBalanceList_Checked(object sender, System.Windows.RoutedEventArgs e) {

        }
    }
}
