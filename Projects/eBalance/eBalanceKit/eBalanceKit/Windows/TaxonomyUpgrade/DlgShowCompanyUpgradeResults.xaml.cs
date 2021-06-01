// --------------------------------------------------------------------------------
// author: Mirko Dibbert
// since:  2011-11-20
// copyright 2011 AvenDATA GmbH
// --------------------------------------------------------------------------------
using System.ComponentModel;
using System.Windows;
using Microsoft.Win32;
using eBalanceKitResources.Localisation;
using eBalanceKitBusiness.Structures;

namespace eBalanceKit.Windows.TaxonomyUpgrade {
    public partial class DlgShowCompanyUpgradeResults {
        public DlgShowCompanyUpgradeResults() { InitializeComponent(); }

        private CompanyUpgradeResults Model { get { return DataContext as CompanyUpgradeResults; } }

        private void BtnSaveClick(object sender, RoutedEventArgs e) {
            var dlg = new SaveFileDialog();
            dlg.FileOk += DlgFileOk;
            dlg.Filter = "pdf " + ResourcesCommon.Files + " (*.pdf)|*.pdf";
            dlg.ShowDialog();
        }

        private void DlgFileOk(object sender, CancelEventArgs e) {
            string fileName = ((SaveFileDialog)sender).FileName;
            Model.SaveResults(fileName);
        }

        private void BtnOkClick(object sender, RoutedEventArgs e) { Close(); }

        private void WindowClosing(object sender, System.ComponentModel.CancelEventArgs e) { Model.ProcessChanges(); }
        
        private void BtnInfoClick(object sender, RoutedEventArgs e) { new DlgTaxonomyUpgradeInfo() { Owner = this }.ShowDialog(); }
    }
}