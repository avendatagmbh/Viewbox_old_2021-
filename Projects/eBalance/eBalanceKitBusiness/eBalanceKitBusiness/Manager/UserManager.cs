/**************************************************************************************************************
 * author               date            comment
 * ------------------------------------------------------------------------------------------------------------
 * Mirko Dibbert        2010-12-28      initial implementation
 *************************************************************************************************************/

using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.DirectoryServices.ActiveDirectory;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Collections.ObjectModel;
using Utils;
using eBalanceKitBase.Structures;
using eBalanceKitBusiness.Logs;
using eBalanceKitBusiness.Rights;
using eBalanceKitBusiness.Structures.DbMapping;
using DbAccess;
using eBalanceKitBusiness.Structures;
using System.DirectoryServices;
using eBalanceKitResources.Localisation;


namespace eBalanceKitBusiness.Manager {

    /// <summary>
    /// This class provides several user management functions.
    /// </summary>
    public class UserManager {

        private static UserManager _instance;

        private UserManager() { }

        public static UserManager Instance { get { return _instance ?? (_instance = new UserManager()); } }



        #region properties

        /// <summary>
        /// Gets or sets the current user.
        /// </summary>
        /// <value>The current user.</value>
        public User CurrentUser { get; private set; }

        /// <summary>
        /// Gets or sets the list of all available users (including inactive and deleted users).
        /// </summary>
        /// <value>The users.</value>
        public ObservableCollection<User> Users { get; private set; }

        /// <summary>
        /// Gets or sets the editable users (including active and inactive, but not deleted users).
        /// </summary>
        /// <value>The active users.</value>
        public ObservableCollection<User> EditableUsers { get; private set; }

        /// <summary>
        /// Gets or sets the active users (including active and not deleted users).
        /// </summary>
        /// <value>The active users.</value>
        public ObservableCollection<User> ActiveUsers { get; private set; }

        /// <summary>
        /// Gets or sets the id-ordered user dictionary.
        /// </summary>
        /// <value>The id-ordered user user dictionary.</value>
        private Dictionary<int, User> UserById { get; set; }

        private RightDeducer _rightDeducer;
        /// <summary>
        /// Gets or sets the rights deducer.
        /// </summary>
        /// <value>The right deducer.</value>
        public RightDeducer RightDeducer {
            get {
                if (_rightDeducer == null)
                    ReinitializeRightDeducer();
                return _rightDeducer;
            }
            private set { _rightDeducer = value; }
        }
        #endregion properties

        /*****************************************************************************************************/

        #region eventHandler

        /// <summary>
        /// Handles the PropertyChanged event of the user control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.ComponentModel.PropertyChangedEventArgs"/> instance containing the event data.</param>
         void user_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e) {
            if (e.PropertyName == "IsActive") {
                User user = sender as User;
                if (user.IsActive) {
                    ActiveUsers.Add(user);
                } else {
                    ActiveUsers.Remove(user);
                }
            }
        }

        #endregion eventHandler

        /*****************************************************************************************************/

        #region methods

        /// <summary>
        /// Inits this instance.
        /// </summary>
        public  void Init() {

            Users = new ObservableCollection<User>();
            ActiveUsers = new ObservableCollection<User>();
            EditableUsers = new ObservableCollection<User>();
            UserById = new Dictionary<int, User>(); 
            
            List<User> tmp = new List<User>(Users);
            
            using (IDatabase conn = AppConfig.ConnectionManager.GetConnection()) {
                try {
                    //Only database versions after or equal to 1.3.0 have the is_deleted flag
                    foreach (User user in conn.DbMapping.LoadSorted<User>(conn.Enquote("username"))) {
                        user.PropertyChanged += new System.ComponentModel.PropertyChangedEventHandler(user_PropertyChanged);
                        tmp.Add(user);
                    }

                    if (tmp.Count == 0) {
                        // create default admin user
                        User user = new User {
                            UserName = "admin",
                            FullName = "Administrator",
                            IsAdmin = true
                        };

                        SetPassword(user, "admin");
                        
                        AddUser(user);
                    } else {
                        tmp.Sort();

                        Users.Clear();
                        ActiveUsers.Clear();
                        EditableUsers.Clear();

                        foreach (User u in tmp)
                        {
                            LogManager.Instance.NewUser(u, true);
                            Users.Add(u);
                            if (u.IsActive && !u.IsDeleted) ActiveUsers.Add(u);
                            if (!u.IsDeleted) EditableUsers.Add(u);
                            UserById[u.Id] = u;
                        }
                    }

                } catch (Exception ex) {
                    throw new Exception("Could not load users: " + ex.Message);
                }
            }
        }

        /*****************************************************************************************************/

        public  void SetPassword(User user, string password) {
            user.Salt = Guid.NewGuid().ToString("N");
            user.PasswordHash = Utils.StringUtils.GetPasswordHash(password, user.Salt);
        }


        /// <summary>
        /// Tries to logon the specified user.
        /// </summary>
        public  bool Logon(User user, string password) {
            bool valid = false;

            if (user.IsDomainUser) {
                //check active directory if the user exists
                using (DirectoryEntry directoryEntry = new DirectoryEntry("LDAP://" + user.DomainName)) {
                    directoryEntry.Username = user.UserName;
                    DirectorySearcher searcher = new DirectorySearcher(directoryEntry);
                    try {
                        searcher.FindOne();
                        valid = true;
                    } catch (COMException ex) {
                        //the error code -2147023570 == bad user or password
                        if (ex.ErrorCode == -2147023570) valid = false;
                    }
                }

                //if domain user exists then good to go
                if (valid) {
                    LoginUserAsCurrent(user);
                    return true;
                } 

            } else {
                // try to log in local user
                string passwordHash = Utils.StringUtils.GetPasswordHash(password, user.Salt);
                if (user.PasswordHash.Equals(passwordHash)) {
                    LoginUserAsCurrent(user);
                    return true;
                } 
            }



            // login failed
            return false;
        }

        private void LoginUserAsCurrent(User user) {
            CurrentUser = user;
            List<UpgradeInformation> upgradeInformations;
            // upgrade data context is read in the Login.xaml.cs. The welcome dialog will use this as DataContext.
            user.UpgradeDataContext = new UpgradeDataContext {
                Rows = new ObservableCollection<UpgradeRow>(),
                Header = ResourcesLogin.WelcomeTitle,
                Value = ResourcesLogin.WelcomeText
            };
            using (var conn = AppConfig.ConnectionManager.GetConnection()) {
                upgradeInformations = conn.DbMapping.LoadSorted<UpgradeInformation>(conn.Enquote("upgrade_available_from"));
            }
            if (user.LastLogin == null) {
                ////int? upgradeInfoId =
                ////    (int?)
                ////    conn.ExecuteScalar(string.Format("SELECT {0} FROM {1} WHERE {2} = (SELECT MAX({2}) FROM {1})",
                ////                                     conn.Enquote("id"), conn.Enquote("upgrade_information"),
                ////                                     conn.Enquote("upgrade_available_from")));
                if (upgradeInformations.Count > 0) {
                    ////UpgradeInformation upgradeInformation = conn.DbMapping.Load<UpgradeInformation>(upgradeInfoId);
                    UpgradeInformation lastUpgradeInfo = upgradeInformations[upgradeInformations.Count - 1];
                    // if the user checks IsKeepNextOpen than the PossibleLastLogin will be the user's last login date.
                    // this way the database's last login is not correct, but that is only used in comparing with the release date.
                    user.UpgradeDataContext.PossibleLastLogin = lastUpgradeInfo.UpgradeAvailableFrom.AddSeconds(-1d);
                    UpgradeRow temp = lastUpgradeInfo.CreateUpgradeRow();
                    if (temp != null) 
                        user.UpgradeDataContext.Rows.Add(temp);
                    ////upgradeInformation.OpenResource();
                }
            } else {
                ////upgradeInformations =
                ////    conn.DbMapping.Load<UpgradeInformation>(
                ////        string.Format("{0} > (SELECT {1} FROM {2} WHERE {3} = {4})",
                ////                      conn.Enquote("upgrade_available_from"), conn.Enquote("last_login"),
                ////                      conn.Enquote("users"), conn.Enquote("id"), user.Id));
                // if the user checks IsKeepNextOpen than the PossibleLastLogin will be the user's last login date.
                // this way the database's last login is not correct, but that is only used in comparing with the release date.
                user.UpgradeDataContext.PossibleLastLogin = user.LastLogin.Value;
                // 'i' will be the index, how much upgrade information is seen already
                int i;
                for (i = 0; i < upgradeInformations.Count; i++) {
                    // user.LastLogin only used there.
                    if (user.LastLogin < upgradeInformations[i].UpgradeAvailableFrom) {
                        break;
                    }
                }
                // we don't need the seen upgrade infos
                upgradeInformations.RemoveRange(0, i);
                foreach (UpgradeInformation upgradeInformation in upgradeInformations) {
                    UpgradeRow temp = upgradeInformation.CreateUpgradeRow();
                    if (temp != null)
                        user.UpgradeDataContext.Rows.Add(temp);
                }
            }
            CurrentUser.LastLogin = DateTime.Now;
            using (var conn = AppConfig.ConnectionManager.GetConnection()) {
                conn.DbMapping.Save(CurrentUser);
            }
            UpdateUsersEditableList();
            //DEVNOTE: do not initialize RightDeducer until RoleManager is not initialized
            //RightDeducer = new RightDeducer(CurrentUser);
        }

        public void ReinitializeRightDeducer() {
            _rightDeducer = new RightDeducer(CurrentUser);
        }

        //public bool Logon(params )

        /// <summary>
        /// Log off the current user.
        /// </summary>
        public  void Logoff() {
            CurrentUser = null;
            DocumentManager.Instance.CurrentDocument = null;
            RightDeducer = null;
        }

        //public  void Sort<TSource, TKey>(this Collection<TSource> source, Func<TSource, TKey> keySelector) {
        //    var sortedList = source.OrderBy(keySelector).ToList();
        //    source.Clear();
        //    foreach (var sortedItem in sortedList)
        //        source.Add(sortedItem);
        //}

        /// <summary>
        /// Adds the specified user.
        /// </summary>
        /// <param name="user">The user.</param>
        public  void AddUser(User user) {            
            if (!user.IsDomainUser && Exists(user.UserName)) return;
            using (IDatabase conn = AppConfig.ConnectionManager.GetConnection()) {
                try {

                    conn.DbMapping.Save(user);
                    UserById[user.Id] = user;
                    
                    // insert sorted into observable collection
                    List<User> tmp = new List<User>(Users);
                    tmp.Add(user);
                    tmp.Sort();

                    //Users = new ObservableCollection<User>(tmp);
                    //ActiveUsers = new ObservableCollection<User>(tmp.Where(auser => auser.IsActive && !auser.IsDeleted));
                    //EditableUsers = new ObservableCollection<User>(tmp.Where(euser => !euser.IsDeleted));

                    //Users.Add(user);
                    //EditableUsers.Add(user);
                    //if (user.IsActive) {
                    //    ActiveUsers.Add(user);
                    //}

                    //Users = new ObservableCollection<User>(Users.OrderBy(u => u.UserName).ToList());
                    //EditableUsers = new ObservableCollection<User>(EditableUsers.OrderBy(u => u.UserName).ToList());
                    //ActiveUsers = new ObservableCollection<User>(ActiveUsers.OrderBy(u => u.UserName).ToList());

                    //Users.OrderBy(u => u.UserName);

                    //Users.Sort(x => x.UserName);
                    //EditableUsers.Sort(x => x.UserName);
                    //ActiveUsers.Sort(x => x.UserName);

                    Users.Clear();
                    ActiveUsers.Clear();
                    EditableUsers.Clear();
                    foreach (User u in tmp) {
                        Users.Add(u);
                        if (u.IsActive && !u.IsDeleted) ActiveUsers.Add(u);
                        if (!u.IsDeleted) EditableUsers.Add(u);
                    }

                    if (DocumentManager.Instance.CurrentDocument != null)
                        DocumentManager.Instance.CurrentDocument.OnAllowedOwnersChanged();

                    UpdateUsersEditableList();
                    LogManager.Instance.NewUser(user, false);

                } catch (Exception ex) {
                    throw new Exception(Localisation.ExceptionMessages.UserManagerAdd + ex.Message);
                }
            }
        }


        /// <summary>
        /// Deletes the specified user.
        /// </summary>
        /// <param name="user">The user.</param>
        public  void DeleteUser(User user) {
            using (IDatabase conn = AppConfig.ConnectionManager.GetConnection()) {
                
                try {
                    LogManager.Instance.DeleteUser(user);
                    user.IsDeleted = true;
                    conn.DbMapping.Save(user);

                    // reset the owner of documents to a not deleted user
                    if (DocumentManager.Instance.Documents.Any(doc => doc.OwnerId == user.Id)) {
                        foreach (var document in DocumentManager.Instance.Documents.Where(doc => doc.OwnerId == user.Id)) {
                            // The new owner will be the current user if it's an admin (should be but you never know) or the first available admin will be the new owner
                            document.Owner = CurrentUser.IsAdmin ? CurrentUser : ActiveUsers.First(validUser => validUser.IsAdmin);
                        }
                    }

                    ActiveUsers.Remove(user);
                    EditableUsers.Remove(user);
                    UpdateUsersEditableList();
                } catch (Exception ex) {
                    throw new Exception(Localisation.ExceptionMessages.UserManagerDelete + ex.Message);
                }

                RightManager.UserDeleted(conn, user);
            }
        }

        private  void UpdateUsersEditableList() {
            // Update editable users collection for rights management
            if (CurrentUser != null && CurrentUser.AllowUserManagement)
                CurrentUser.OnEditableUsersCollectionChanged(null,
                                                             new NotifyCollectionChangedEventArgs(
                                                                 NotifyCollectionChangedAction.Reset));
        }
        
        /// <summary>
        /// Updates the values of an existing user.
        /// </summary>
        /// <param name="user"></param>
        public  void UpdateUser(User user) {
            using (IDatabase conn = AppConfig.ConnectionManager.GetConnection()) {
                try {
                    conn.DbMapping.Save(user);
                } catch (Exception ex) {
                    throw new Exception(Localisation.ExceptionMessages.UserManagerUpdate + ex.Message);
                }
            }
        }


        /// <summary>
        /// Checks if the specified user name already exists.
        /// </summary>
        /// <param name="userName"></param>
        /// <returns></returns>
        public  bool Exists(string userName) {
            foreach (User user in EditableUsers.Where(user => !user.IsDomainUser)) {
                if (string.IsNullOrEmpty(user.UserName) && string.IsNullOrEmpty(userName)) return true;
                if (string.IsNullOrEmpty(user.UserName) || string.IsNullOrEmpty(userName)) continue;
                if (user.UserName.ToLower().Equals(userName.ToLower())) return true;
            }

            return false;
        }

        #region GetActiveDirectoryUsers
        public  IEnumerable<UserInfoActiveDirectory> GetActiveDirectoryUsers(string domainName) {
            var userList = new List<UserInfoActiveDirectory>();
            userList.AddRange(from user in Utils.ActiveDirectory.GetActiveDirectoryUsers(domainName) select new UserInfoActiveDirectory(){Name = user.Item1, LoginName = user.Item2});
            return userList;
        }
        #endregion GetActiveDirectoryUsers


        public  User GetUser(int id) { return UserById.ContainsKey(id) ? UserById[id] : null; }

        #endregion methods
        
        public bool AutoLogin(User user, string passwordHash) {

            if (!user.IsActive) {
                throw new Exception(ResourcesLogin.NotAcitveUser);
            }

            bool valid = false;

            if (user.IsDomainUser) {
                //check active directory if the user exists
                using (DirectoryEntry directoryEntry = new DirectoryEntry("LDAP://" + user.DomainName)) {
                    directoryEntry.Username = user.UserName;
                    DirectorySearcher searcher = new DirectorySearcher(directoryEntry);
                    try {
                        searcher.FindOne();
                        valid = true;
                    }
                    catch (COMException ex) {
                        //the error code -2147023570 == bad user or password
                        if (ex.ErrorCode == -2147023570) valid = false;
                    }
                }

                //if domain user exists then good to go
                if (valid) {
                    LoginUserAsCurrent(user);
                    return true;
                }

            }
            else if (user.AutoLogin && user.PasswordHash.Equals(passwordHash)) {
                LoginUserAsCurrent(user);
                return true;
            }
            return false;
        }

        /// <summary>
        /// Delegate of login method that is called by <see cref="UserManager.LoginByCommandLine"/>
        /// </summary>
        /// <param name="user">The user you would like to login.</param>
        /// <param name="passwordOrHash">Usualy the password hash but could also be the password if the LoginMethod works with the plain password.</param>
        public delegate void LoginMethod(User user, string passwordOrHash);

        /// <summary>
        /// cmd call has to be like "C:\eBalanceKitManagement.exe mode=ebk 1=C382CD6D9737BF122145790FD0115986D60FD3C5"
        /// User with Id=1 and password hash=C382CD6D9737BF122145790FD0115986D60FD3C5 will be logged on
        /// if user is domain user a dummy password hash has to be sent
        /// </summary>
        /// <param name="loginMethod">The method that should be executed to proceed the login.</param>
        /// <returns>Login done?</returns>
        public bool LoginByCommandLine(LoginMethod loginMethod) {

            
            var cmdArgs = Environment.GetCommandLineArgs();
            // There are CommandLine parameters
            if (cmdArgs.Any()) {
                // with the right length
                if (cmdArgs.Length == 3) {
                    // so we take the first parameter (0 = path to the app)
                    // and split it
                    var mode = cmdArgs[1].Split('=');
                    // if it equals the expected value 
                    if (mode.Length == 2 && mode[0].ToLower().Equals("mode") && mode[1].ToLower().Equals("ebk")) {
                        // everything is fine so far so we take the 2nd parameter and split it
                        var info = cmdArgs[2].Split('=');
                        int userId;
                        // we try to parse the user id
                        if (info.Length == 2 && int.TryParse(info[0], out userId) && userId != 0) {
                            var user = GetUser(userId);
                            // and get the user
                            if (user != null) {
                                if (user.IsDomainUser || user.PasswordHash.Equals(info[1])) {
                                    loginMethod(user, info[1]);
                                    return true;
                                }
                            }
                        }
                    }
                }
            }
            return false;
        }
    }
}
