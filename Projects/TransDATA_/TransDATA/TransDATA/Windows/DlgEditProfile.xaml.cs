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
using TransDATABusiness.ConfigDb.Tables;
using TransDATABusiness.Manager;
using System.Collections.ObjectModel;

namespace TransDATA.Windows {
    /// <summary>
    /// Interaktionslogik für DlgEditProfile.xaml
    /// </summary>
    public partial class DlgEditProfile : Window {
        public DlgEditProfile(Profile profile) {
            InitializeComponent();
            this.DataContext = profile;
            this.controlConfigDatabase.Init(profile.ConfigDatabase);
            _profileNames = new ObservableCollection<string>();
            txtProfileName.ItemsSource = this.ProfileListInit();
        }

        /****************************************************************************************************/
        #region fields

        /// <summary>
        /// List of all profiles.
        /// </summary>
        private ObservableCollection<string> _profileNames;

        #endregion fields

        /***************************************************************************************************/
        #region properties

        public Profile Profile {
            get { return (Profile)this.DataContext; }
        }

        #endregion properties

        /****************************************************************************************************/

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
            if (Ok != null) Ok(this, null);
        }

        #endregion eventTrigger

        /*******************************************************************************************************/

        /// <summary>
        /// Handles the Click event of the btnExportFolder control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Windows.RoutedEventArgs"/> instance containing the event data.</param>
        private void btnExportFolder_Click(object sender, RoutedEventArgs e) {
            System.Windows.Forms.FolderBrowserDialog dlg = new System.Windows.Forms.FolderBrowserDialog();
            dlg.Description = "Profilordner auswählen";
            dlg.RootFolder = System.Environment.SpecialFolder.Desktop;
            dlg.SelectedPath = txtExportFolder.Text;
            dlg.ShowNewFolderButton = false;
            if (dlg.ShowDialog() == System.Windows.Forms.DialogResult.OK) {
                txtExportFolder.Text = dlg.SelectedPath;
            }

            dlg.Dispose();
        }

        /// <summary>
        /// Handles the Click event of the btnCancel control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Windows.RoutedEventArgs"/> instance containing the event data.</param>
        private void btnCancel_Click(object sender, RoutedEventArgs e) {
            this.Close();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e) {

        }

        /// <summary>
        /// Handles the Click event of the btnLogin control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Windows.RoutedEventArgs"/> instance containing the event data.</param>
        private void btnLogin_Click(object sender, RoutedEventArgs e) {
           
            if (this.controlConfigDatabase.LoginValidate()) {
                if (!this.ProfileNameValidate(txtProfileName.Text)) {
                    try {
                        UpdateBindingSources();
                        ProfileManager.ProfileNames.Add(txtProfileName.Text.Trim().ToLower());
                        ((Profile)this.DataContext).Save();
                    } catch (Exception ex) {
                        MessageBox.Show("Fehler beim Speichern des Profils: " + ex.Message);
                        return;
                    }
                }
                OnOk();
                this.Close();
            } 
        }

        private void btnForward_Click(object sender, RoutedEventArgs e) {
             tabControl.SelectedIndex = 1;
             btnForward.IsEnabled = false;
             btnBackward.IsEnabled = true;
        }

        private void btnBackward_Click(object sender, RoutedEventArgs e) {
            tabControl.SelectedIndex = 0;
            btnBackward.IsEnabled = false;
            btnForward.IsEnabled = true;
        }

        private void tabControl_SelectionChanged(object sender, SelectionChangedEventArgs e) {
            if (profile.IsSelected) { 
                btnForward.IsEnabled = true;
                btnBackward.IsEnabled = false;
            } else {
                btnForward.IsEnabled = false;
                btnBackward.IsEnabled = true;
            }
        }

        private void controlConfigDatabase_Loaded(object sender, RoutedEventArgs e) {

        }

        private void txtProfileName_SelectionChanged(object sender, SelectionChangedEventArgs e) {

            foreach (string profileName in ProfileManager.ProfileNames) {
                if (profileName.ToLower().Equals(txtProfileName.Text.Trim().ToLower())) {
                    //((Profile)this.DataContext).Load()
                }
            }
        }
        /****************************************************************************************/
        #region  methods

        /// <summary>
        /// Initialization method
        /// </summary>
        private ObservableCollection<string> ProfileListInit() {

            // Add all existing profileNames.

            foreach (string profileName in ProfileManager.ProfileNames) {
                this._profileNames.Add(profileName);
            }

           return this._profileNames;
        }


        private bool ProfileNameValidate(string profileName) {
            foreach (string profname in ProfileManager.ProfileNames) {
                if (profileName.ToLower().Equals(profname.Trim().ToLower())) {
                    return true;
                }  
            }       
           return false;      
        }

        private void UpdateBindingSources() {
            txtDescription.GetBindingExpression(TextBox.TextProperty).UpdateSource();
            txtProfileName.GetBindingExpression(ComboBox.TextProperty).UpdateSource();
            txtLocation.GetBindingExpression(TextBox.TextProperty).UpdateSource();
            txtExportFolder.GetBindingExpression(TextBox.TextProperty).UpdateSource();
            this.controlConfigDatabase.UpdateBindings();
        }

        #endregion methods

    }
}
