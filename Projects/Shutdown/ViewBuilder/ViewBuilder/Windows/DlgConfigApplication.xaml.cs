/**************************************************************************************************************
 * author               date            comment
 * ------------------------------------------------------------------------------------------------------------
 * Mirko Dibbert        2010-10-16      initial implementation
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
using Utils;
using ViewBuilderBusiness.Manager;
using System.Text.RegularExpressions;

namespace ViewBuilder.Windows {
    
    /// <summary>
    /// Interaktionslogik für DlgConfigApplication.xaml
    /// </summary>
    public partial class DlgConfigApplication : Window {
        public DlgConfigApplication() {
            InitializeComponent();
            this.DataContext = ApplicationManager.ApplicationConfig;
            databaseConfig.DataContext = ApplicationManager.ApplicationConfig.ConfigDbConfig;
            txtPassword.Password = ApplicationManager.ApplicationConfig.SmtpServer.Password;
        }
        
        /*****************************************************************************************************/

        #region events

        /// <summary>
        /// Occurs when the ok-button has been pressed.
        /// </summary>
        public event EventHandler Ok;

        #endregion events

        /*****************************************************************************************************/

        #region eventTrigger

        /// <summary>
        /// Called when the ok-button has been pressed.
        /// </summary>
        private void OnOk() {
            UpdateBindingSources();
            if (Ok != null) Ok(this, null);
        }

        #endregion eventTrigger

        /*****************************************************************************************************/

        #region eventHandler
        
        /// <summary>
        /// Handles the PreviewKeyUp event of the Window control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Windows.Input.KeyEventArgs"/> instance containing the event data.</param>
        private void Window_PreviewKeyUp(object sender, KeyEventArgs e) {
            if (e.Key == Key.Escape) {
                this.Close();
            }
        }
        
        /// <summary>
        /// Handles the Click event of the btnSelectDirectory control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Windows.RoutedEventArgs"/> instance containing the event data.</param>
        private void btnSelectDirectory_Click(object sender, RoutedEventArgs e) {
            System.Windows.Forms.FolderBrowserDialog dlg = new System.Windows.Forms.FolderBrowserDialog();
            dlg.Description = "Profilordner auswählen";
            dlg.RootFolder = System.Environment.SpecialFolder.Desktop;
            dlg.SelectedPath = txtDirecotry.Text;
            dlg.ShowNewFolderButton = true;
            if (dlg.ShowDialog() == System.Windows.Forms.DialogResult.OK) {
                txtDirecotry.Text = dlg.SelectedPath;
            }

            dlg.Dispose();
        }
        
        /// <summary>
        /// Handles the Checked event of the optDirectory control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Windows.RoutedEventArgs"/> instance containing the event data.</param>
        private void optDirectory_Checked(object sender, RoutedEventArgs e) {
            if (localConfig != null && databaseConfig != null) {
                localConfig.Visibility = System.Windows.Visibility.Visible;
                databaseConfig.Visibility = System.Windows.Visibility.Collapsed;
            }
        }

        /// <summary>
        /// Handles the Checked event of the optDatabase control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Windows.RoutedEventArgs"/> instance containing the event data.</param>
        private void optDatabase_Checked(object sender, RoutedEventArgs e) {
            if (localConfig != null && databaseConfig != null) {
                databaseConfig.Visibility = System.Windows.Visibility.Visible;
                localConfig.Visibility = System.Windows.Visibility.Collapsed;
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

            if (optDatabase.IsChecked == true) {
                string dbName = databaseConfig.txtDatabase.Text.Trim();
                if (dbName.Length == 0) {
                    MessageBox.Show(this,
                        "Es wurde noch kein Name für die Profildatenbank festgelegt.",
                        "Datenbankname fehlt",
                        MessageBoxButton.OK,
                        MessageBoxImage.Information);

                    return;
                }

                if (!databaseConfig.TestConnection()) return;
                if (!databaseConfig.DatabaseExists()) {
                    if (MessageBox.Show(this,
                        "Die Datenbank '" + dbName + "' existiert nicht, soll eine neue Datenbank angelegt erzeugt werden?",
                        "Datenbank existiert nicht",
                        MessageBoxButton.YesNo,
                        MessageBoxImage.Question,
                        MessageBoxResult.Yes) != MessageBoxResult.Yes) return;

                    if (!databaseConfig.CreateDatabaseIfNotExists()) return;
                }
            
            } else {
                string configDirectory = txtDirecotry.Text.Trim();
                if (configDirectory.Length == 0) {
                    MessageBox.Show(this,
                        "Es wurde noch kein Profilverzeichnis festgelegt.",
                        "Profilverzeichnis fehlt",
                        MessageBoxButton.OK,
                        MessageBoxImage.Information);

                    return;
                }
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

        #endregion eventHandler

        /*****************************************************************************************************/

        #region methods

        /// <summary>
        /// Updates the binding sources.
        /// </summary>
        private void UpdateBindingSources() {
            //*****************************************
            // profile location config
            //*****************************************
            if (optDatabase.IsChecked == true) optDatabase.GetBindingExpression(RadioButton.IsCheckedProperty).UpdateSource();
            else if (optDirectory.IsChecked == true) optDirectory.GetBindingExpression(RadioButton.IsCheckedProperty).UpdateSource();            
            
            txtDirecotry.GetBindingExpression(TextBox.TextProperty).UpdateSource();
            databaseConfig.UpdateBindings();

            //*****************************************
            // smtp server config
            //*****************************************
            txtSender.GetBindingExpression(TextBox.TextProperty).UpdateSource();
            txtServer.GetBindingExpression(TextBox.TextProperty).UpdateSource();
            txtPort.GetBindingExpression(TextBox.TextProperty).UpdateSource();
            txtUser.GetBindingExpression(TextBox.TextProperty).UpdateSource();
            ApplicationManager.ApplicationConfig.SmtpServer.Password = txtPassword.Password;
        }

        #endregion methods

        private void txtPort_PreviewTextInput(object sender, TextCompositionEventArgs e) {
            e.Handled = !Regex.IsMatch(e.Text, "[0-9]+");
        }

    }
}
