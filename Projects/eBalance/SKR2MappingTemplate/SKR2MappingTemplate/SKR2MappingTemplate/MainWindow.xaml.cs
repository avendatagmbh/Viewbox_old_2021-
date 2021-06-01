using System;
using System.Collections.Generic;
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
using System.Windows.Navigation;
using System.Windows.Shapes;
using Microsoft.Office.Interop.Excel;
using System.IO;
using System.Xml;
using System.ComponentModel;
using System.Threading;
using OpenFileDialog = Microsoft.Win32.OpenFileDialog;

namespace SKR2MappingTemplate
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : System.Windows.Window
    {
        private MainWindowModel Model;
        public MainWindow()
        {
            InitializeComponent();
            Model = new MainWindowModel();
            DataContext = Model;
            Model.InitModel();
        }


        private void btnConvert_Click(object sender, RoutedEventArgs e)
        {
            Model.DoConvert();
        }


        private void btnTOpenFile_Click(object sender, RoutedEventArgs e)
        {
            var dlg = new OpenFileDialog();
            var modalResult = dlg.ShowDialog();
            if (modalResult.HasValue && modalResult.Value)
            {
                Model.TFileName = dlg.FileName;
            }
        }

        private void btnMOpenFile_Click(object sender, RoutedEventArgs e)
        {
            var dlg = new OpenFileDialog();
            var modalResult = dlg.ShowDialog();
            if (modalResult.HasValue && modalResult.Value)
            {
                Model.MFileName = dlg.FileName;
            }
        }

        private void btnOSelectDir_Click(object sender, RoutedEventArgs e)
        {
            var dlg = new FolderBrowserDialog();
            var modalResult = dlg.ShowDialog();
            if (modalResult.Equals(System.Windows.Forms.DialogResult.OK))
            {
                tbODirectory.Text = dlg.SelectedPath + "\\";
            }
        }
    }
}
