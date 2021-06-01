using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Microsoft.Win32;

namespace eBalanceKit.Windows.Import {
    /// <summary>
    /// Interaktionslogik für CtlFileSelection.xaml
    /// </summary>
    public partial class CtlFileSelection : UserControl {
        public CtlFileSelection() {
            InitializeComponent();
        }

        private void btnSelectFile_Click(object sender, RoutedEventArgs e) {

            var dlg = new OpenFileDialog();

            //var dlg = new SaveFileDialog();
            dlg.FileOk += dlg_FileOk;
            dlg.Filter = "CSV files (*.csv)|*.csv";
            dlg.FileName = txtFilename.Text;
            //dlg.OverwritePrompt = false;
            //dlg.CreatePrompt = false;
            dlg.DefaultExt = "csv";
            dlg.Multiselect = false;
            //dlg.

            dlg.ShowDialog();

        }

        private void dlg_FileOk(object sender, CancelEventArgs e) { txtFilename.Text = ((OpenFileDialog)sender).FileName; }

    }
}
