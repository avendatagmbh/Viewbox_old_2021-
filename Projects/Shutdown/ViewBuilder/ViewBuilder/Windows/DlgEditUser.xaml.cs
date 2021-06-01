/**************************************************************************************************************
 * author               date            comment
 * ------------------------------------------------------------------------------------------------------------
 * Mirko Dibbert        2010-10-17      initial implementation
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
using ViewBuilderBusiness.Structures.Config;
using ViewBuilderBusiness.Manager;

namespace ViewBuilder.Windows {
    
    /// <summary>
    /// Interaktionslogik für DlgEditUser.xaml
    /// </summary>
    public partial class DlgEditUser : Window {

        /// <summary>
        /// Initializes a new instance of the <see cref="DlgEditUser"/> class.
        /// </summary>
        /// <param name="user">The user.</param>
        public DlgEditUser(UserConfig user) {
            InitializeComponent();
            this.DataContext = user;
            _isNewConfig = false;
            txtFullName.Focus();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DlgEditUser"/> class.
        /// </summary>
        public DlgEditUser() {
            InitializeComponent();
            this.DataContext = new UserConfig();
            _isNewConfig = true;
            txtName.Focus();
        }

        /*****************************************************************************************************/

        #region fields

        private bool _isNewConfig;

        #endregion fields
        
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
        /// Handles the KeyUp event of the Window control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Windows.Input.KeyEventArgs"/> instance containing the event data.</param>
        private void Window_KeyUp(object sender, KeyEventArgs e) {
            if (e.Key == Key.Escape) {
                this.Close();
            }
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

            if ((_isNewConfig && UserManager.Exists(txtName.Text)) ||
                (!_isNewConfig && txtName.Text != ((UserConfig)DataContext).Name) && UserManager.Exists(txtName.Text)) {
                MessageBox.Show(this,
                    "Benutzerkürzel ist bereits vergeben.",
                    "Fehler",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);

                txtName.Focus();
                return;
            }

            if (!txtPassword.Password.Equals(txtPassword2.Password)) {
                MessageBox.Show(this,
                    "Die Passwörter stimmen nicht überein.",
                    "Fehler",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);

                txtPassword.Focus();
                return;
            }

            OnOk();
            this.Close();
        }
        
        #endregion eventHandler

        /*****************************************************************************************************/

        #region methods

        /// <summary>
        /// Updates the binding sources.
        /// </summary>
        private void UpdateBindingSources() {
            txtFullName.GetBindingExpression(TextBox.TextProperty).UpdateSource();
            txtName.GetBindingExpression(TextBox.TextProperty).UpdateSource();
            txtEMail.GetBindingExpression(TextBox.TextProperty).UpdateSource();

            if (txtPassword.Password.Length > 0) {
                ((UserConfig)DataContext).SetPassword(txtPassword.Password);
            }
        }

        #endregion methods

    }
}
