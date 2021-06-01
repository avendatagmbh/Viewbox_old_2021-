using System.Diagnostics;
using System.Windows;
using eBalanceKitBusiness.Export.Models;
using Microsoft.Win32;
using eBalanceKitResources.Localisation;
using System.IO;

namespace eBalanceKit.Windows.Export
{
    /// <summary>
    /// Interaction logic for DlgExportCompany.xaml
    /// </summary>
    public partial class DlgExportCompany : Window
    {
        public DlgExportCompany() {
            InitializeComponent();
            DataContext = new ExportCompanyModel();
        }

        /// <summary>
        /// Export the company into csv file
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BtnOk_OnClick(object sender, RoutedEventArgs e)
        {
            SaveFileDialog dlgSaveFile = new SaveFileDialog
            {
                FileName = ((ExportCompanyModel)DataContext).FileName,
                DefaultExt = ".csv",
                Filter = "csv " + ResourcesCommon.Files + " (*.csv)|*.csv"
            };
            var result = dlgSaveFile.ShowDialog();
            if (!result.Value) return;
            ((ExportCompanyModel)DataContext).FileName = dlgSaveFile.FileName;
            ((ExportCompanyModel)DataContext).Export();
            var info = new DirectoryInfo(((ExportCompanyModel) DataContext).FileName).Parent;
            if (info != null)
                Process.Start("explorer.exe", info.FullName);
            Close();
        }
        
        private void BtnCancel_OnClick(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
