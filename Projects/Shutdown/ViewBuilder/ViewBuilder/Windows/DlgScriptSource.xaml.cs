/**************************************************************************************************************
 * author               date            comment
 * ------------------------------------------------------------------------------------------------------------
 * Mirko Dibbert        2010-10-14      initial implementation
 *************************************************************************************************************/

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
using ViewBuilderBusiness.Structures;
using System.IO;

namespace ViewBuilder.Windows {

    /// <summary>
    /// Interaktionslogik für DlgScriptSource.xaml
    /// </summary>
    public partial class DlgScriptSource : Window {

        /// <summary>
        /// Initializes a new instance of the <see cref="DlgScriptSource"/> class.
        /// </summary>
        /// <param name="scriptSource">The script source object.</param>
        public DlgScriptSource(ConfigScriptSource scriptSource) {
            InitializeComponent();
            DataContext = scriptSource;
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
            UpdateBindingSources();
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
                    "Das angegebene Verzeichnis existiert nicht!",
                    "Fehler",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);

                return;
            }
            
            OnOk();
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
            dlg.Description = "Profilordner auswählen";
            //dlg.RootFolder = System.Environment.SpecialFolder.Desktop;
            //dlg.RootFolder = txtScriptdir.Text;
            dlg.SelectedPath = txtScriptdir.Text;
            dlg.ShowNewFolderButton = false;
            if (dlg.ShowDialog() == System.Windows.Forms.DialogResult.OK) {
                txtScriptdir.Text = dlg.SelectedPath;
            }

            dlg.Dispose();
        }

        #endregion eventHandler

        /*****************************************************************************************************/

        #region methods

        /// <summary>
        /// Updates the binding sources.
        /// </summary>
        private void UpdateBindingSources() {
            //if (optDirectory.IsChecked == true) ((ConfigScriptSource)DataContext).ScriptSourceMode = ScriptSourceMode.Directory;
            //else if (optDatabase.IsChecked == true) ((ConfigScriptSource)DataContext).ScriptSourceMode = ScriptSourceMode.Database;
            ((ConfigScriptSource)DataContext).ScriptSourceMode = ScriptSourceMode.Directory;

            chkIncludeSubDirectories.GetBindingExpression(CheckBox.IsCheckedProperty).UpdateSource();
            txtScriptdir.GetBindingExpression(TextBox.TextProperty).UpdateSource();
            //optDirectory.GetBindingExpression(RadioButton.IsCheckedProperty).UpdateSource();
        }

        #endregion methods
    }
}
