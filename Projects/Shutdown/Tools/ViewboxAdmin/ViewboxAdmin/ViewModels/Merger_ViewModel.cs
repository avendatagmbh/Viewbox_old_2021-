using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using SystemDb;
using Utils;
using ViewboxAdmin.Command;
using ViewboxAdmin.Manager;
using ViewboxAdmin.Models.ProfileRelated;
using ViewboxAdmin.ViewModels.MergeDataBase;
using ViewboxAdminBusiness.Manager;
using ViewboxAdmin_ViewModel;
using ViewboxAdmin_ViewModel.Structures.Config;
using ErrorEventArgs = System.IO.ErrorEventArgs;

namespace ViewboxAdmin.ViewModels
{
    public class Merger_ViewModel : ViewModelBaseWithDebug
    {
        public Merger_ViewModel(IProfileModelRepository profilerepo, IProfileManager profmanager, IMergeDataBase mergerstrategy):base(mergerstrategy) {
            this.ProfileModelRepository = profilerepo;
            this.ProfileManager = profmanager;
            this.MergeDataBaseStrategy = mergerstrategy;
            MergeCommand = new RelayCommand(o => MergeDataBases());
            ClearDebugWindowCommand = new RelayCommand(o => this.DebugText = string.Empty);
        }

        public ICommand ClearDebugWindowCommand { get; set; }

        public  ICommand MergeCommand { get; set; }

        public void MergeDataBases() {
            if (IsEnabled()) {
                Task.Factory.StartNew(() => MergeDataBaseStrategy.MergeDataBases(SystemDb1, SystemDb2));
            } 
            else
            {
                AddDebugInfo("Databases are not loaded yet");
            }
        }

        private bool IsEnabled() {
            return SystemDb1 != null && SystemDb2 != null;
        }
        public IProfileModelRepository ProfileModelRepository { get; private set; }
        public IProfileManager ProfileManager { get; private set; }
        public IMergeDataBase MergeDataBaseStrategy { get; private set; }

        private IProfile _firstprofile;
        public IProfile FirstProfile { 
            get { return _firstprofile; }
            set {
                if (_firstprofile != value) {
                    _firstprofile = value;
                    OnPropertyChanged("FirstProfile");
                    ProfileModel1 = ProfileModelRepository.GetModel(value);
                    ProfileModel1.FinishedProfileLoadingEvent -=profmodel_FinishedProfileLoadingEvent1;
                    ProfileModel1.FinishedProfileLoadingEvent +=profmodel_FinishedProfileLoadingEvent1;
                    ProfileModel1.LoadData();
                }
            }
        }

        private IProfile _secondprofile;
        public IProfile SecondProfile {
            get { return _secondprofile; }
            set {
                if (_secondprofile != value)
                {
                    _secondprofile = value;
                    OnPropertyChanged("SecondProfile");
                    ProfileModel2 = ProfileModelRepository.GetModel(value);
                    ProfileModel2.FinishedProfileLoadingEvent -= profmodel_FinishedProfileLoadingEvent2;
                    ProfileModel2.FinishedProfileLoadingEvent += profmodel_FinishedProfileLoadingEvent2;
                    ProfileModel2.LoadData();
                }
            }
        }
        
        private IProfileModel _profilemodel1;
        public IProfileModel ProfileModel1 {
            get { return _profilemodel1; }
            set {
                if (_profilemodel1 != value) {
                    _profilemodel1 = value;
                    OnPropertyChanged("ProfileModel1");
                }
            }
        }

       
        private IProfileModel _profilemodel2;
        public IProfileModel ProfileModel2 {
            get { return _profilemodel2; }
            set {
                if (_profilemodel2 != value) {
                    _profilemodel2 = value;
                    OnPropertyChanged("ProfileModel2");
                }
            }
        }

        public ISystemDb SystemDb1 { get; private set; }

        public ISystemDb SystemDb2 { get; private set; }

        private void profmodel_FinishedProfileLoadingEvent1(object sender, ErrorEventArgs e) {
            if(e.GetException()!=null) {
                AddDebugInfo("The following error occured during loading the 1st profile: ");
                AddDebugInfo(e.GetException().Message);
                SystemDb1 = null;
            }
            else {
                SystemDb1 = ProfileModel1.Profile.SystemDb;
                AddDebugInfo("The 1st systemdb loaded correctly");
                ProfileModel1.FinishedProfileLoadingEvent -= profmodel_FinishedProfileLoadingEvent1;
            }
            
        }

        private void profmodel_FinishedProfileLoadingEvent2(object sender, ErrorEventArgs e) {
            if (e.GetException() != null)
            {
                AddDebugInfo("The following error occured during loading the 2nd profile: ");
                AddDebugInfo(e.GetException().Message);
                SystemDb1 = null;
            }
            else
            {
                SystemDb2 = ProfileModel2.Profile.SystemDb;
                AddDebugInfo("The 2nd systemdb loaded correctly");
                ProfileModel2.FinishedProfileLoadingEvent -= profmodel_FinishedProfileLoadingEvent2;
            }
        }

       



        


    }
}
