// --------------------------------------------------------------------------------
// author: Mirko Dibbert
// since: 2011-11-21
// copyright 2011 AvenDATA GmbH
// --------------------------------------------------------------------------------

using System;
using System.ComponentModel;
using System.Windows;
using Microsoft.Win32;
using eBalanceKitResources.Localisation;
using eBalanceKitBusiness.MappingTemplate;

namespace eBalanceKit.Windows.MappingTemplates {
    public partial class DlgShowTaxonomyUpdate {

        public DlgShowTaxonomyUpdate() {
            InitializeComponent();
        }

        private UpdateTemplateResultModel Model { get { return DataContext as UpdateTemplateResultModel; } }

        private void BtnOkClick(object sender, RoutedEventArgs e) { Close(); }

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

        private void Window_Closing(object sender, CancelEventArgs e) {
            Model.ProcessUpgradeTemplate();
        }
    }
}