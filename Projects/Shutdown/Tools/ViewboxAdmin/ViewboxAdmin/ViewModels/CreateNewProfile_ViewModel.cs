using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Windows.Input;
using SystemDb;
using DbAccess.Structures;
using ViewboxAdmin.Command;
using ViewboxAdmin_ViewModel.Structures.Config;

namespace ViewboxAdmin.ViewModels
{
    public class CreateNewProfile_ViewModel : Profile, IDataErrorInfo
    {
        public CreateNewProfile_ViewModel(ProfileManagement_ViewModel profileManagementViewModel, ISystemDb systemdb, IDbConfig dbconfig):base(systemdb,dbconfig) {
            this.profileManagementViewModel = profileManagementViewModel;

            SaveCommand = new RelayCommand(o=>this.SaveProfile(),a => CanSave());
            CloseCommand = new RelayCommand(o=>OnCloseWindow(),a => true);
        }

        private bool CanSave() {
            return !String.IsNullOrEmpty(Name);
        }

        public ProfileManagement_ViewModel profileManagementViewModel { get; private set; }

        public void SaveProfile() {
            profileManagementViewModel.SelectedProfile = this;
            profileManagementViewModel.ProfileManager.Save(this);
            OnCloseWindow();
        }

        public Action CloseWindow;
        private void OnCloseWindow() {
            if (CloseWindow!=null) {
                CloseWindow();
            }
        }

        public ICommand SaveCommand { get; set; }
        public ICommand CloseCommand { get; set; }

        public string Error {
            get { throw new NotImplementedException(); }
        }

        public string this[string columnName] {
            get { string result = string.Empty;
            if(columnName=="Name") {
                if (string.IsNullOrEmpty(this.Name)) {
                    result = "Please enter a Name for the profile";
                }
            }
                return result;
            }
        }
       
    }
}
