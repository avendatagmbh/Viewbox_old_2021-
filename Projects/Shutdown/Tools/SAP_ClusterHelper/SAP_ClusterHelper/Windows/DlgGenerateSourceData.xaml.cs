using System;
using System.Windows;
using System.Windows.Forms;
using SAP_ClusterHelper.Controlers;
using SAP_ClusterHelper.Models;
using MessageBox = System.Windows.MessageBox;

namespace SAP_ClusterHelper.Windows {
    public partial class DlgGenerateSourceData : Window {
        public DlgGenerateSourceData() { InitializeComponent();
            DataContext = new GenerateSourceDataModel();
        }

        private void BtnOkClick(object sender, RoutedEventArgs e) {
            try {
                var dlg = new DlgProgress(this);
                var progressInfo = dlg.ProgressInfo;
                var config = (GenerateSourceDataModel) DataContext;
                dlg.ExecuteModal(
                    () => GenerateSourceDataController.GenerateSourceData(config, progressInfo));
                
                MessageBox.Show("generation finished");
            } catch (Exception ex) {
                MessageBox.Show("Data generation failed: " + ex.Message);
            }
        }

        private void BtnCancelClick(object sender, RoutedEventArgs e) { Close(); }

        private void BtnSelectFolderClick(object sender, RoutedEventArgs e) {
            FolderBrowserDialog dlg = new FolderBrowserDialog();
            if (dlg.ShowDialog() == System.Windows.Forms.DialogResult.OK) {
                ((GenerateSourceDataModel) DataContext).ExportFolder = dlg.SelectedPath;
            }
        }
    }
}
