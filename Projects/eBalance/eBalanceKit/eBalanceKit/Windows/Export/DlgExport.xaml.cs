// --------------------------------------------------------------------------------
// author: Sebastian Vetter, Mirko Dibbert
// since: 2012-02-09
// copyright 2012 AvenDATA GmbH
// --------------------------------------------------------------------------------
using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Input;
using eBalanceKitBusiness.Export;
using eBalanceKitResources.Localisation;
using eBalanceKitBusiness.Export.Models;
using eBalanceKitBusiness.Structures.DbMapping;
using KeyEventArgs = System.Windows.Input.KeyEventArgs;
using MessageBox = System.Windows.MessageBox;
using SaveFileDialog = Microsoft.Win32.SaveFileDialog;

namespace eBalanceKit.Windows.Export {
    public partial class DlgExport {
        public DlgExport(Document document, ExportTypes exportType) {
            InitializeComponent();
            DataContext = new ExportModel(document) {Config = {ExportType = exportType}};
        }

        private ExportModel Model { get { return DataContext as ExportModel; } }

        private void WindowPreview_OnKeyDown(object sender, KeyEventArgs e) {
            switch (e.Key) {
                case Key.Escape:
                    e.Handled = true;
                    Close();
                    break;
            }
        }

        private void btnOk_Click(object sender, RoutedEventArgs e) {
            switch (Model.Config.ExportType) {
                case ExportTypes.Pdf: {
                        SaveFileDialog dlg = new SaveFileDialog { FileName = Model.Config.DefaultFilename, Filter = ResourcesCommon.FileFilterPdf };
                        if (!dlg.ShowDialog(Owner).Value) return;
                        Model.Config.Filename = dlg.FileName;
                    }
                    break;

                case ExportTypes.Csv: {
                        var dlgFolder = new FolderBrowserDialog {
                            //Description = Model. exportInfo.SelectDestinationLabel,
                            SelectedPath = Model.Config.FilePath,
                            ShowNewFolderButton = true
                        };

                        if (dlgFolder.ShowDialog() != System.Windows.Forms.DialogResult.OK) return;
                        Model.Config.Filename = dlgFolder.SelectedPath;
                        Model.Config.FilePath = dlgFolder.SelectedPath;

                    }
                    break;

                case ExportTypes.Xbrl:
                    throw new NotImplementedException();

                default:
                    throw new ArgumentOutOfRangeException();
            }

            bool? result = Model.Export();
            if (result == null) {
                MessageBox.Show(ResourcesExport.ExportError + ": " + Model.LastExceptionMessage,
                                ResourcesExport.ExportError,
                                MessageBoxButton.OK, MessageBoxImage.Error);
            } else {
                //MessageBox.Show(ResourcesExport.ExportSuccessMessage, ResourcesExport.ExportSuccess,
                //                MessageBoxButton.OK, MessageBoxImage.Information);
                Process.Start("explorer.exe", Model.Config.FilePath);
                Close();
            }
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e) { Close(); }
    }
}