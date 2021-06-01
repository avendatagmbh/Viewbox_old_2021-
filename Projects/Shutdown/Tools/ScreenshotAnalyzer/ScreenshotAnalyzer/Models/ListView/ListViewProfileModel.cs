using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;
using ScreenshotAnalyzer.Controls.ProfileRelated;
using ScreenshotAnalyzer.Manager;
using ScreenshotAnalyzer.Resources.Localisation;
using ScreenshotAnalyzer.Windows;
using ScreenshotAnalyzerBusiness.Manager;
using ScreenshotAnalyzerBusiness.Structures.Config;

namespace ScreenshotAnalyzer.Models.ListView {
    
    internal class ListViewProfileModel : ListViewModelBase, IListViewModel, INotifyPropertyChanged {

        #region Constructor
        public ListViewProfileModel(MainWindowModel mainWindowModel) {
            MainWindowModel = mainWindowModel;
            mainWindowModel.PropertyChanged += mainWindowModel_PropertyChanged;
        }

        void mainWindowModel_PropertyChanged(object sender, PropertyChangedEventArgs e) {
            if (e.PropertyName == "SelectedProfile")
                SelectedItem = ((MainWindowModel) sender).SelectedProfile;
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
        private ObservableCollection<Profile> _items = ProfileManager.Profiles;
        #endregion  

        private CtlProfileDetails ProfileDetails { get; set; }
        private MainWindowModel MainWindowModel { get; set; }
        #region SelectedItem
        public object SelectedItem {
            get {
                return MainWindowModel.SelectedProfile; 
            }
            set {
                MainWindowModel.SelectedProfile = (Profile) value;
                //_selectedItem = value;
                ProfileDetails.DataContext = ProfileModelManager.GetModel(MainWindowModel.SelectedProfile);
                OnPropertyChanged("SelectedItem");
                OnPropertyChanged("HeaderString");

            }
        }
        //private object _selectedItem;
        #endregion

        #endregion properties

        #region methods

        #region InitUiElements
        public void InitUiElements(System.Windows.Controls.Panel dataPanel, System.Windows.Controls.Panel listPanel) {
            this.DataPanel = dataPanel;
            this.ListPanel = listPanel;
            listPanel.Children.Add(CreateListBox());

            ProfileDetails = new CtlProfileDetails(){DataContext=null};
            dataPanel.Children.Add(ProfileDetails);
        }
        #endregion

        #region HeaderString
        public string HeaderString {
            get {
                if (SelectedItem != null)
                    return ((Profile)SelectedItem).DisplayString;
                else
                    return "";
            }
        }
        #endregion

        #region AddItem
        public bool AddItem() {
            try {
                //Profile newProfile = ProfileManager.CreateNewProfile();
                DlgNewProfile dlgNewProfileDialog = new DlgNewProfile() { Owner = Owner };
                if (dlgNewProfileDialog.ShowDialog().Value) {
                    try {
                        Profile newProfile = ProfileManager.CreateNewProfile(dlgNewProfileDialog.ProfileName);
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
            if (item != null && item is Profile) {
                try {
                    if (MessageBox.Show(Owner, string.Format(ResourcesGui.ListViewProfileModel_DeleteItem_DeleteProfileQuestion, (item as Profile).Name)
                        , "", MessageBoxButton.YesNo, MessageBoxImage.Question, MessageBoxResult.No)
                        == MessageBoxResult.Yes) {
                            ProfileManager.DeleteProfile(item as Profile);
                            (Owner as DlgMainWindow).Model.DeletedProfile(item as Profile);
                    }
                } catch (Exception ex) {
                    MessageBox.Show(Owner, ResourcesGui.ListViewProfileModel_DeleteItem_Error_DeletingProfile + Environment.NewLine + ex.Message, "", MessageBoxButton.OK, MessageBoxImage.Error);
                    //throw new Exception(ex.Message);
                }
            }
        }
        #endregion

        #endregion methods
        
    }
}
