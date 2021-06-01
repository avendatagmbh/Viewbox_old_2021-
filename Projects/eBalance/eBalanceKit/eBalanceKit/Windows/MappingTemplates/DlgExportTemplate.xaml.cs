// --------------------------------------------------------------------------------
// author: Mirko Dibbert
// since:  2011-11-04
// copyright 2011 AvenDATA GmbH
// --------------------------------------------------------------------------------
using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Input;
using Microsoft.Win32;
using eBalanceKitResources.Localisation;
using eBalanceKitBusiness.MappingTemplate;
using eBalanceKitBusiness.Structures.DbMapping.MappingTemplate;

namespace eBalanceKit.Windows.MappingTemplates {
    public partial class DlgExportTemplate {
        public DlgExportTemplate(MappingTemplateHead template) {
            InitializeComponent();
            Template = template;
        }

        public new MappingTemplateHead Template { get; set; }

        private void BtnSelectFileClick(object sender, RoutedEventArgs e) {
            var dlg = new SaveFileDialog();
            dlg.FileOk += DlgFileOk;
            dlg.Filter = "xml " + ResourcesCommon.Files + " (*.xml)|*.xml";
            dlg.ShowDialog();
        }

        private void DlgFileOk(object sender, CancelEventArgs e) { txtFile.Text = ((SaveFileDialog) sender).FileName; }

        private void BtnExportClick(object sender, RoutedEventArgs e) {
            try {
                TemplateManager.ExportTemplate(Template, txtFile.Text);
                MessageBox.Show(ResourcesCommon.TemplateExportSucceed);
                Close();
            } catch (Exception ex) {
                MessageBox.Show(ex.Message);
            }
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