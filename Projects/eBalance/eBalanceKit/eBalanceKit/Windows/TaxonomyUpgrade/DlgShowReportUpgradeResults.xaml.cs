// --------------------------------------------------------------------------------
// author: Mirko Dibbert
// since:  2011-11-16
// copyright 2011 AvenDATA GmbH
// --------------------------------------------------------------------------------
using System.ComponentModel;
using System.Windows;
using Microsoft.Win32;
using eBalanceKitResources.Localisation;
using eBalanceKitBusiness.Manager;
using eBalanceKitBusiness.Structures;
using eBalanceKitBusiness.Structures.DbMapping;

namespace eBalanceKit.Windows.TaxonomyUpgrade {
    public partial class DlgShowReportUpgradeResults {
        private readonly Document _document;

        public DlgShowReportUpgradeResults(DocumentUpgradeResults documentUpgradeResults, Document document) {
            InitializeComponent();
            DataContext = documentUpgradeResults;
            _document = document;
        }

        private DocumentUpgradeResults Model { get { return DataContext as DocumentUpgradeResults; } }

        private void BtnOkClick(object sender, RoutedEventArgs e) { Close(); }

        private void BtnExportClick(object sender, RoutedEventArgs e) {
            var dlg = new SaveFileDialog();
            dlg.FileOk += DlgFileOk;
            dlg.Filter = "pdf " + ResourcesCommon.Files + " (*.pdf)|*.pdf";
            dlg.ShowDialog();
        }

        private void DlgFileOk(object sender, CancelEventArgs e) {
            string fileName = ((SaveFileDialog)sender).FileName;
            Model.SaveResults(fileName);
        }

        protected override void OnClosing(CancelEventArgs e) { DocumentManager.Instance.ProcessUpdateChanges(Model, _document); }

        private void BtnInfoClick(object sender, RoutedEventArgs e) { new DlgTaxonomyUpgradeInfo() { Owner = this }.ShowDialog(); }
    }
}