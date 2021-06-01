// --------------------------------------------------------------------------------
// author: Mirko Dibbert
// since: 2012-06-21
// copyright 2012 AvenDATA GmbH
// --------------------------------------------------------------------------------

namespace eBalanceKit.Windows.Management.Edit {
    public partial class CtlEditSystem {
        public CtlEditSystem() { InitializeComponent(); }
        private void TxtNameTextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e) { ((eBalanceKitBusiness.Structures.DbMapping.System) DataContext).Validate(); }
    }
}