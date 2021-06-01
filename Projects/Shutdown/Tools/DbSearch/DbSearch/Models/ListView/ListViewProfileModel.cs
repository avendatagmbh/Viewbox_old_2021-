using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;
using DbSearch.Controls.Profile;
using DbSearch.Localisation;
using DbSearch.Manager;
using DbSearch.Models.Profile;
using DbSearch.Windows;
using DbSearch.Windows.Profile;
using DbSearchBase.Interfaces;
using DbSearchLogic.Manager;

namespace DbSearch.Models.ListView {
    
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
        private ObservableCollection<DbSearchLogic.Config.Profile> _items = ProfileManager.Profiles;
        #endregion  

        private CtlProfileDetails ProfileDetails { get; set; }
        
        #region SelectedItem
        public object SelectedItem {
            get { return _selectedItem; }
            set {
                _selectedItem = value;
                ProfileDetails.DataContext = ProfileModelManager.GetModel(_selectedItem as DbSearchLogic.Config.Profile);
                (Owner as DlgMainWindow).Model.SelectedProfile = _selectedItem as DbSearchLogic.Config.Profile;
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

            ProfileDetails = new CtlProfileDetails();
            dataPanel.Children.Add(ProfileDetails);

            //ProfileDetails.LostFocus += new RoutedEventHandler(ProfileDetails_LostFocus);
        }

        //void ProfileDetails_LostFocus(object sender, RoutedEventArgs e) {
        //    (ProfileDetails.DataContext as ProfileModel).SaveProfile();
        //}
        #endregion

        #region HeaderString
        public string HeaderString {
            get {
                if (SelectedItem != null)
                    return ((DbSearchLogic.Config.Profile)SelectedItem).DisplayString;
                else
                    return "";
            }
        }
        #endregion

        #region AddItem
        public bool AddItem() {
            try {
                DbSearchLogic.Config.Profile newProfile = ProfileManager.CreateNewProfile();
                DlgNewProfile dlgNewProfileDialog = new DlgNewProfile(newProfile) { Owner = Owner };
                if (dlgNewProfileDialog.ShowDialog().Value) {
                    try {
                        ProfileManager.AddProfile(newProfile);
                        SelectedItem = newProfile;
                        (Owner as DlgMainWindow).Model.AddedProfile(newProfile);
                    } catch (InvalidOperationException ex) {
                        MessageBox.Show(Owner, ResourcesGui.Error + Environment.NewLine + ex.Message, "", MessageBoxButton.OK,
                                        MessageBoxImage.Error);
                    }
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
            if (item != null && item is DbSearchLogic.Config.Profile) {
                try {
                    if (MessageBox.Show(Owner, "Möchten Sie das Profil " + (item as DbSearchLogic.Config.Profile).Name +
                        " wirklich löschen? Beachten Sie bitte, dass dabei die Daten nicht verloren gehen, Sie können das Profil jederzeit wiederherstellen."
                        , "", MessageBoxButton.YesNo, MessageBoxImage.Question, MessageBoxResult.No)
                        == MessageBoxResult.Yes) {
                            ProfileManager.DeleteProfile(item as DbSearchLogic.Config.Profile);
                            (Owner as DlgMainWindow).Model.DeletedProfile(item as DbSearchLogic.Config.Profile);
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
