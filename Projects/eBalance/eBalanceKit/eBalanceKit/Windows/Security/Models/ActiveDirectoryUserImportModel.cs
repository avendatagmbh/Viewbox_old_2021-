// --------------------------------------------------------------------------------
// author: Solueman Hussain / Mirko Dibbert
// since:  2011-11-05
// copyright 2011 AvenDATA GmbH
// --------------------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Threading;
using Utils;
using eBalanceKitBase.Windows;
using eBalanceKitBusiness.Manager;
using eBalanceKitBusiness.Structures;
using eBalanceKitBusiness.Structures.DbMapping;
using eBalanceKitResources.Localisation;

namespace eBalanceKit.Windows.Security.Models {

    public enum SortDirection { Name, UserName }

    internal class ActiveDirectoryUserImportModel {

        #region ctor
        public ActiveDirectoryUserImportModel(Window owner) {
            Owner = owner;
            LoadUsers();
        }
        #endregion ctor

        #region Owner
        private Window Owner { get; set; }
        #endregion Owner
        
        #region Users
        private readonly ObservableCollection<UserInfoActiveDirectory> _users =
            new ObservableCollection<UserInfoActiveDirectory>();

        public IEnumerable<UserInfoActiveDirectory> Users { get { return _users; } }
        #endregion Users

        #region FilteredUsers
        private readonly ObservableCollectionAsync<UserInfoActiveDirectory> _filteredUsers =
            new ObservableCollectionAsync<UserInfoActiveDirectory>();

        public IEnumerable<UserInfoActiveDirectory> FilteredUsers { get { return _filteredUsers; } }
        #endregion FilteredUsers

        #region Filter
        private string _filter;

        public string Filter {
            get { return _filter; }
            set {
                _filter = value;
                ApplyFilter();
            }
        }
        #endregion Filter

        #region SortDirection
        private SortDirection _sortDirection;

        public SortDirection SortDirection {
            get { return _sortDirection; }
            set {
                _sortDirection = value;
                ApplyFilter();
            }
        }
        #endregion SortDirection
        
        #region ShowChecked
        private bool _showChecked;

        public bool ShowChecked {
            get { return _showChecked; }
            set {
                _showChecked = value;
                ApplyFilter();
            }
        }
        #endregion ShowChecked

        #region ApplyFilter
        private void ApplyFilter() {
            _filteredUsers.Clear();
            var filter = (Filter == null ? null : Filter.ToLower());
            foreach (var user in
                from user in _users
                where
                    (filter == null || (user.Name.ToLower().Contains(filter) || user.LoginName.ToLower().Contains(filter))) && 
                    (!ShowChecked || user.IsChecked)
                orderby SortDirection == SortDirection.Name ? user.Name : user.LoginName
                select user)

                _filteredUsers.Add(user);
        }
        #endregion ApplyFilter

        #region LoadUsers
        private void LoadUsers() {
            var progress = new DlgProgress(Owner)
            {ProgressInfo = {IsIndeterminate = true, Caption = "Lade Benutzer..."}};
            progress.ExecuteModal(ActiveDirectoryImport);
        }
        #endregion LoadUsers

        #region ActiveDirectoryImport
        private void ActiveDirectoryImport() {
            _users.Clear();
            if (!String.IsNullOrEmpty(SelectedDomain)) {
                var userList = UserManager.Instance.GetActiveDirectoryUsers(SelectedDomain);
                foreach (var user in userList) {
                    _users.Add(user);
                }
            }
            ApplyFilter();
        }
        #endregion ActiveDirectoryImport

        #region ImportUsers
        public void ImportUsers() {
            Dictionary<string, User> existingUsers = new Dictionary<string, User>();
            foreach (var userExist in UserManager.Instance.EditableUsers) {
                existingUsers.Add(userExist.UserName + userExist.DomainName, userExist);
            }

            foreach (var user in Users.Where(user => user.IsChecked)) {
                var userActiveDirectory = new User {
                    FullName = user.Name,
                    UserName = user.LoginName,
                    PasswordHash =" ",
                    IsDomainUser = true,
                    DomainName = SelectedDomain
                };

                if (!existingUsers.ContainsKey(userActiveDirectory.UserName + userActiveDirectory.DomainName)) UserManager.Instance.AddUser(userActiveDirectory);

                if (existingUsers.ContainsKey(userActiveDirectory.UserName + userActiveDirectory.DomainName) &&
                    existingUsers[userActiveDirectory.UserName + userActiveDirectory.DomainName].DomainName !=
                    userActiveDirectory.DomainName) UserManager.Instance.AddUser(userActiveDirectory);

            }
        }
        #endregion ImportUsers

        #region Domains
        private ObservableCollection<string> _domains;

        public IEnumerable<string> Domains {
            get {
                if (_domains == null) {
                    try {
                        _domains = Utils.ActiveDirectory.GetDomains();
                    } catch (Exception) {
                        MessageBox.Show("Error in reading domain.", ResourcesCommon.Error);
                        _domains = new ObservableCollection<string>();
                    }
                    SelectedDomain = _domains.FirstOrDefault();
                }
                return _domains;
            }
        }
        #endregion

        #region SelectedDomain
        private string _selectedDomain;

        public string SelectedDomain {
            get { return _selectedDomain; }
            set {
                _selectedDomain = value;
                LoadUsers();
            }
        }
        #endregion
    }
}