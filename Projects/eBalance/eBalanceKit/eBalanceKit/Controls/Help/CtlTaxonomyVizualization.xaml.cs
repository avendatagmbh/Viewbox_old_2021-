using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Microsoft.Win32;
using eBalanceKitBase.Windows;
using eBalanceKitBusiness.Export;
using eBalanceKitResources.Localisation;

namespace eBalanceKit.Controls.Help {
    /// <summary>
    /// Interaktionslogik für CtlTaxonomyVizualization.xaml
    /// </summary>
    public partial class CtlTaxonomyVizualization : UserControl {
        public CtlTaxonomyVizualization() {
            InitializeComponent();
            DataContext = new Models.Assistants.TaxonomyVizualizationModel();
        }

        Models.Assistants.TaxonomyVizualizationModel Model { get { return DataContext as Models.Assistants.TaxonomyVizualizationModel; } }

        private void CtlTaxonomyDescription_MouseDown(object sender, MouseButtonEventArgs e) {
            var type = (Taxonomy.Enums.TaxonomyType)((CtlTaxonomyDescription) sender).Tag;

            var fileExt = ".xlsx";
            var parent = Utils.UIHelpers.TryFindParent<Window>(this);
            SaveFileDialog dlg = new SaveFileDialog {
                FileName = "Vizualization_" + type.ToString() + "_" + Model.SelectedVersion + fileExt, Filter = ResourcesCommon.FileFilterXlsx
            };
            var result = dlg.ShowDialog(parent);
            if (!result.Value) return;
            var usedFilePath = dlg.FileName.EndsWith(fileExt) ? dlg.FileName : (dlg.FileName + fileExt);
            var path = System.IO.Path.GetDirectoryName(usedFilePath);
            try {
                //Model.GenerateVizualization(type, usedFilePath);
                XlsxExporter exporter = new XlsxExporter();
                DlgProgress progressBar = new DlgProgress(parent);
                string error = null;
                // needed to double the reference, because the Export runs on different thread.
                string selectedVersion = Model.SelectedVersion;
                progressBar.ProgressInfo.Caption = ResourcesExport.ProgressBarInfoForXlsxExport;
                progressBar.ProgressInfo.IsIndeterminate = true;
                progressBar.ExecuteModal(delegate { error = exporter.Export(usedFilePath, type, selectedVersion); });

                if (error != null) {
                    MessageBox.Show(parent, error, "Problem",
                                    MessageBoxButton.OK, MessageBoxImage.Exclamation);
                } else {
                    Process.Start("explorer.exe",
                                  ExportHelper.CheckVirtualStoreUsage(path, parent)
                                      ? ExportHelper.VirtualStoreRoot
                                      : path);
                }
            } catch (Exception exception) {
                MessageBox.Show(parent, exception.Message, "Problem",
                                MessageBoxButton.OK, MessageBoxImage.Exclamation);
            }
        }
    }
}
