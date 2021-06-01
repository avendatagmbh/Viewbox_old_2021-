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
using ViewBuilder.Models;

namespace ViewBuilder.Windows
{
    /// <summary>
    /// Interaction logic for  DlgExtendedColumnInformation.xaml
    /// </summary>
    public partial class DlgExtendedColumnInformation : Window
    {

        ExtendedColumnInformationModel Model { get { return DataContext as ExtendedColumnInformationModel; } }

        public DlgExtendedColumnInformation()
        {
            InitializeComponent();
        }


        /// <summary>
        /// Handles the Click event of the btnSelectDirectory control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Windows.RoutedEventArgs"/> instance containing the event data.</param>
        private void btnSelectDirectory_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Forms.FolderBrowserDialog dlg = new System.Windows.Forms.FolderBrowserDialog();
            dlg.Description = "Select folder";
            dlg.SelectedPath = txtScriptdir.Text;
            dlg.ShowNewFolderButton = false;
            if (dlg.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                Model.FilePath = dlg.SelectedPath;
            }

            dlg.Dispose();
        }

        private void btnRefresh_Click(object sender, RoutedEventArgs e)
        {
            Model.Refresh();
        }

        private void btnGenerate_Click(object sender, RoutedEventArgs e)
        {
            Model.Generate();
        }

        private void btnCheck_Click(object sender, RoutedEventArgs e)
        {
            Model.CheckExtInfos();
        }

        private void btnNext_Click(object sender, RoutedEventArgs e)
        {
            Model.NextFile();
        }
    }
}
