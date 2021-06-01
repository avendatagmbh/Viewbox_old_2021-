// (C)opyright 2011 AvenDATA GmbH
// ----------------------------------------
// author: Mirko Dibbert
// since : 2011-08-27

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using Base.Localisation;
using Config.DbStructure;
using Config.Interfaces.DbStructure;
using DbAccess;
using Utils;

namespace Config.Manager {
    /// <summary>
    /// This class provides several user management functions.
    /// </summary>
    public class UserManager : NotifyPropertyChangedBase {
        #region constructor
        internal UserManager(IDatabase conn) {
            try {
                List<User> users = conn.DbMapping.LoadSorted<User>("username");

                if (users.Count == 0) {
                    // no users found, create default admin user
                    var user = new User {
                                            UserName = "admin",
                                            FullName = "Administrator",
                                            IsAdmin = true
                                        };

                    SetPassword(user, "admin");
                    AddUser(user, conn);
                }
                else {
                    // init user list with existing users
                    foreach (User user in users) {
                        _users.Add(user);
                        _userById[user.Id] = user;
                    }
                }
            }
            catch (Exception ex) {
                throw new Exception("Could not load users: " + ex.Message);
            }
        }
        #endregion constructor

        private static readonly Dictionary<int, IUser> _userById = new Dictionary<int, IUser>();

        #region properties

        #region CurrentUser
        private IUser _currentUser;

        public IUser CurrentUser {
            get { return _currentUser; }
            private set {
                _currentUser = value;
                OnPropertyChanged("CurrentUser");
            }
        }
        #endregion CurrentUser

        #region Users
        public ObservableCollection<IUser> _users = new ObservableCollection<IUser>();

        public IEnumerable<IUser> Users {
            get { return _users; }
        }
        #endregion Users

        //--------------------------------------------------------------------------------

        //--------------------------------------------------------------------------------
        #endregion properties

        #region methods
        //--------------------------------------------------------------------------------

        public static void SetPassword(IUser user, string password) {
            var rnd = new Random();
            var sbSalt = new StringBuilder(32);
            for (int i = 0; i < 32; i++) sbSalt.Append((char) rnd.Next(32, 122));
            (user as User).Salt = sbSalt.ToString();
            (user as User).PasswordHash = StringUtils.GetPasswordHash(password, (user as User).Salt);
        }

        /// <summary>
        /// Tries to logon the specified user.
        /// </summary>
        /// <param name="user"></param>
        /// <param name="password"></param>
        /// <returns></returns>
        public bool Logon(IUser user, string password) {
            string passwordHash = StringUtils.GetPasswordHash(password, (user as User).Salt);
            if ((user as User).PasswordHash.Equals(passwordHash)) {
                CurrentUser = user;
                ConfigDb.ProfileManager.InitVisibleProfiles(user);
                return true;
            }
            else return false;
        }

        public bool CanLogon(IUser user, string password) {
            string passwordHash = StringUtils.GetPasswordHash(password, (user as User).Salt);
            return (user as User).PasswordHash.Equals(passwordHash);
        }


        /// <summary>
        /// Log off the current user.
        /// </summary>
        public void Logoff() {
            CurrentUser = null;
            ConfigDb.ProfileManager.ClearVisibleProfiles();
        }
        
        /// <summary>
        /// Adds the specified user.
        /// </summary>
        /// <param name="user">The user.</param>
        /// <param name="conn">The conn.</param>
        public void AddUser(IUser user, IDatabase conn = null) {
            try {
                if (conn == null) ConfigDb.Save(user);
                else conn.DbMapping.Save(user);

                _userById[(user as User).Id] = user;

                // insert sorted into observable collection
                var tmp = new List<IUser>(Users);
                tmp.Add(user);
                tmp.Sort();
                _users.Clear();
                foreach (User user1 in tmp) _users.Add(user1);
            }
            catch (Exception ex) {
                throw new Exception(string.Format(ExceptionMessages.UserManagerAdd, ex.Message));
            }
        }

        /// <summary>
        /// Deletes the specified user.
        /// </summary>
        /// <param name="user">The user.</param>
        public void DeleteUser(IUser user) {
            try {
                ConfigDb.Delete(user);
                _users.Remove(user);
                _userById.Remove((user as User).Id);
            }
            catch (Exception ex) {
                throw new Exception(string.Format(ExceptionMessages.UserManagerDelete, ex.Message));
            }
        }
        
        /// <summary>
        /// Updates the values of an existing user.
        /// </summary>
        /// <param name="user"></param>
        public void UpdateUser(IUser user) {
            try {
                ConfigDb.Save(user);
            }
            catch (Exception ex) {
                throw new Exception(string.Format(ExceptionMessages.UserManagerUpdate, ex.Message));
            }
        }


        /// <summary>
        /// Checks if the specified user name already exists.
        /// </summary>
        /// <param name="userName"></param>
        /// <returns></returns>
        public bool Exists(string userName) {
            foreach (User user in Users) {
                if (string.IsNullOrEmpty(user.UserName) && string.IsNullOrEmpty(userName)) return true;
                else if (string.IsNullOrEmpty(user.UserName) || string.IsNullOrEmpty(userName)) continue;
                else if (user.UserName.ToLower().Equals(userName.ToLower())) return true;
            }

            return false;
        }

        public IUser GetUser(int id) {
            return _userById.ContainsKey(id) ? _userById[id] : null;
        }

        //--------------------------------------------------------------------------------
        #endregion methods
    }
}