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
using System.Runtime.InteropServices;
using ViewBuilderBusiness.Structures;
using System.Collections.ObjectModel;
using ViewBuilderBusiness.Structures.Config;
using ViewBuilderBusiness.Manager;

namespace ViewBuilder.Windows
{
    /// <summary>
    /// Interaktionslogik für DlgEditProfile.xaml
    /// </summary>
    public partial class DlgEditProfile : Window
    {

        /// <summary>
        /// Initializes a new instance of the <see cref="DlgEditProfile"/> class.
        /// </summary>
        public DlgEditProfile(ProfileConfig profile)
        {
            InitializeComponent();
            this.DataContext = profile;
            _isNewConfig = false;
            txtProfileName.IsEnabled = false;
            txtProfileDescription.Focus();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DlgEditProfile"/> class.
        /// </summary>
        public DlgEditProfile()
        {
            InitializeComponent();
            this.DataContext = new ProfileConfig();
            _isNewConfig = true;
            txtProfileName.Focus();
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
        private void OnOk()
        {
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
        private void Window_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape) this.Close();
        }

        /// <summary>
        /// Handles the Click event of the btnOk control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Windows.RoutedEventArgs"/> instance containing the event data.</param>
        private void btnOk_Click(object sender, RoutedEventArgs e)
        {

            if (_isNewConfig)
            {
                foreach (string profileName in ProfileManager.ProfileNames)
                {
                    if (profileName.ToLower().Equals(txtProfileName.Text.Trim().ToLower()))
                    {
                        MessageBox.Show(this,
                            "Der angegebene Profilname existiert bereits.",
                            "Profil existiert bereits",
                            MessageBoxButton.OK,
                            MessageBoxImage.Information);

                        txtProfileName.Focus();
                        return;
                    }
                }
            }
            if (string.IsNullOrEmpty(viewboxDbName.Text))
            {
                MessageBox.Show(this, "Der Viewboxname darf nicht leer sein.", "", MessageBoxButton.OK,
                                MessageBoxImage.Error);
                return;
            }

            //"Viewbox-Datenbank" and "Datenbank" fields not be the same!
            if (viewboxDbName.Text.ToLower().Equals(databaseConfig.txtDatabase.Text.ToLower()))
            {
                MessageBox.Show(this, "'Viewbox-Datenbank' und 'Datenbank' nicht identisch sein.", "", MessageBoxButton.OK,
                MessageBoxImage.Error);

                return;
            }

            try
            {
                UpdateBindingSources();
                ViewBuilderBusiness.Manager.ProfileManager.Save(this.Profile);
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, "Fehler beim Speichern des Profils: " + ex.Message);
                return;
            }

            OnOk();
            this.Close();
        }

        /// <summary>
        /// Handles the Click event of the btnCancel control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Windows.RoutedEventArgs"/> instance containing the event data.</param>
        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        /// <summary>
        /// Handles the GotFocus event of the textbox control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Windows.RoutedEventArgs"/> instance containing the event data.</param>
        private void textbox_GotFocus(object sender, RoutedEventArgs e)
        {
            ((TextBox)sender).SelectAll();
        }

        #endregion eventHandler

        /*****************************************************************************************************/

        #region methods

        private void UpdateBindingSources()
        {
            txtProfileName.GetBindingExpression(TextBox.TextProperty).UpdateSource();
            txtProfileDescription.GetBindingExpression(TextBox.TextProperty).UpdateSource();
            cbAutoGenerateIndex.GetBindingExpression(CheckBox.IsCheckedProperty).UpdateSource();
            viewboxDbName.GetBindingExpression(TextBox.TextProperty).UpdateSource();
            databaseConfig.UpdateBindings();
        }

        #endregion methods

        /*****************************************************************************************************/

        #region properties

        public ProfileConfig Profile
        {
            get { return (ProfileConfig)this.DataContext; }
        }

        #endregion properties
    }
}
