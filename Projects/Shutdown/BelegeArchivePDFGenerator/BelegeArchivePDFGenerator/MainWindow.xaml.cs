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
using System.Windows.Forms;
using BelegeArchivePDFGenerator.PdfGenerator;
using Microsoft.Win32;
using System.Collections.Concurrent;
using Forms = System.Windows.Forms;
using BelegeArchivePDFGenerator.DataModels;
using System.ComponentModel;
using iTextSharp.text;
using System.IO;
using iTextSharp.text.pdf;

namespace BelegeArchivePDFGenerator
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private ConcurrentBag<APDFGenerator> generators = new ConcurrentBag<APDFGenerator>();
        private ConcurrentBag<DataModel> belegArchiveData = new ConcurrentBag<DataModel>();

        public MainWindow()
        {
            InitializeComponent();          
        }

        private void browseDestBtn_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new Forms.FolderBrowserDialog();

            dialog.ShowDialog();
            destTb.Text = dialog.SelectedPath;
        }

        private void browseFilePathBtn_Click(object sender, RoutedEventArgs e)
        {
            Microsoft.Win32.OpenFileDialog dialog = new Microsoft.Win32.OpenFileDialog();
            dialog.DefaultExt = ".jpg";
            dialog.Filter = "JPEG Files (*.jpeg)|*.jpeg|PNG Files (*.png)|*.png|JPG Files (*.jpg)|*.jpg|GIF Files (*.gif)|*.gif"; 

            Nullable<bool> result = dialog.ShowDialog();

            if (result == true)
            {
                // Open document 
                string filename = dialog.FileName;
                logoPathLabel.Content = filename;
            }
        }

        private void databaseCb_DropDownOpened(object sender, EventArgs e)
        {
            string connectionString =
                "HOST=" + hostTb.Text + ";" +
                "PORT=" + portTb.Text + ";" +
                "UID=" + uidTb.Text + ";" +
                "PASSWORD=" + passwordTb.Password + ";";

            var dbList = DbSelect.ShowDatabases(connectionString);
            if(dbList != null)
            {
                DbConnection.connectionString = connectionString;
                if (DbSelect.ShowDatabases(connectionString) != null)
                {
                    databaseCb.ItemsSource = dbList;
                }
            }
        }

        private void generateBtn_Click(object sender, RoutedEventArgs e)
        {
            DbConnection.connectionString += "DATABASE=" + databaseCb.Text + ";";
            belegArchiveData = DbSelect.SelectRowsFrom("belegarchive", 10000, 1000);

            foreach (var row in belegArchiveData)
            {
                generators.Add(new ConcretePrintDocPDFGenerator(row, destTb.Text, logoPathLabel.Content.ToString()));
            }

            procPb.Maximum = generators.Count;

            foreach (var geni in generators)
            {
                BackgroundWorker bw = new BackgroundWorker();
                bw.WorkerReportsProgress = true;
                bw.WorkerSupportsCancellation = true;
                bw.DoWork += new DoWorkEventHandler(bw_Do_PDF_Generator_Work);
                bw.RunWorkerCompleted += new RunWorkerCompletedEventHandler(bw_PDF_Generator_RunWorkerCompleted);
                bw.RunWorkerAsync(geni);

            }
        }

        void bw_Do_PDF_Generator_Work(object sender, DoWorkEventArgs e)
        {
            APDFGenerator geni = e.Argument as APDFGenerator;
            geni.Generate();
        }

        void bw_PDF_Generator_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            procPb.Value += 1;
        }
    }
}
