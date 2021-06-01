// --------------------------------------------------------------------------------
// author: Márton Garai
// since: 2012-12-07
// copyright 2012 AvenDATA GmbH
// --------------------------------------------------------------------------------
using System.Windows;

namespace eBalanceKit.Windows
{
    /// <summary>
    /// Interaction logic for DlgSelectVersion.xaml
    /// </summary>
    public partial class DlgSelectVersion
    {
        public DlgSelectVersion() {
            InitializeComponent();
        }

        private void BtnOkClick(object sender, RoutedEventArgs e) { 
            Tag = cboVersion.SelectedValue.ToString();
            DialogResult = true;
        }
    }
}
