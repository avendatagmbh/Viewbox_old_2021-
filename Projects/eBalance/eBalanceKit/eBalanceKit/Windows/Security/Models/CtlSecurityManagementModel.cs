// --------------------------------------------------------------------------------
// author: Mirko Dibbert
// since:  2011-08-05
// copyright 2011 AvenDATA GmbH
// --------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using eBalanceKitBusiness.Manager;
using eBalanceKitBusiness.Rights;
using eBalanceKitBusiness.Structures.DbMapping;
using eBalanceKitBusiness.Structures.DbMapping.Rights;

namespace eBalanceKit.Windows.Security.Models {
    /// <summary>
    /// Model for the CtlSecurityManagement control.
    /// </summary>
    internal class CtlSecurityManagementModel : INotifyPropertyChanged
    {

        #region Constructor

        public CtlSecurityManagementModel() {
            if (CurrentUser != null) {
                RoleManager.Roles.CollectionChanged -= CurrentUser.OnEditableUsersCollectionChanged;
                RoleManager.Roles.CollectionChanged -= CurrentUser.OnRolesCollectionChanged;
                RoleManager.Roles.CollectionChanged += CurrentUser.OnEditableUsersCollectionChanged;
                RoleManager.Roles.CollectionChanged += CurrentUser.OnRolesCollectionChanged;
            }
        }

        #endregion Constructor

        #region events
        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged(string propertyName) { if (PropertyChanged != null) PropertyChanged(this, new PropertyChangedEventArgs(propertyName)); }
        #endregion

        #region user

        #region SelectedUser
        public User SelectedUser {
            get {
                    if (UserListSelectedItem == null || !(UserListSelectedItem is User)) return null;
                    return UserListSelectedItem as User;
                }
        }
        #endregion SelectedUser

        #region NewUser
        private User _newUser;

        public User NewUser {
            get { return _newUser; }
            set {
                _newUser = value;

                OnPropertyChanged("NewUser");
                OnPropertyChanged("EditedUser");
            }
        }
        #endregion NewUser

        //--------------------------------------------------------------------------------

        public User EditedUser { get { return NewUser ?? SelectedUser; } }

        public void CreateUser() {
            //Set LastLogin to current date such that not the changelog window is shown on first login
            NewUser = new User(){LastLogin = DateTime.Now};
        }

        public void SaveEditedUser() {
            if (NewUser != null) {
                UserManager.Instance.AddUser(NewUser);
                UserListSelectedItem = NewUser;
                NewUser = null;
            } else {
                UserManager.Instance.UpdateUser(EditedUser);
            }
        }

        public void CancelUserEdit() { NewUser = null; }

        internal void DeleteUser(User user) {
            if (user != null) {
                var rolesToDelete = SelectedUser.AssignedRoles.ToList();
                foreach(var role in rolesToDelete)
                    RightManager.RemoveAssignedRole(user, role);                
                UserManager.Instance.DeleteUser(user);
            }
        }

        //--------------------------------------------------------------------------------
        #endregion user

        #region role

        #region SelectedRole
        public Role SelectedRole {
            get {
                if (RoleListSelectedItem == null || !(RoleListSelectedItem is Role)) return null;
                return RoleListSelectedItem as Role;
            }
        }
        #endregion SelectedRole

        #region NewRole
        private Role _newRole;

        public Role NewRole {
            get { return _newRole; }
            set {
                _newRole = value;
                OnPropertyChanged("NewRole");
                OnPropertyChanged("EditedRole");
            }
        }
        #endregion NewRole

        //--------------------------------------------------------------------------------

        public Role EditedRole { get { return NewRole ?? SelectedRole; } }

        public void CreateRole() { NewRole = new Role(new DbRole()); }

        public void SaveEditedRole() {
            if (NewRole != null) {
                RoleManager.AddRole(NewRole);
                RoleListSelectedItem = NewRole;
                NewRole = null;
            } else {
                RoleManager.UpdateRole(EditedRole);
            }
        }

        public void CancelRoleEdit() { NewRole = null; }

        internal void DeleteRole(Role role) {
            if (role != null) {
                var tmp = new List<User>(role.AssignedUsers);
                foreach (var user in tmp) RightManager.RemoveAssignedRole(user, role);
                RoleManager.DeleteRole(role);
            }
        }

        //--------------------------------------------------------------------------------
        #endregion user

        public User CurrentUser { get { return UserManager.Instance.CurrentUser; } }

        #region user list
        //--------------------------------------------------------------------------------
        private object _userListSelectedItem;

        public object UserListSelectedItem {
            get { return _userListSelectedItem; }
            set {
                _userListSelectedItem = value;
                OnPropertyChanged("UserListSelectedItem");
                OnPropertyChanged("SelectedUser");
            }
        }

        //--------------------------------------------------------------------------------
        #endregion user list

        #region role list
        //--------------------------------------------------------------------------------
        private object _roleListSelectedItem;

        public object RoleListSelectedItem {
            get { return _roleListSelectedItem; }
            set {
                _roleListSelectedItem = value;
                OnPropertyChanged("RoleListSelectedItem");
                OnPropertyChanged("SelectedRole");
            }
        }

        public bool HasAdminRights { get { return UserManager.Instance.CurrentUser.IsAdmin; } }

        public bool AllowUserManagement { get { return UserManager.Instance.CurrentUser.IsAdmin || (UserManager.Instance.CurrentUser.IsCompanyAdmin && UserManager.Instance.CurrentUser.AssignedCompanies.Count > 0); } }

        public bool IsCurrentUser { get { return UserManager.Instance.CurrentUser == EditedUser; } }

        //--------------------------------------------------------------------------------
        #endregion role list

        #region Company list

        public ObservableCollection<Company> Companies { get { return CompanyManager.Instance.AllowedCompanies; } }

        #endregion Company list
    }
}