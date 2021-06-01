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
using ViewBuilder.Models;

namespace ViewBuilder.Windows {
    /// <summary>
    /// Interaktionslogik für DlgLogOptions.xaml
    /// </summary>
    public partial class DlgLogOptions : Window {
        public DlgLogOptions() {
            InitializeComponent();
        }

        LogOptionsModel Model { get { return DataContext as LogOptionsModel; } }

        private void btnOk_Click(object sender, RoutedEventArgs e) {
            if (string.IsNullOrEmpty(tbFile.Text.Trim())) {
                MessageBox.Show(this,"Bitte einen gültigen Dateinamen angeben.");
                return;
            }
            DialogResult = true;
            Close();
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e) {
            DialogResult = false;
            Close();
        }
        void dlg_FileOk(object sender, System.ComponentModel.CancelEventArgs e) {
            Model.FileName = ((Microsoft.Win32.OpenFileDialog)sender).FileName;
        }

        private void btnSelectFile_Click(object sender, RoutedEventArgs e) {
            Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog();
            dlg.FileOk += dlg_FileOk;
            dlg.Filter = "pdf files (*.pdf)|*.pdf";
            if (!string.IsNullOrEmpty(tbFile.Text))
                dlg.InitialDirectory = new FileInfo(tbFile.Text).DirectoryName;
            //dlg.FilePath = "eBalanceKit.xls";
            dlg.CheckFileExists = false;
            dlg.Title = "Bitte wählen Sie die zu schreibende Pdf Datei.";
            dlg.Multiselect = false;
            dlg.ShowDialog();

        }

    }
}
