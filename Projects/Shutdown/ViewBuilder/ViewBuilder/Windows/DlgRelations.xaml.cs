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
using System.Windows.Shapes;
using Microsoft.Win32;
using ViewBuilder.Models;

namespace ViewBuilder.Windows {
    /// <summary>
    /// Interaktionslogik für DlgRelations.xaml
    /// </summary>
    public partial class DlgRelations : Window {
        public DlgRelations() {
            InitializeComponent();
        }

        RelationsModel Model { get { return DataContext as RelationsModel; } }

        private void btnSelectFolder_Click(object sender, RoutedEventArgs e) {
            var dlg = new OpenFileDialog();
            dlg.InitialDirectory = @"Q:\Großprojekte\";
            dlg.Filter = "Csv-Dateien (*.csv)|*.csv";
            dlg.ShowDialog();
            if (!string.IsNullOrEmpty(dlg.FileName)) {
                Model.CsvPath = dlg.FileName;
            }
        }

        private void btnOk_Click(object sender, RoutedEventArgs e) {
            Model.Save();
            Close();
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e) {
            Close();
        }

        private void btnAddRelations_Click(object sender, RoutedEventArgs e) {
            Model.AddRelations();
        }

        private void btnUpdateFileContent_Click(object sender, RoutedEventArgs e) {
            Model.UpdateFileContents();
        }
    }
}
