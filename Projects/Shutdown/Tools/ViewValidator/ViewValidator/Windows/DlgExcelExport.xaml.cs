using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using ViewValidatorLogic.Manager;

namespace ViewValidator.Windows {
    /// <summary>
    /// Interaktionslogik für DlgExcelExport.xaml
    /// </summary>
    public partial class DlgExcelExport : Window {
        public DlgExcelExport() {
            InitializeComponent();

            tbFile.Text = ApplicationManager.ApplicationConfig.LastExcelFile;
        }

        private void btnOk_Click(object sender, RoutedEventArgs e) {
            if (string.IsNullOrEmpty(tbFile.Text.Trim())) {
                MessageBox.Show("Bitte einen gültigen Dateinamen angeben.");
                return;
            }
            ApplicationManager.ApplicationConfig.LastExcelFile = tbFile.Text;
            this.DialogResult = true;
            this.Close();
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e) {
            this.DialogResult = false;
            this.Close();
        }

        void dlg_FileOk(object sender, System.ComponentModel.CancelEventArgs e) {
            tbFile.Text = ((Microsoft.Win32.OpenFileDialog)sender).FileName;
        }

        private void btnSelectFile_Click(object sender, RoutedEventArgs e) {
            Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog();
            dlg.FileOk += new System.ComponentModel.CancelEventHandler(dlg_FileOk);
            dlg.Filter = "xls files (*.xls)|*.xls";
            if (!string.IsNullOrEmpty(this.tbFile.Text))
                dlg.InitialDirectory = new FileInfo(this.tbFile.Text).DirectoryName;
            //dlg.FileName = "eBalanceKit.xls";
            dlg.CheckFileExists = false;
            dlg.Title = "Bitte wählen Sie die zu schreibende Excel Datei.";
            dlg.Multiselect = false;
            dlg.ShowDialog();

        }
    }
}
