using System.Windows;

namespace eBalanceKit.Windows.Management.Edit {
    /// <summary>
    /// Interaktionslogik für DlgEditBalanceLists.xaml
    /// </summary>
    public partial class DlgEditBalanceLists : Window {
        public DlgEditBalanceLists() {
            InitializeComponent();
        }

        private void BtnOkClick(object sender, RoutedEventArgs e) {
            Close();
        }
    }
}
