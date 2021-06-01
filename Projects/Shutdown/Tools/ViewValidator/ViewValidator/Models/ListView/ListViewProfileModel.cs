using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;
using ViewValidator.Controls.Profile;
using ViewValidator.Manager;
using ViewValidator.Models.Profile;
using ViewValidator.Windows;
using ViewValidatorLogic.Config;
using ViewValidatorLogic.Manager;

namespace ViewValidator.Models.ListView {
    
    internal class ListViewProfileModel : ListViewModelBase, IListViewModel, INotifyPropertyChanged {

        #region Constructor
        public ListViewProfileModel() {
        }

        #endregion

        #region events
        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged(string property) { if (PropertyChanged != null) PropertyChanged(this, new PropertyChangedEventArgs(property)); }
        #endregion

        #region properties
        #region Items
        public object Items {
            get { return _items; }
        }
        private ObservableCollection<ProfileConfig> _items = ProfileManager.Profiles;
        #endregion  

        private ProfileDetails ProfileDetails { get; set; }
        
        #region SelectedItem
        public object SelectedItem {
            get { return _selectedItem; }
            set {
                _selectedItem = value;
                ProfileDetails.DataContext = ProfileModelManager.GetModel(_selectedItem as ProfileConfig);
                (Owner as DlgMainWindow).Model.SelectedProfile = _selectedItem as ProfileConfig;
                OnPropertyChanged("SelectedItem");
                OnPropertyChanged("HeaderString");

            }
        }
        private object _selectedItem;
        #endregion

        #endregion properties

        #region methods

        #region InitUiElements
        public void InitUiElements(System.Windows.Controls.Panel dataPanel, System.Windows.Controls.Panel listPanel) {
            this.DataPanel = dataPanel;
            this.ListPanel = listPanel;
            listPanel.Children.Add(CreateListBox());

            ProfileDetails = new ProfileDetails();
            dataPanel.Children.Add(ProfileDetails);

            ProfileDetails.LostFocus += new RoutedEventHandler(ProfileDetails_LostFocus);
        }

        void ProfileDetails_LostFocus(object sender, RoutedEventArgs e) {
            (ProfileDetails.DataContext as ProfileModel).SaveProfile();
        }
        #endregion

        #region HeaderString
        public string HeaderString {
            get {
                if (SelectedItem != null)
                    return ((ProfileConfig)SelectedItem).DisplayString;
                else
                    return "";
            }
        }
        #endregion

        #region AddItem
        public bool AddItem() {
            try {
                ProfileConfig newProfile = new ProfileConfig();
                DlgNewProfile dlgNewProfileDialog = new DlgNewProfile(newProfile) { Owner = Owner };
                if (dlgNewProfileDialog.ShowDialog().Value) {
                    ProfileManager.AddProfile(newProfile);
                    SelectedItem = newProfile;
                    (Owner as DlgMainWindow).Model.SelectedProfile = newProfile;
                    return true;
                }
            } catch (Exception ex) {
                throw new Exception(ex.Message);
            }
            return false;
        }
        #endregion

        #region DeleteSelectedItem
        public void DeleteSelectedItem() {
            DeleteItem(SelectedItem);
        }
        #endregion

        #region DeleteItem
        public void DeleteItem(object item) {
            if (item != null && item is ProfileConfig) {
                try {
                    if (MessageBox.Show(Owner, "Möchten Sie das Profile " + (item as ProfileConfig).Name +
                        " wirklich löschen? Diese Aktion kann nicht rückgängig gemacht werden.", "", MessageBoxButton.YesNo, MessageBoxImage.Question, MessageBoxResult.No)
                        == MessageBoxResult.Yes) {
                        ProfileManager.DeleteProfile(item as ProfileConfig);
                        (Owner as DlgMainWindow).Model.DeletedProfile(item as ProfileConfig);
                    }
                } catch (Exception ex) {
                    throw new Exception(ex.Message);
                }
            }
        }
        #endregion

        #endregion methods
        
    }
}
