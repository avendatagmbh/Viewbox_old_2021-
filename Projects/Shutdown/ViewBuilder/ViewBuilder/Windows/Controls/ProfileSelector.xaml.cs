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
using System.Windows.Navigation;
using System.Windows.Shapes;
using Utils;
using ViewBuilderBusiness.Manager;
using ViewBuilderBusiness.Structures.Config;
using System.ComponentModel;

namespace ViewBuilder.Windows.Controls {
    /// <summary>
    /// Interaktionslogik für ProfileSelector.xaml
    /// </summary>
    public partial class ProfileSelector : UserControl, INotifyPropertyChanged {

        /// <summary>
        /// Initializes a new instance of the <see cref="ProfileSelector"/> class.
        /// </summary>
        public ProfileSelector() {
            InitializeComponent();

            this.DataContext = ApplicationManager.ApplicationConfig;
            ViewBuilderBusiness.Manager.ProfileManager.UpdateProfileNames();
            lstProfilenames.ItemsSource = ViewBuilderBusiness.Manager.ProfileManager.ProfileNames;
        }

        /*****************************************************************************************************/

        #region events

        /// <summary>
        /// Occurs when a property changed
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;
        public event EventHandler DoubleClickProfile;
        #endregion events

        /*****************************************************************************************************/

        #region eventTrigger

        /// <summary>
        /// Called when a property changed.
        /// </summary>
        /// <param name="property">The property.</param>
        private void OnPropertyChanged(string property) {
            if (PropertyChanged != null) PropertyChanged(this, new PropertyChangedEventArgs(property));
        }

        #endregion eventTrigger

        /*****************************************************************************************************/

        /// <summary>
        /// Gets or sets the index of the selected.
        /// </summary>
        /// <value>The index of the selected.</value>
        public int SelectedIndex {
            get { return lstProfilenames.SelectedIndex; }
            set { lstProfilenames.SelectedIndex = value; }
        }

        /// <summary>
        /// Gets or sets the selected item.
        /// </summary>
        /// <value>The selected item.</value>
        public object SelectedItem {
            get { return lstProfilenames.SelectedItem; }
            set { lstProfilenames.SelectedItem = lstProfilenames; }
        }

        /// <summary>
        /// Handles the Click event of the btnDeleteProfile control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Windows.RoutedEventArgs"/> instance containing the event data.</param>
        private void btnDeleteProfile_Click(object sender, RoutedEventArgs e) {
            DeleteProfile();
        }

        /// <summary>
        /// Handles the Click event of the btnAddProfile control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Windows.RoutedEventArgs"/> instance containing the event data.</param>
        private void btnAddProfile_Click(object sender, RoutedEventArgs e) {
            AddProfile();
        }

        /// <summary>
        /// Handles the Ok event of the DlgEditProfile control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        void DlgEditProfile_Ok(object sender, EventArgs e) {
            try {
                ProfileManager.Save(((DlgEditProfile)sender).Profile);
                lstProfilenames.SelectedItem = ((DlgEditProfile)sender).Profile.Name;

                if (lstProfilenames.SelectedIndex >= 0) {
                    ProfileConfig profile = ProfileManager.Open(lstProfilenames.SelectedItem.ToString());
                    txtProfileDescription.Text = profile.Description;
                } else {
                    txtProfileDescription.Clear();
                }
            } catch (Exception ex) {
                MessageBox.Show(UIHelpers.TryFindParent<Window>(this), ex.Message, "Fehler", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// Handles the SelectionChanged event of the lstProfilenames control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Windows.Controls.SelectionChangedEventArgs"/> instance containing the event data.</param>
        private void lstProfilenames_SelectionChanged(object sender, SelectionChangedEventArgs e) {
            if (lstProfilenames.SelectedIndex >= 0) {
                ProfileConfig profile = ProfileManager.Open(lstProfilenames.SelectedItem.ToString());
                txtProfileDescription.Text = profile.Description;
            } else {
                txtProfileDescription.Clear();
            }

            OnPropertyChanged("SelectedIndex");
            OnPropertyChanged("SelectedItem");
        }

        /// <summary>
        /// Handles the Click event of the btnEditProfile control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Windows.RoutedEventArgs"/> instance containing the event data.</param>
        private void btnEditProfile_Click(object sender, RoutedEventArgs e) {
            ProfileConfig profile = ProfileManager.Open(lstProfilenames.SelectedItem.ToString());
            DlgEditProfile DlgEditProfile = new DlgEditProfile(profile) { Owner = UIHelpers.TryFindParent<Window>(this) };
            DlgEditProfile.Ok += new EventHandler(DlgEditProfile_Ok);
            DlgEditProfile.ShowDialog();
        }

        /// <summary>
        /// Handles the Loaded event of the UserControl control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Windows.RoutedEventArgs"/> instance containing the event data.</param>
        private void UserControl_Loaded(object sender, RoutedEventArgs e) {
            lstProfilenames.Focus();
        }

        #region methods

        /// <summary>
        /// Opens the new profile dialog.
        /// </summary>
        private void AddProfile() {
            DlgEditProfile DlgEditProfile = new DlgEditProfile() { Owner = UIHelpers.TryFindParent<Window>(this)};
            DlgEditProfile.Ok += new EventHandler(DlgEditProfile_Ok);
            DlgEditProfile.ShowDialog();
        }

        /// <summary>
        /// Deletes the profile.
        /// </summary>
        private void DeleteProfile() {
            try {
                if (MessageBox.Show(UIHelpers.TryFindParent<Window>(this),
                    "Profil \"" + lstProfilenames.SelectedItem.ToString() + "\" löschen?" + Environment.NewLine + "Achtung, diese Aktion kann nicht rückgängig gemacht werden!",
                    "Profil löschen",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question,
                    MessageBoxResult.No) == MessageBoxResult.Yes) {

                    int index = lstProfilenames.SelectedIndex;

                    ViewBuilderBusiness.Manager.ProfileManager.DeleteProfile(lstProfilenames.SelectedItem.ToString());
                    if (index < lstProfilenames.Items.Count) lstProfilenames.SelectedIndex = index;
                    else if (index > 0) lstProfilenames.SelectedIndex = index - 1;
                }

            } catch (Exception ex) {
                MessageBox.Show(UIHelpers.TryFindParent<Window>(this), ex.Message, "Fehler", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        #endregion methods

        private void lstProfilenames_MouseDoubleClick(object sender, MouseButtonEventArgs e) {
            UIElement elem = (UIElement)lstProfilenames.InputHitTest(e.GetPosition(lstProfilenames));
            while (elem != lstProfilenames) {
                if (elem is ListBoxItem) {
                    string selectedItem = (string)((ListBoxItem)elem).Content;
                    int index = ProfileManager.ProfileNames.IndexOf(selectedItem);
                    // Handle the double click here
                    if (DoubleClickProfile != null)
                        DoubleClickProfile(this, new EventArgs());
                    return;
                }
                elem = (UIElement)VisualTreeHelper.GetParent(elem);
            }
        }

    }
}
