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
using System.Windows.Shapes;
using Microsoft.Win32;
using eBalanceKitResources.Localisation;

namespace eBalanceKit.Windows.MappingTemplates {
    /// <summary>
    /// Interaktionslogik für DlgExportDeletedElements.xaml
    /// </summary>
    public partial class DlgExportDeletedElements : Window {
        public DlgExportDeletedElements() {
            InitializeComponent();
        }

        public bool Export { set; get; }

        private void BtnExportClick(object sender, RoutedEventArgs e) { Export = true; Hide();}

        private void BtnCancelClick(object sender, RoutedEventArgs e) {
            Export = false; Close(); }

        private void BtnSelectFileClick(object sender, RoutedEventArgs e) { var saveFile = new SaveFileDialog();
        var dlg = new SaveFileDialog();
        dlg.FileOk += DlgFileOk;
        dlg.Filter = "pdf " + ResourcesCommon.Files + " (*.pdf)|*.pdf";
        dlg.ShowDialog();
        }
        private void DlgFileOk(object sender, CancelEventArgs e) { txtFile.Text = ((SaveFileDialog)sender).FileName; }
    }
}
