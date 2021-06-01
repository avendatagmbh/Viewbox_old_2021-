using System;
using System.Collections.Generic;
using System.Diagnostics;
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
using Utils;
using ViewBuilder.Models;
using ViewBuilderBusiness.SapBillSchemaImport;
using ViewBuilderBusiness.Structures;
using System.IO;
using System.Collections.ObjectModel;

namespace ViewBuilder.Windows {
    /// <summary>
    /// Interaction logic for DlgSapBillSchemaImport.xaml
    /// </summary>
    public partial class DlgSapBillSchemaImport : Window
    {
        public readonly DlgSapBillSchemaImportViewModel ViewModel;

        public DlgSapBillSchemaImport() {
            InitializeComponent();
            ViewModel=new DlgSapBillSchemaImportViewModel();
            DataContext = ViewModel;
        }

        /*****************************************************************************************************/

        #region events

        /// <summary>
        /// Occurs when the ok-button has been pressed.
        /// </summary>
        public event EventHandler Ok;

        #endregion events

        /*****************************************************************************************************/

        #region eventsTrigger

        /// <summary>
        /// Called when the ok-button has been pressed.
        /// </summary>
        private void OnOk() {
            if (Ok != null) Ok(this, null);
        }

        #endregion eventsTrigger

        /*****************************************************************************************************/

        #region eventHandler
        
        /// <summary>
        /// Handles the KeyUp event of the Window control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Windows.Input.KeyEventArgs"/> instance containing the event data.</param>
        private void Window_KeyUp(object sender, KeyEventArgs e) {
            if (e.Key == Key.Escape) {
                this.Close();
                e.Handled = true;
            }
        }

        /// <summary>
        /// Handles the Click event of the btnCancel control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Windows.RoutedEventArgs"/> instance containing the event data.</param>
        private void btnCancel_Click(object sender, RoutedEventArgs e) {
            this.Close();
        }

        /// <summary>
        /// Handles the Click event of the btnOk control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Windows.RoutedEventArgs"/> instance containing the event data.</param>
        private void btnOk_Click(object sender, RoutedEventArgs e) {

            if (!Directory.Exists(txtScriptdir.Text)) {
                MessageBox.Show(this,
                    "The specified directory does not exist!",
                    "Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);

                return;
            }
            
            OnOk();
            if (!ViewModel.Files.Any(w=>w.Selected))
                this.Close();
        }

        /// <summary>
        /// Handles the Closed event of the Window control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        private void Window_Closed(object sender, EventArgs e) {
            this.DataContext = null;
        }

        /// <summary>
        /// Handles the Click event of the btnSelectDirectory control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Windows.RoutedEventArgs"/> instance containing the event data.</param>
        private void btnSelectDirectory_Click(object sender, RoutedEventArgs e) {
            System.Windows.Forms.FolderBrowserDialog dlg = new System.Windows.Forms.FolderBrowserDialog();
            dlg.Description = "Select folder";
            dlg.SelectedPath = txtScriptdir.Text;
            dlg.ShowNewFolderButton = false;
            if (dlg.ShowDialog() == System.Windows.Forms.DialogResult.OK) {
                ViewModel.FilePath = dlg.SelectedPath;
            }

            dlg.Dispose();
        }


        #endregion eventHandler

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Button btn = (Button) sender;
            Process.Start((string)btn.Tag);
        }
        
        /*****************************************************************************************************/

        #region methods

        #endregion methods
    }
}
