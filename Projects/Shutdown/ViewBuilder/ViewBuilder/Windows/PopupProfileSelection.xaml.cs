/**************************************************************************************************************
 * author               date            comment
 * ------------------------------------------------------------------------------------------------------------
 * Mirko Dibbert        2010-10-23      initial implementation
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
using ViewBuilder.Models;
using ViewBuilderBusiness.Structures.Config;
using ViewBuilderBusiness.Manager;

namespace ViewBuilder.Windows {
    
    /// <summary>
    /// Interaktionslogik für PopupProfileSelection.xaml
    /// </summary>
    public partial class PopupProfileSelection : Window {
        internal PopupProfileSelection(MainWindowModel mainWindowModel) {
            InitializeComponent();
            _mainWindowModel = mainWindowModel;
        }

        private MainWindowModel _mainWindowModel;

        /*****************************************************************************************************/

        #region eventHandler

        /// <summary>
        /// Handles the Click event of the btnOk control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Windows.RoutedEventArgs"/> instance containing the event data.</param>
        private void btnOk_Click(object sender, RoutedEventArgs e) {
            UpdateProfile();
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
        /// Handles the Closed event of the Window control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        private void Window_Closed(object sender, EventArgs e) {
            SaveApplicationConfig();
            this.DataContext = null;
        }


        /// <summary>
        /// Handles the PreviewKeyDown event of the Window control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Windows.Input.KeyEventArgs"/> instance containing the event data.</param>
        private void Window_PreviewKeyDown(object sender, KeyEventArgs e) {
            if (e.Key == Key.Enter) {
                if (profileSelector.SelectedIndex >= 0) {
                    e.Handled = true;
                    UpdateProfile();
                }
            
            } else if (e.Key == Key.Escape) {
                this.Close();
            }
        }

        /// <summary>
        /// Handles the Loaded event of the Window control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Windows.RoutedEventArgs"/> instance containing the event data.</param>
        private void Window_Loaded(object sender, RoutedEventArgs e) {
            profileSelector.Focus();
        }

        #endregion eventHandler

        /*****************************************************************************************************/

        #region methods

        /// <summary>
        /// Opens the main window.
        /// </summary>
        private void UpdateProfile() {
            _mainWindowModel.Profile = ProfileManager.Open(profileSelector.SelectedItem.ToString());
            this.Close();
        }

        /// <summary>
        /// Saves the application config.
        /// </summary>
        private void SaveApplicationConfig() {
            try {
                ApplicationManager.ApplicationConfig.LastProfile = profileSelector.SelectedItem.ToString();
                ApplicationManager.Save();
            } catch (Exception ex) {
                MessageBox.Show(this,
                    "Fehler beim Speichern der Programmkonfiguration: " + ex.Message,
                    "Fehler",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        #endregion methods

    }
}
