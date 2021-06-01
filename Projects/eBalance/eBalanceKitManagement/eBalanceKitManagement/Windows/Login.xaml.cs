using System;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Input;
using CustomResources;
using eBalanceKitBusiness.Manager;
using eBalanceKitBusiness.Structures;
using eBalanceKitBusiness.Structures.DbMapping;
using eBalanceKitBase.Windows;
using eBalanceKitBase.Structures;
using eBalanceKitBusiness.Exceptions;
using eBalanceKitManagement.Structures;
using System.Collections.Generic;
using DbAccess;
using System.Collections.ObjectModel;
using Utils;
using eBalanceKitResources.Localisation;

namespace eBalanceKitManagement.Windows {

    public partial class Login : Window {

        private bool _keepAppConfig;
        private DlgProgress _progress;
        private bool _initCalled;

        #region constructor
        public void InitLogin(Language language = null) {

            _initCalled = true;

            Title = CustomStrings.ProductName + " / " + ResourcesLogin.Caption;

            ////Ignore if the database is out of date because it can be upgraded with this tool
            //// set language (currently only for localisation testing purposes)
            //System.Threading.Thread.CurrentThread.CurrentCulture = CultureInfo.CreateSpecificCulture("de-DE");
            //System.Threading.Thread.CurrentThread.CurrentUICulture = CultureInfo.CreateSpecificCulture("de-DE");
            ////System.Threading.Thread.CurrentThread.CurrentUICulture = CultureInfo.CreateSpecificCulture("en-US");
            //Thread.CurrentThread.CurrentUICulture = AppConfig.SelectedLanguage.Culture;
            // load user config

            try {
                UserConfig.Init();
            } catch (Exception ex) {
                MessageBox.Show(
                    ex.Message + Environment.NewLine,
                    ResourcesLogin.Error,
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }

            CultureInfo cultureToSet = null;
            //Thread.CurrentThread.CurrentCulture = CultureInfo.CreateSpecificCulture("de-DE");
            //if (language != null) {
            //    Thread.CurrentThread.CurrentUICulture = language.Culture;
            //} else {
            //    // set last language if exist
            //    if (!string.IsNullOrEmpty(UserConfig.LastLanguage)) {
            //        cultureToSet = CultureInfo.CreateSpecificCulture(UserConfig.LastLanguage);
            //        Thread.CurrentThread.CurrentUICulture = cultureToSet;
            //        //UserManager.Instance.UpdateUser(UserManager.Instance.CurrentUser);
            //    } else {
            //        Thread.CurrentThread.CurrentUICulture = AppConfig.SelectedLanguage.Culture;
            //    }
            //}

            Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;
            if (Thread.CurrentThread.CurrentUICulture.TwoLetterISOLanguageName != AppConfig.SelectedLanguage.Culture.TwoLetterISOLanguageName)
            {
                Thread.CurrentThread.CurrentUICulture =
                    CultureInfo.CreateSpecificCulture(AppConfig.SelectedLanguage.Culture.Name);
                try
                {
                    new Login().Show();
                }
                catch (Exception)
                {
                    MessageBox.Show(ResourcesLogin.CantCreateLogin);
                }
                Close();
                return;
            }

            if (language == null)
                cultureToSet = CultureInfo.CreateSpecificCulture(UserConfig.LastLanguage);

            InitializeComponent();

#if DEBUG
            LanguagePanel.Visibility = Visibility.Visible;
#endif

            // register app config events
            AppConfig.Error += new System.IO.ErrorEventHandler(AppConfig_Error);

            this.txtVersion.Text = ResourcesLogin.Version +
                                   System.Reflection.Assembly.GetEntryAssembly().GetName().Version.Major + "." +
                                   System.Reflection.Assembly.GetEntryAssembly().GetName().Version.Minor +
                                   (System.Reflection.Assembly.GetEntryAssembly().GetName().Version.Build > 0
                                        ? "." + System.Reflection.Assembly.GetEntryAssembly().GetName().Version.Build
                                        : string.Empty);

            if (!Utils.MiscUtils.IsVS2010RTLInstalled()) {
                MessageBox.Show(ResourcesLogin.InstallRuntimeCPlusPlus2010);
                this.Close();
                return;
            }

            //// load user config
            //try {
            //    UserConfig.Init();
            //} catch (Exception ex) {
            //    MessageBox.Show(
            //        ex.Message + Environment.NewLine,
            //        "Fehler",
            //        MessageBoxButton.OK,
            //        MessageBoxImage.Error);
            //}

            if (!AppConfig.IsInitialized) {
                if (!AppConfig.DatabaseConfig.ExistsConfigfile) {
                    MessageBox.Show(
                        ResourcesLogin.MissingConfigFile);
                    this.Close();
                    return;
                }

                _progress = new DlgProgress(null)
                            {WindowStartupLocation = System.Windows.WindowStartupLocation.CenterScreen};
                new Thread(Init) {
                    CurrentCulture = System.Threading.Thread.CurrentThread.CurrentCulture,
                    CurrentUICulture = System.Threading.Thread.CurrentThread.CurrentUICulture
                }.Start();
                _progress.ShowDialog();
                if (_progress.DialogResult == false) {
                    this.Close();
                    return;
                }
            }

            if (cultureToSet != null) {
                Language lastLng = AppConfig.Languages.FirstOrDefault(l => string.Compare(l.Culture.Name, cultureToSet.Name, StringComparison.InvariantCultureIgnoreCase) == 0);
                if (lastLng != null) {
                    AppConfig.SelectedLanguage = lastLng;
                    languages.SelectedItem = lastLng;
                }
            }

            try {
                ObservableCollection<LoginUser> loginUsers = new ObservableCollection<LoginUser>();
                //using (IDatabase conn = AppConfig.ConnectionManager.GetConnection()) {
                //using (IDatabase conn = ConnectionManager.CreateConnection(AppConfig.DatabaseConfig.DbConfig)) {
                //    conn.Open();
                //    System.Data.Common.DbDataReader reader = conn.ExecuteReader("SELECT * FROM " + conn.Enquote("users"));
                //    while (reader.Read()) {
                //        LoginUser current = new LoginUser();
                //        current.UserName = Convert.ToString(reader["username"]);
                //        current.FullName = Convert.ToString(reader["fullname"]);
                //        current.PasswordHash = Convert.ToString(reader["password"]);
                //        current.EncryptedKey = Convert.ToString(reader["key"]);
                //        current.IsActive = Convert.ToBoolean(reader["is_active"]);
                //        current.IsAdmin = Convert.ToBoolean(reader["is_admin"]);
                //        loginUsers.Add(current);
                //    }
                //}
                //username.ItemsSource = loginUsers;

                username.ItemsSource = UserManager.Instance.ActiveUsers;

                //// set last user if any exist
                if (!string.IsNullOrEmpty(UserConfig.LastUser)) {
                    foreach (User user in UserManager.Instance.ActiveUsers) {
                        if (user.UserName == UserConfig.LastUser) {
                            username.SelectedItem = user;
                            break;
                        }
                    }
                    //foreach (LoginUser user in loginUsers) {
                    //    if (user.UserName == UserConfig.LastUser) {
                    //        username.SelectedItem = user;
                    //        break;
                    //    }
                    //}

                    // set initial focus to password
                    password.Focus();

                } else {
                    // set initial focus to username
                    username.Focus();
                }

            } catch (Exception ex) {
                MessageBox.Show(
                    ex.Message + Environment.NewLine,
                    ResourcesLogin.Error,
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);

                _keepAppConfig = false;
                this.Close();
            }

            if (Info.Serial == null || Info.RK == null) {
                _keepAppConfig = true;
                //new Register().Show();
                this.Close();
                MessageBox.Show(ResourcesLogging.NotRegistered);
            }

            UserManager.Instance.LoginByCommandLine(Logon);

        }


        public Login(Language language) {
            if (!_initCalled) {
                InitLogin(language);
            }
        }

        public Login() {
            if (!_initCalled) {
                InitLogin();
            }
        }

        #endregion constuctor

        #region event handler

        #region btnOk_Click
        private void btnOk_Click(object sender, RoutedEventArgs e) {
            btnOk.IsEnabled = false;

            new Thread(Logon) {
                CurrentCulture = System.Threading.Thread.CurrentThread.CurrentCulture,
                CurrentUICulture = System.Threading.Thread.CurrentThread.CurrentUICulture
            }.Start();
        }
        #endregion

        #region password_KeyDown
        private void password_KeyDown(object sender, KeyEventArgs e) {
            if (e.Key == Key.Return) {
                e.Handled = true;
                btnOk.IsEnabled = false;

                new Thread(Logon) {
                    CurrentCulture = System.Threading.Thread.CurrentThread.CurrentCulture,
                    CurrentUICulture = System.Threading.Thread.CurrentThread.CurrentUICulture
                }.Start();

            }

            if (e.Key == Key.Escape) {
                e.Handled = true;
                this.Close();
            }
        }
        #endregion

        #region Window_Closed
        private void Window_Closed(object sender, System.EventArgs e) {
            if (!_keepAppConfig) AppConfig.Dispose();
            AppConfig.Error -= new System.IO.ErrorEventHandler(AppConfig_Error);
        }
        #endregion

        #region AppConfig_Error
        void AppConfig_Error(object sender, System.IO.ErrorEventArgs e) {
            MessageBox.Show(e.GetException().Message + Environment.NewLine + e.GetException().StackTrace);
        }
        #endregion

        #endregion

        #region Init
        private void Init() {
            try {
                AppConfig.Init(_progress.ProgressInfo);
            } catch (Exception ex) {
                this.Dispatcher.Invoke(
                    System.Windows.Threading.DispatcherPriority.Background,
                    new Action(delegate {

                    _progress.DialogResult = false;

                    if (ex is DatabaseOutOfDateException || ex is ProgramOutOfDateException) {
                        MessageBox.Show(
                            ex.Message + Environment.NewLine,
                            ResourcesLogin.Hint,
                            MessageBoxButton.OK,
                            MessageBoxImage.Information);

                    } else {
                        MessageBox.Show(
                            ex.Message + Environment.NewLine,
                            ResourcesLogin.Error,
                            MessageBoxButton.OK,
                            MessageBoxImage.Error);
                    }
                    _keepAppConfig = false;
                    this.Close();
                }));
            }

            this.Dispatcher.Invoke(
                System.Windows.Threading.DispatcherPriority.Background,
                new Action(delegate {
                _progress.DialogResult = true;
                _progress.Close();
                //_progress = null;
            }));

        }
        #endregion Init

        #region Logon
        private void Logon() { Logon(null, null); }

        private void Logon(User user, string passwordHash) {


            this.Dispatcher.Invoke(
                System.Windows.Threading.DispatcherPriority.Background,
                new Action(delegate {

                try {
                    bool autoLogin;
                    User loginUser;

                    if (user != null) {
                        autoLogin = user.AutoLogin;
                        user.AutoLogin = true;
                        loginUser = user;
                    } else {
                        loginUser = username.SelectedValue as User;
                        if (loginUser == null) {
                            throw new Exception("invalid attempt to login user.");
                        }
                        autoLogin = loginUser.AutoLogin;
                    }


                    if ((!string.IsNullOrEmpty(passwordHash) && UserManager.Instance.AutoLogin(user, passwordHash)) || (UserManager.Instance.Logon(loginUser, password.Password))) {

                        //if (!loginUser.IsAdmin) {
                        //    MessageBox.Show(eBalanceKitResources.Localisation.ResourcesLogin.AccessDenied);
                        //    btnOk.IsEnabled = true;
                        //    return;
                        //}

                        // update and save user config
                        UserConfig.LastUser = loginUser.UserName;
                        loginUser.AutoLogin = autoLogin;

                        if (AppConfig.SelectedLanguage != null)
                            UserConfig.LastLanguage = AppConfig.SelectedLanguage.Culture.TwoLetterISOLanguageName;

                        UserConfig.Save();

                        Window window = null;

                        try {
                            // open main window
                            window = new MainWindow();
                            window.Show();

                            // initialization succeed, close login window
                            _keepAppConfig = true;
                            this.Close();

                        } catch (Exception ex) {
                            if (window != null) window.Close();
                            throw new Exception(ex.Message, ex);
                        }

                    } else {
                        MessageBox.Show(eBalanceKitResources.Localisation.ResourcesLogin.InvalidUserOrPassword);
                        password.SelectAll();
                        password.Focus();
                        btnOk.IsEnabled = true;
                    }

                } catch (Exception ex) {
                    MessageBox.Show(ex.Message);
                    btnOk.IsEnabled = true;
                }
            }));
        }
        #endregion

        #region SelectedLanguage
        public Language SelectedLanguage { get { return AppConfig.SelectedLanguage; } set {
            if (AppConfig.SelectedLanguage == value) return;
            AppConfig.SelectedLanguage = value;
            new Login(value).Show();
            _keepAppConfig = true;
            Close();
        } }
        #endregion SelectedLanguage

        private void Window_Loaded(object sender, RoutedEventArgs e) {

        }
    }
}