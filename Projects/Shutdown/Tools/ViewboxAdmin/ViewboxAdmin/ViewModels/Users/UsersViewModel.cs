using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Input;
using AV.Log;
using ViewboxAdmin.Command;
using ViewboxAdmin.CustomEventArgs;
using ViewboxAdmin.Structures;
using ViewboxAdmin_ViewModel;
using log4net;

namespace ViewboxAdmin.ViewModels.Users
{
    class UsersViewModel : NotifyBase
    {
        #region Constructor
        public UsersViewModel(ObservableCollection<UserModel> users, IUnitOfWork<UserModel> unitOfWork ) {
            this.Users = users;
            this.UnitOfWork = unitOfWork;
            DeleteUserCommand = new RelayCommand(o => OnVerifyUserDelete(DeleteUser,DoNothing), IsCommandEnabled);
            EditUserCommand = new RelayCommand(o => OnUserEdit(AddEditedUser,DoNothing), IsCommandEnabled);
            NewUserCommand = new RelayCommand(o => OnUserCreate(AddNewUser,DoNothing));
            CommitUserCommand = new RelayCommand(o => CommitChanges());
        }
        #endregion Constructor

        public ObservableCollection<UserModel> Users { get; private set; }
        public IUnitOfWork<UserModel> UnitOfWork { get; private set; } 

        public ICommand DeleteUserCommand { get; private set;}
        public ICommand EditUserCommand { get; private set; }
        public ICommand NewUserCommand { get; private set; }
        public ICommand CommitUserCommand { get; private set; }

        #region SelectedUser
        private UserModel _selectedUser;
        public UserModel SelectedUser {
            get { return _selectedUser; }
            set {
                if (_selectedUser != value)
                {
                    _selectedUser = value;
                    OnPropertyChanged("SelectedUser");
                }
            }
        }
        #endregion SelectedUser

        public event EventHandler<NewUserEventArg> CreateOrEditUser;
        public event EventHandler<MessageBoxActions> VerifyUserDelete;
        public event EventHandler<ErrorEventArgs> ExceptionOccured;
        public event EventHandler<MessageBoxActions> UniqueKeyWarning; 

        private void OnException(Exception e) {
            if(ExceptionOccured!=null) {
                ExceptionOccured(this, new ErrorEventArgs(e));
            }
        }
        private void OnUserEdit(Action onYes, Action onNo) {
            if(CreateOrEditUser!=null) {
                this.UserToEdit = SelectedUser;
                CreateOrEditUser(this,new NewUserEventArg(UserToEdit,onYes,onNo));
            }
        }
        private void OnUserCreate(Action onYes, Action onNo) {
            if (CreateOrEditUser != null)
            {
                this.UserToEdit = new UserModel();
                CreateOrEditUser(this, new NewUserEventArg(UserToEdit, onYes, onNo));
            }
        }
        private void OnVerifyUserDelete(Action onYes, Action onNo) {
            if(VerifyUserDelete!=null) {
                VerifyUserDelete(this, new MessageBoxActions(onYes,onNo));
            }
        }
        private void OnUniqueKeyWarning() {
            if(UniqueKeyWarning!=null)
                UniqueKeyWarning(this, new MessageBoxActions(() => { ; }, () => { ; }));
        }


        private void AddEditedUser() {
            UnitOfWork.MarkAsDirty(UserToEdit);
        }
        private void AddNewUser() {
            if (!IsValidUser(UserToEdit)) {
                OnUniqueKeyWarning();
                return;
            }
            UnitOfWork.MarkAsNew(UserToEdit);
            Users.Add(UserToEdit);
            SelectedUser = UserToEdit;
        }
        private void DeleteUser() {
            UnitOfWork.MarkAsDeleted(SelectedUser);
            this.Users.Remove(SelectedUser);
        }

        private void CommitChanges() {
            try
            {
                UnitOfWork.Commit();
            }
            catch (Exception ex)
            {
                _log.Error("error during committing", ex);
                OnException(ex);
            }
        }
        private ILog _log = LogHelper.GetLogger();
        private bool IsCommandEnabled(object o) { return SelectedUser != null; }
        private UserModel UserToEdit { get; set; }
        private void DoNothing() { ; }
        
        private bool IsValidUser(UserModel user) {
            bool IsUnique =  !Users.Any(userModel => userModel.UserName == user.UserName);
            bool IsNonEmpty = user.UserName != null;
            return IsUnique && IsNonEmpty;
        }
    }
}
