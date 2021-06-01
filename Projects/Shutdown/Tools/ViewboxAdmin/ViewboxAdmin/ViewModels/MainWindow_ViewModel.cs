using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Windows;
using System.Windows.Threading;
using AV.Log;
using ViewboxAdmin.Factories;
using ViewboxAdmin.Manager;
using ViewboxAdmin.Models.ProfileRelated;
using ViewboxAdmin.Structures;
using ViewboxAdmin.ViewModels;
using ViewboxAdmin.ViewModels.Collections;
using ViewboxAdmin.ViewModels.MergeDataBase;
using ViewboxAdmin.ViewModels.Roles;
using ViewboxAdmin.ViewModels.Users;
using ViewboxAdminBusiness.Manager;
using ViewboxAdmin_ViewModel;
using ViewboxAdmin_ViewModel.Structures.Config;
using Autofac;
using log4net;

namespace ViewboxAdmin.Models
{
    public class MainWindow_ViewModel : NotifyBase, IMainWindow_ViewModel
    {
        internal ILog _log = LogHelper.GetLogger();

        #region Constructor
        public MainWindow_ViewModel(IProfileManager profilemanager, IProfileModelRepository profilerepo, INavigationEntriesDataContext naventries, IDispatcher dispatcher,IIoCResolver componentContext) {
            //injecting dependencies
            this.Profilemananger = profilemanager;
            this.ProfileModelRepository = profilerepo;
            this.NavigationTreeElementDataContext = naventries;
            this.Dispatcher = dispatcher;
            this.Container = componentContext;
            //member init
            this.NavigationTree = NavigationTreeElementDataContext.NavigationTree;
            this.Profiles = Profilemananger.Profiles;
            
        }
        #endregion Constructor


        #region Private methods
        

        private void SetCurrentUser() {
            CurrentUser = System.Security.Principal.WindowsIdentity.GetCurrent().Name;
            if (CurrentUser.LastIndexOf('\\') != -1)
            {
                CurrentUser = CurrentUser.Substring(CurrentUser.LastIndexOf('\\') + 1);
            }
        }

        private IProfileModel ProfileModel { get; set; }

        private void UpdateWithProfile(IProfile profile) {
            if (profile != null)
            {
                ProfileModel = ProfileModelRepository.GetModel(profile);
                ProfileModel.FinishedProfileLoadingEvent -= FinishedProfileLoadingEvent;
                ProfileModel.FinishedProfileLoadingEvent += FinishedProfileLoadingEvent;
                ProfileModel.Error -= ProfileModelOnError;
                ProfileModel.Error += ProfileModelOnError;
                //start async loading
                ProfileModel.LoadData();
            }
        }

        private void ProfileModelOnError(object sender, ErrorEventArgs errorEventArgs) {
            Exception ex = errorEventArgs.GetException();
            if (ex != null)
            {
                _log.Warn(ex);
                OnException(ex);
            }
        }

        private void FinishedProfileLoadingEvent(object sender, ErrorEventArgs e) {
            if (e.GetException() != null)
            {
                //profile loading was unsuccessful... report exception to UI
                _log.Warn(e.GetException());
                OnException(e.GetException());
            }
            else {
                Dispatcher.Invoke(UpdateDataContext);
                ProfileModel.FinishedProfileLoadingEvent -= FinishedProfileLoadingEvent;
            }
        }
        
        #endregion Private methods

        #region Properties
        public INavigationEntriesDataContext NavigationTreeElementDataContext { get; private set; }
        public IIoCResolver Container { get; private set; }
        private IProfile _selectedProfile;
        public IProfile SelectedProfile {
            get { return _selectedProfile; }
            set {
                
                    _selectedProfile = value;
                    SelectedProfileModel = ProfileModelRepository.GetModel(value);
                    if (SelectedProfile.DatabaseOutOfDateInformation == null) {
                        UpdateWithProfile(_selectedProfile);
                    }
                OnPropertyChanged("SelectedProfile");
                OnPropertyChanged("ProfileSelectionVisibility");
                    

            }
        }

        private IProfileModel _selectedProfileModel;
        public IProfileModel SelectedProfileModel {
            get {
                return _selectedProfileModel;
            }
            set {
                if (_selectedProfileModel != value)
                {
                    _selectedProfileModel = value;
                    OnPropertyChanged("SelectedProfileModel");
                }
            }
        }

        

        #region CurrentUser
        private string _currentUser;

        public string CurrentUser {
            get { return _currentUser; }
            set {
                if (_currentUser != value) {
                    _currentUser = value;
                    OnPropertyChanged("CurrentUser");
                }
            }
        }
        #endregion CurrentUser

        public INavigationTree NavigationTree { get; set; }

        public ObservableCollection<IProfile> Profiles { get; set; }
       
        public bool ProfileSelectionVisibility { get { return Profiles != null && Profiles.Count > 0; } }

        public IProfileManager Profilemananger { get; private set; }

        public IProfileModelRepository ProfileModelRepository { get; private set; }

        public IDispatcher Dispatcher { get; private set; }
        #endregion Properties

        #region Public Methods
        public void SetProfile() {
            if (Profilemananger.GetLastProfile() != null) {
                this.SelectedProfile = Profilemananger.GetLastProfile();
            }
            SetCurrentUser();
            NavigationTreeElementDataContext["Profile"] = Container.Resolve<ProfileManagement_ViewModel>();
        }

        public void UpdateDataContext() {
            NavigationTreeElementDataContext["DeleteSystem"] = Container.Resolve<SystemDeleteViewModel>();
            NavigationTreeElementDataContext["EditText"] = Container.Resolve<EditText_ViewModel>();
            NavigationTreeElementDataContext["Merge"] = Container.Resolve<Merger_ViewModel>();
            NavigationTreeElementDataContext["Optimizations"] = Container.Resolve<OptimizationTree_ViewModel>();
            NavigationTreeElementDataContext["Parameters"] = Container.Resolve<IParameters_ViewModel>();
            NavigationTreeElementDataContext["Users"] = Container.Resolve<UsersViewModel>();
            NavigationTreeElementDataContext["Roles"] = Container.Resolve<RoleEditViewModel>();
            ////TODO: put em to the ioc container
            //var Users = new LoadUsers(SelectedProfile.SystemDb).GetUsers();
            //NavigationTreeElementDataContext["Roles"] = new RoleEditViewModel(Users, new RoleLoader(SelectedProfile.SystemDb,Users).GetRoles(),new RoleUnitOfWork(), new MessageBoxProvider());
        }
        public void UpGradeDatabase() {
            try
            {
                if (SelectedProfile != null)
                {
                    SelectedProfile.UpgradeDatabase();
                    UpdateWithProfile(SelectedProfile);
                }

            }
            catch (Exception ex)
            {
                _log.Error(ex);
                OnException(ex);
            }
        }
        public void SaveApplicationAndProfile() {
            try
            {
                string lastProfileName = String.Empty;
                if (SelectedProfile != null)
                {
                    lastProfileName = SelectedProfile.Name;
                }
                Profilemananger.SetLastProfile(lastProfileName);
                //TODO: too deep call... 
                Profilemananger.ApplicationManager.Save();
            }
            catch (Exception ex)
            {
                _log.Error(ex);
                OnException(ex);
            }
        }
        #endregion Public Methods

        #region Events
        public event EventHandler<ErrorEventArgs> ExceptionEvent;
        public void OnException(Exception e) {
            if (ExceptionEvent != null)
            {
                ExceptionEvent(this,new ErrorEventArgs(e));
            }
        }
        #endregion Events
        
    }
}
