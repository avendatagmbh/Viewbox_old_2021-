using System;
using System.Collections.Generic;
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
using eBalanceKitConfig.Models;
using System.IO;

namespace eBalanceKitConfig.Controls {
    /// <summary>
    /// Interaktionslogik für CtlDbConfig_SQLite.xaml
    /// </summary>
    public partial class CtlDbConfig_SQLite : UserControl {
        public CtlDbConfig_SQLite() {
            InitializeComponent();
        }

        ConfigModel Model {
            get {
                return this.DataContext as ConfigModel;
            }
        }

        private void btnSelectFilename_Click(object sender, RoutedEventArgs e) {
            Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog();
            dlg.FileOk += new System.ComponentModel.CancelEventHandler(dlg_FileOk);
            dlg.Filter = "db3 files (*.db3)|*.db3|All files (*.*)|*.*";
            dlg.InitialDirectory = new FileInfo(this.txtHostname.Text).DirectoryName;
            dlg.FileName = "eBalanceKit.db3";
            dlg.CheckFileExists = false;
            dlg.Title = "Bitte wählen Sie die Datei für die SQLite Datenbank.";
            dlg.Multiselect = false;
            dlg.ShowDialog();
        }
        void dlg_FileOk(object sender, System.ComponentModel.CancelEventArgs e) {
            this.Model.HostnameSQLite = ((Microsoft.Win32.OpenFileDialog)sender).FileName;
        }
    }
}
