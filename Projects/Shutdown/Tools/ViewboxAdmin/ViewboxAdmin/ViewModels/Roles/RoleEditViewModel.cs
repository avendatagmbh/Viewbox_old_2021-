using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Input;
using AV.Log;
using ViewboxAdmin.Command;
using ViewboxAdmin.ViewModels.Roles;
using ViewboxAdmin.ViewModels.Users;
using ViewboxAdmin_ViewModel;
using log4net;

namespace ViewboxAdmin.ViewModels
{
    class RoleEditViewModel: NotifyBase
    {
        

        public RoleEditViewModel(ObservableCollection<UserModel> users, ObservableCollection<RoleModel> roles, IUnitOfWork<RoleModel> unitofwork, IMessageBoxProvider messageBoxProvider) {
            this.Users = users;
            this.Roles = roles;
            this.UnitOfWork = unitofwork;
            this.MessageBoxProvider = messageBoxProvider;

            DeleteCommand = new RelayCommand(o => DeleteRoleModel(),o => IsButtonEnabled());
            EditCommand = new RelayCommand(o => EditRole(), o => IsButtonEnabled());
            NewCommand = new RelayCommand(o=> NewRole(), o => true);
            CommitCommand = new RelayCommand(o=>Commit());
        }

        public ObservableCollection<UserModel> Users { get; private set; }
        public ObservableCollection<RoleModel> Roles { get; private set; }
        public IUnitOfWork<RoleModel> UnitOfWork { get; private set; }
        public IMessageBoxProvider MessageBoxProvider { get; private set; }

        public ICommand DeleteCommand { get; private set; }
        public ICommand NewCommand { get; private set; }
        public ICommand CommitCommand { get; private set; }
        public ICommand EditCommand { get; private set; }

        public delegate void editrole(Action yes,Action no, RoleModel role);
        public event editrole EditOrCreate;

        private RoleModel RoleUnderEdit { get; set;} 

        private void EditRole() {
            RoleUnderEdit = new RoleModel();
            RoleUnderEdit.Name = SelectedRole.Name;
            RoleUnderEdit.Flags = SelectedRole.Flags;
            OnEditOrDelete(EditFinished,()=>{}, RoleUnderEdit);
        }

        private void NewRole() {
            RoleUnderEdit = new RoleModel();
            OnEditOrDelete(CreateFinished,()=> { },RoleUnderEdit);
        }

        private void CreateFinished() {
            Roles.Add(RoleUnderEdit);
            UnitOfWork.MarkAsNew(RoleUnderEdit);
            _log.Info("new item");
            SelectedRole = RoleUnderEdit;
        }

        private void EditFinished() {
            UnitOfWork.MarkAsDirty(SelectedRole);
            _log.Info("Item marked as dirty");
            SelectedRole.Name = RoleUnderEdit.Name;
            SelectedRole.Flags = RoleUnderEdit.Flags;
        }

        public void MarkAsDirty() {
            UnitOfWork.MarkAsDirty(SelectedRole);
        }

        private void OnEditOrDelete(Action yes, Action no, RoleModel role) {
            if(EditOrCreate!=null) {
                EditOrCreate(yes, no, role);
            }
        }

        

        #region SelectedRole
        private RoleModel _selectedRole;

        public RoleModel SelectedRole {
            get { return _selectedRole; }
            set {
                if (_selectedRole != value) {
                    _selectedRole = value;
                    OnPropertyChanged("SelectedRole");
                }
            }
        }
        #endregion SelectedRole

        private void DeleteRoleModel() {
            MessageBoxResult result = MessageBoxProvider.Show("Do u really want to remove the selected role? ",
                                                              "Delete role", MessageBoxButton.YesNo,
                                                              MessageBoxImage.Question, MessageBoxResult.No);
            if(result == MessageBoxResult.Yes) {
                UnitOfWork.MarkAsDeleted(SelectedRole);
                Roles.Remove(SelectedRole);
            }
        }

        private void Commit() {
            try {
                UnitOfWork.Commit();
                _log.Info("Successful committing role related changes to db");
            } catch (Exception ex) {
                string message = "error during committing, you probably ruined the database referencial integrity :D " + ex.Message;

                MessageBoxProvider.Show(message, "error during committing", MessageBoxButton.OK, MessageBoxImage.Error,
                                        MessageBoxResult.OK);
                _log.Error(message,ex);
            }
            
        }

        private ILog _log = LogHelper.GetLogger();
        
        private bool IsButtonEnabled() { return SelectedRole != null; }
        
    }
}
