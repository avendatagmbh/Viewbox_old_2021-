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
using ViewBuilderBusiness.Persist;
using ViewBuilderBusiness.Structures;
using System.Reflection;
using ViewBuilderBusiness.Manager;
using ViewBuilderBusiness.Structures.Config;
using ViewBuilder.Models;
using ViewBuilder.Helpers;

namespace ViewBuilder.Windows
{
    /// <summary>
    /// Interaktionslogik für DlgProfileSelection.xaml
    /// </summary>
    public partial class DlgProfileSelection : Window
    {
        public DlgProfileSelection()
        {
            InitializeComponent();

            UserConfig user = new UserConfig { Name = ActiveDirectory.GetUserAbbr() };


            _user = user;
            txtCurrentUser.Content = user.DisplayString;
            profileSelector.DoubleClickProfile += profileSelector_DoubleClickProfile;
        }

        void profileSelector_DoubleClickProfile(object sender, EventArgs e)
        {
            OpenMainWindow();
        }

        /*****************************************************************************************************/

        #region fields

        private UserConfig _user;

        #endregion fields

        /*****************************************************************************************************/

        #region eventHandler

        /// <summary>
        /// Handles the Loaded event of the Window control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Windows.RoutedEventArgs"/> instance containing the event data.</param>
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            lblVersion.Content = AssemblyHelper.GetAssemblyVersion();
            lblAssemblyDate.Content = AssemblyHelper.GetAssemblyDate();
        }

        /// <summary>
        /// Handles the Click event of the btnOk control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Windows.RoutedEventArgs"/> instance containing the event data.</param>
        private void btnOk_Click(object sender, RoutedEventArgs e)
        {
            OpenMainWindow();
        }

        /// <summary>
        /// Handles the Closed event of the Window control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        private void Window_Closed(object sender, EventArgs e)
        {
            SaveApplicationConfig();
            this.DataContext = null;
        }


        /// <summary>
        /// Handles the PreviewKeyDown event of the Window control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Windows.Input.KeyEventArgs"/> instance containing the event data.</param>
        private void Window_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                if (profileSelector.SelectedIndex >= 0)
                {
                    e.Handled = true;
                    OpenMainWindow();
                }
            }
        }

        #endregion eventHandler

        /*****************************************************************************************************/

        #region methods

        /// <summary>
        /// Opens the main window.
        /// </summary>
        private void OpenMainWindow()
        {
            ProfileConfig profile = ProfileManager.Open(profileSelector.SelectedItem.ToString());
            profile.AssignedUser = _user;
            Settings.CurrentProfileConfig = profile;
            new MainWindow(profile).Show();
            this.Close();
        }

        /// <summary>
        /// Saves the application config.
        /// </summary>
        private void SaveApplicationConfig()
        {
            try
            {
                if (profileSelector.SelectedItem != null)
                {
                    ApplicationManager.ApplicationConfig.LastProfile = profileSelector.SelectedItem.ToString();
                    ApplicationManager.Save();
                }
            }
            catch (Exception ex)
            {
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
