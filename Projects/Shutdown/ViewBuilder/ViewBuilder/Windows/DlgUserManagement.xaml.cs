/**************************************************************************************************************
 * author               date            comment
 * ------------------------------------------------------------------------------------------------------------
 * Mirko Dibbert        2010-10-15      initial implementation
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
using ViewBuilderBusiness.Manager;
using ViewBuilderBusiness.Structures.Config;

namespace ViewBuilder.Windows {

    /// <summary>
    /// Interaktionslogik für DlgConfigUser.xaml
    /// </summary>
    public partial class DlgUserManagement : Window {

        /// <summary>
        /// Initializes a new instance of the <see cref="DlgUserManagement"/> class.
        /// </summary>
        public DlgUserManagement() {
            InitializeComponent();
            this.DataContext = UserManager.Users;
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
        /// Handles the Click event of the btnOk control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Windows.RoutedEventArgs"/> instance containing the event data.</param>
        private void btnOk_Click(object sender, RoutedEventArgs e) {
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
        /// Handles the Click event of the btnDeleteUser control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Windows.RoutedEventArgs"/> instance containing the event data.</param>
        private void btnDeleteUser_Click(object sender, RoutedEventArgs e) {
            try {
                UserManager.DeleteUser((UserConfig)lstUser.SelectedItem);
            } catch (Exception ex) {
                MessageBox.Show(this,
                    "Fehler beim Update der Benutzerkonfiguration: " + ex.Message,
                    "Fehler",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// Handles the Click event of the btnAddUser control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Windows.RoutedEventArgs"/> instance containing the event data.</param>
        private void btnAddUser_Click(object sender, RoutedEventArgs e) {
            DlgEditUser dlgEditUser = new DlgEditUser { Owner = this };
            dlgEditUser.Ok += new EventHandler(dlgEditUser_Ok);
            dlgEditUser.ShowDialog();
        }

        /// <summary>
        /// Handles the Click event of the btnEditUser control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Windows.RoutedEventArgs"/> instance containing the event data.</param>
        private void btnEditUser_Click(object sender, RoutedEventArgs e) {
            DlgEditUser dlgEditUser = new DlgEditUser((UserConfig)lstUser.SelectedItem) { Owner = this };
            dlgEditUser.Ok += new EventHandler(dlgEditUser_Ok);
            dlgEditUser.ShowDialog();
        }

        /// <summary>
        /// Handles the Ok event of the dlgEditUser control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        void dlgEditUser_Ok(object sender, EventArgs e) {
            try {
                UserConfig user = (UserConfig)((DlgEditUser)sender).DataContext;
                if (!UserManager.Exists(user.Name)) {
                    UserManager.AddUser(user);
                } else {
                    UserManager.Save();
                }
            } catch (Exception ex) {
                MessageBox.Show(this,
                    "Fehler beim Update der Benutzerkonfiguration: " + ex.Message,
                    "Fehler",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }
        
        /// <summary>
        /// Handles the MouseDoubleClick event of the lstUser control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Windows.Input.MouseButtonEventArgs"/> instance containing the event data.</param>
        private void lstUser_MouseDoubleClick(object sender, MouseButtonEventArgs e) {
            if (lstUser.SelectedItem != null) {
                DlgEditUser dlgEditUser = new DlgEditUser((UserConfig)lstUser.SelectedItem) { Owner = this };
                dlgEditUser.Ok += new EventHandler(dlgEditUser_Ok);
                dlgEditUser.ShowDialog();
            }
        }
        
        #endregion eventHandler

        /*****************************************************************************************************/

        #region methods

        /// <summary>
        /// Updates the binding sources.
        /// </summary>
        private void UpdateBindingSources() {
        }

        #endregion methods

    }
}
