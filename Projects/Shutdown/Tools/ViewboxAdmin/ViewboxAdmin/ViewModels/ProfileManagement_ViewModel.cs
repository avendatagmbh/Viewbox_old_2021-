using System;
using System.Windows.Input;
using DbAccess.Structures;
using ViewboxAdmin.Command;
using ViewboxAdmin.Models;
using ViewboxAdmin.Structures;
using ViewboxAdminBusiness.Manager;
using ViewboxAdmin_ViewModel;
using ViewboxAdmin_ViewModel.Structures.Config;


namespace ViewboxAdmin.ViewModels
{
    public class ProfileManagement_ViewModel : NotifyBase
    {
        #region Constructor
        public ProfileManagement_ViewModel(IProfileManager profileManager, IMainWindow_ViewModel mainviewmodel) {
            //inject dependencies
            this.ProfileManager = profileManager;
            this.MainWindowViewModel = mainviewmodel;
            // init commands
            DeleteProfileCommand = new RelayCommand(o => OnDeleteItem(DeleteProfile,()=> { }), o => IsDeleteEnabled() );
            CreateNewProfileCommand = new RelayCommand(o=> OnAddNewProfile());
        }
        #endregion Constructor

        #region Properties
        public IMainWindow_ViewModel MainWindowViewModel { get; private set; }

        private IProfileManager _profileManager;
        public IProfileManager ProfileManager {
            get { return _profileManager; }
            set {
                if (_profileManager != value) {
                    _profileManager = value;
                    OnPropertyChanged("ProfileManager");
                }
            }
        }

        
        
        private IProfile _selectedProfile;
        public IProfile SelectedProfile {
            get { return _selectedProfile; }
            set {
                    _selectedProfile = value;
                //updating mainwindow
                    MainWindowViewModel.SelectedProfile = value;
                   
                    OnPropertyChanged("SelectedProfile");
                
            }
        }

        public ICommand DeleteProfileCommand { get; set; }
        public ICommand CreateNewProfileCommand { get; set; }

        #endregion Properties

        #region Private methods
        private void DeleteProfile() {
            ProfileManager.DeleteProfile(SelectedProfile);
        }

        private bool IsDeleteEnabled() {
            return SelectedProfile != null;
        }
        #endregion Private methods

        #region Events
        /// <summary>
        /// reporting delete request to UI
        /// </summary>
        public event EventHandler<MessageBoxActions> DeleteItem;
        private void OnDeleteItem(Action onYes, Action onNo) {
            if (DeleteItem != null)
            {
                DeleteItem(this, new MessageBoxActions(onYes, onNo));
            }
        }

        /// <summary>
        /// reporting new profile dialog request
        /// </summary>
        public event EventHandler<NewProfileWindowEventArg> AddNewProfile;
        private void OnAddNewProfile() {
            if (AddNewProfile != null)
            {
                AddNewProfile(this, new NewProfileWindowEventArg(GetCreateNewProfileViewModel()));
            }
        }

        private CreateNewProfile_ViewModel GetCreateNewProfileViewModel() { return new CreateNewProfile_ViewModel(this, new SystemDb.SystemDb(), new DbConfig("MySQL")); }

        #endregion Events


    }
}
