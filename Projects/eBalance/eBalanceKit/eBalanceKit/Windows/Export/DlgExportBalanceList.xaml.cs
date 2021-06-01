// --------------------------------------------------------------------------------
// author: Lajos Szoke
// since: 2012-12-06
// copyright 2012 AvenDATA GmbH
// --------------------------------------------------------------------------------
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

namespace eBalanceKit.Windows.Export {
    public partial class DlgExportBalanceList
    {
        public DlgExportBalanceList(eBalanceKitBase.Interfaces.INavigationTree navigationTree, Document document) {
            InitializeComponent();
            DataContext = new ExportModel(document, navigationTree, ExportTypes.BalanceList);
            chkBalanceListsByFile.Visibility = document.BalanceListsImported.Count > 1 ? Visibility.Visible : Visibility.Collapsed;
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
            var dlgFolder = new FolderBrowserDialog {
                SelectedPath = Model.Config.FilePath,
                ShowNewFolderButton = true
            };

            if (dlgFolder.ShowDialog() != System.Windows.Forms.DialogResult.OK) return;
            Model.Config.FilePath = dlgFolder.SelectedPath;
            
            bool? result = Model.Export();
            if (result == null) {
                MessageBox.Show(ResourcesExport.ExportError + ": " + Model.LastExceptionMessage,
                                ResourcesExport.ExportError,
                                MessageBoxButton.OK, MessageBoxImage.Error);
            } else {
                Process.Start("explorer.exe", ExportHelper.CheckVirtualStoreUsage(Model.Config.FilePath, this) ? ExportHelper.VirtualStoreRoot : Model.Config.FilePath);
                Close();
            }
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e) { Close(); }
    }
}