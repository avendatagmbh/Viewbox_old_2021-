using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using eBalanceKit.Models.Export;
using eBalanceKit.Windows.Export;
using eBalanceKitBusiness.Export;
using eBalanceKitBusiness.Structures.DbMapping;
using KeyEventArgs = System.Windows.Input.KeyEventArgs;
using SaveFileDialog = Microsoft.Win32.SaveFileDialog;

namespace eBalanceKit.Windows
{
    /// <summary>
    /// Interaktionslogik für DlgExportPdf.xaml
    /// </summary>
    public partial class DlgExportPdf : Window
    {
        private ExportPdfModel myModel { get { return DataContext as ExportPdfModel; } }

        public DlgExportPdf(Document document)
        {
            InitializeComponent();
            this.DataContext = new ExportPdfModel(document); //new ConfigExport(document); 
            //myModel = new ExportPdfModel(document);
            
        }


        private void btnCancel_Click(object sender, RoutedEventArgs e) { Close(); }


        private void Window_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                e.Handled = true;
                Close();
            }
            else if (e.Key == Key.Return) {
                e.Handled = true;
                NextTab();
            }
            /*
            if (e.Key == Key.LeftCtrl) {
                e.Handled = true;
                myModel.ExportHyperCube();
            }
            */
        }

        public void NextTab() { this.tabControl1.SelectedIndex++; }
        public void BackTab() { this.tabControl1.SelectedIndex--; }

        private void checkEnabled() {
            if (tabControl1.SelectedIndex == 0) {
                btBack.IsEnabled = false;
                btNext.IsEnabled = true;
            } else if (tabControl1.SelectedIndex == tabControl1.Items.Count - 1) {
                btNext.IsEnabled = false;
                btBack.IsEnabled = true;
            } else {
                btBack.IsEnabled = true;
                btNext.IsEnabled = true;
            }
        }


        public ConfigExport pdfConfig { get; set; }

        private void btBack_Click(object sender, RoutedEventArgs e)
        {
            BackTab();
        }

        private void btNext_Click(object sender, RoutedEventArgs e)
        {
            NextTab();
        }




        private void btnExport_Click(object sender, RoutedEventArgs e) {

            bool? result = myModel.Export();
            
            if (result == null) {
                this.txtStatus.Text = myModel.LastExceptionMessage;
                lbStatus.Visibility = Visibility.Visible;
            } else {
                this.txtStatus.Text = "Export erfolgreich beendet.";
            }
            

        }



        private void tabControl1_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            checkEnabled();
        }

    }
}
