// --------------------------------------------------------------------------------
// author: Mirko Dibbert
// since:  2011-11-12
// copyright 2011 AvenDATA GmbH
// --------------------------------------------------------------------------------
using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Input;
using Microsoft.Win32;
using eBalanceKitResources.Localisation;
using eBalanceKitBusiness.Manager;
using eBalanceKitBusiness.MappingTemplate;
using eBalanceKitBusiness.Structures.DbMapping.MappingTemplate;

namespace eBalanceKit.Windows.MappingTemplates {
    public partial class DlgImportTemplate {
        public DlgImportTemplate() {
            InitializeComponent();

            MappingTemplateHead = TemplateManager.CreateTemplate();
            DataContext = MappingTemplateHead;
        }

        private MappingTemplateHead MappingTemplateHead { get; set; }

        private void BtnSelectFileClick(object sender, RoutedEventArgs e) {
            var dlg = new OpenFileDialog();
            dlg.FileOk += DlgFileOk;
            dlg.Filter = "xml " + ResourcesCommon.Files + " (*.xml)|*.xml";
            dlg.Multiselect = false;
            dlg.ShowDialog();
        }

        private void DlgFileOk(object sender, CancelEventArgs e) {
            try {
                var file = ((OpenFileDialog) sender).FileName;
                TemplateManager.ImportTemplate(MappingTemplateHead, file);
                if (MappingTemplateHead.TaxonomyInfo == null) {
                    MessageBox.Show(ResourcesCommon.TemplateHasNoAssignedTaxonomy);
                    return;
                }
                txtFile.Text = file;

            } catch (Exception ex) {
                MessageBox.Show(ex.Message);
            }
        }

        private void BtnImportClick(object sender, RoutedEventArgs e) {           
            if (string.IsNullOrEmpty(MappingTemplateHead.Name)) {
                MessageBox.Show(ResourcesCommon.TemplateHasNoName);
                return;
            }
           
            TemplateManager.AddTemplate(MappingTemplateHead);
            DialogResult = true;
        }

        private void BtnCancelClick(object sender, RoutedEventArgs e) { Close(); }

        private void WindowPreviewKeyDown(object sender, KeyEventArgs e) {
            if (e.Key == Key.Escape) {
                Close();
                e.Handled = true;
            }
        }
    }
}