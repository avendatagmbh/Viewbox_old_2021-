// --------------------------------------------------------------------------------
// author: Mirko Dibbert
// since:  2010-09-29
// copyright 2011 AvenDATA GmbH
// --------------------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Hosting;
using System.Security.Policy;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Threading;
using CustomResources;
using Utils;
using Utils.Commands;
using eBalanceKitBase.Structures;
using eBalanceKitResources.Localisation;
using eBalanceKitBase.Windows;
using eBalanceKitBusiness.Exceptions;
using eBalanceKitBusiness.Manager;
using eBalanceKitBusiness.Structures;
using eBalanceKitBusiness.Structures.DbMapping;
using eBalanceKitBase.Converters;
using ErrorEventArgs = System.IO.ErrorEventArgs;

namespace eBalanceKit.Windows {
    /// <summary>
    /// Interaktionslogik für Login.xaml
    /// </summary>
    public partial class Login : Window {
        private DlgProgress _progress;
        private bool _keepAppConfig;
        
        #region constructor
        public Login() { InitLogin(null); }

        public Login(Language language) { InitLogin(language); }

        public void InitLogin(Language language) {

            Title = CustomStrings.ProductName + " / " + ResourcesLogin.Caption;

            //// set language (currently only for localisation testing purposes)
            //Thread.CurrentThread.CurrentCulture = CultureInfo.CreateSpecificCulture("de-DE");
            ////Thread.CurrentThread.CurrentUICulture = CultureInfo.CreateSpecificCulture("de-DE");

            //// set ui culture
            //Thread.CurrentThread.CurrentUICulture = AppConfig.SelectedLanguage.Culture;
            ////System.Threading.Thread.CurrentThread.CurrentUICulture = CultureInfo.CreateSpecificCulture("en-US");

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

            Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;
            if (Thread.CurrentThread.CurrentUICulture.TwoLetterISOLanguageName != AppConfig.SelectedLanguage.Culture.TwoLetterISOLanguageName) {
                Thread.CurrentThread.CurrentUICulture =
                    CultureInfo.CreateSpecificCulture(AppConfig.SelectedLanguage.Culture.Name);
                try {
                    new Login().Show();
                } catch (Exception) {
                    MessageBox.Show(ResourcesLogin.CantCreateLogin);
                }
                Close();
                return;
            }

            InitializeComponent();

#if DEBUG
            LanguagePanel.Visibility = Visibility.Visible;
#endif

            // register app config events
            AppConfig.Error += AppConfig_Error;

            txtVersion.Text = ResourcesLogin.Version +
                              Assembly.GetEntryAssembly().GetName().Version.Major + "." +
                              Assembly.GetEntryAssembly().GetName().Version.Minor +
                              (Assembly.GetEntryAssembly().GetName().Version.Build > 0
                                   ? "." + Assembly.GetEntryAssembly().GetName().Version.Build
                                   : "");

            if (!MiscUtils.IsVS2010RTLInstalled()) {
                MessageBox.Show(ResourcesLogin.InstallRuntimeCPlusPlus2010);
                Close();
                return;
            }

            if (!MiscUtils.IsVS2008RTLInstalled())
            {
                MessageBox.Show(ResourcesLogin.InstallRuntimeCPlusPlus2008);
                
                Close();
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
                    Close();
                    return;
                }

                _progress = new DlgProgress(null) {WindowStartupLocation = WindowStartupLocation.CenterScreen};
                //_progress.Execute(Init);
                new Thread(Init) {
                    CurrentCulture = Thread.CurrentThread.CurrentCulture,
                    CurrentUICulture = Thread.CurrentThread.CurrentUICulture
                }.Start();
                _progress.ShowDialog();
                if (_progress.DialogResult == false) {
                    Close();
                    return;
                }
            }

            try {

                username.ItemsSource = UserManager.Instance.ActiveUsers.Where(activeUser => (!activeUser.IsDomainUser) ||
                                                                         (
                                                                         (activeUser.UserName.ToLower().Equals(Environment.UserName.ToLower())) &&
                                                                          activeUser.IsDomainUser)
                                                                          ).ToList();
                
                // set last user if any exist
                if (!string.IsNullOrEmpty(UserConfig.LastUser)) {
                    foreach (User user in UserManager.Instance.ActiveUsers.Where(user => user.UserName == UserConfig.LastUser)) {
                        username.SelectedItem = user;
                        if (!string.IsNullOrEmpty(UserConfig.PasswordHash) && UserConfig.PasswordHash.Equals(user.PasswordHash)) {
                            user.AutoLogin = true;
                        } else {
                            user.AutoLogin = false;
                        }
                        break;
                    }
                    
                    foreach (User user in UserManager.Instance.ActiveUsers.Where(user => user.UserName.ToLower() == Environment.UserName.ToLower() && user.IsDomainUser)) {
                        username.SelectedItem = user;
                        user.AutoLogin = false;
                        break;
                    }
                    //IEnumerable<User> filter = from user in UserManager.Instance.ActiveUsers
                    //                           let name = user.UserName
                    //                           let isAdmin = user.IsDomainUser
                    //                           where name.ToLower().StartsWith(domainUser.ToLower())
                    //                                 || isAdmin == false
                    //                           select user;


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
                Close();
                return;
            }

            if (Info.Serial == null || Info.RK == null) {
                _keepAppConfig = true;
                new Register().Show();
                Close();
            }
        }
        #endregion constuctor

        #region event handler

        #region btnOk_Click
        private void btnOk_Click(object sender, RoutedEventArgs e) {
            if (username.SelectedItem == null) return;

            btnOk.IsEnabled = false;

            new Thread(Logon) {
                CurrentCulture = AppConfig.AppCultureInfo,
                CurrentUICulture = AppConfig.AppUiCulture
            }.Start();

            if (UserManager.Instance.CurrentUser != null) {
                RoleManager.Roles.CollectionChanged -= UserManager.Instance.CurrentUser.OnEditableUsersCollectionChanged;
                RoleManager.Roles.CollectionChanged -= UserManager.Instance.CurrentUser.OnRolesCollectionChanged;
            }
        }
        #endregion

        #region WindowPreviewKeyDown
        private void WindowPreviewKeyDown(object sender, KeyEventArgs e) {
            switch (e.Key) {
                case Key.Return:
                    e.Handled = true;
                    if (username.SelectedItem == null) return;
                    btnOk.IsEnabled = false;
                    new Thread(Logon) {
                        CurrentCulture = Thread.CurrentThread.CurrentCulture,
                        CurrentUICulture = Thread.CurrentThread.CurrentUICulture
                    }.Start();
                    break;

                case Key.Escape:
                    e.Handled = true;
                    Close();
                    break;
            }
        }
        #endregion WindowPreviewKeyDown

        #region Window_Closed
        private void Window_Closed(object sender, System.EventArgs e) {
            if (!_keepAppConfig) AppConfig.Dispose();
            AppConfig.Error -= AppConfig_Error;
        }
        #endregion

        #region AppConfig_Error
        private void AppConfig_Error(object sender, ErrorEventArgs e) { MessageBox.Show(e.GetException().Message + Environment.NewLine + e.GetException().StackTrace); }
        #endregion

        #endregion
        
        #region SelectedLanguage
        public Language SelectedLanguage {
            get { return AppConfig.SelectedLanguage; }
            set {
                if (value == AppConfig.SelectedLanguage)
                    return;
                AppConfig.SelectedLanguage = value;
                new Login(value).Show();
                UserConfig.LastLanguage = value.Culture.TwoLetterISOLanguageName;
                _keepAppConfig = true;
                Close();
            }
        }
        #endregion SelectedLanguage

        #region Init
        private void Init() {
            bool isDead = false;
            try {
                AppConfig.Init(_progress.ProgressInfo);
            } catch (Exception ex) {
                Dispatcher.Invoke(
                    DispatcherPriority.Background,
                    new Action(delegate {
                        try {
                            _progress.DialogResult = false;
                        }
                        catch (Exception) {
                            MessageBox.Show(ResourcesLogin.CantSetDialogResult);
                        }

                        Thread.CurrentThread.CurrentCulture = AppConfig.AppCultureInfo;
                        Thread.CurrentThread.CurrentUICulture = AppConfig.AppUiCulture;
                        if (ex is DatabaseOutOfDateException || ex is ProgramOutOfDateException) {
                            if (MessageBox.Show(ResourcesLogin.IsStartDatabaseManagement,
                                                ResourcesLogin.IsStartDatabaseManagementCaption,
                                                MessageBoxButton.YesNo, MessageBoxImage.Question, MessageBoxResult.No) ==
                                MessageBoxResult.Yes) {
// ReSharper disable RedundantAssignment
                                string path = Path.Combine(Environment.CurrentDirectory, "DatabaseManagement.exe");
// ReSharper restore RedundantAssignment
#if DEBUG
                                path = Path.Combine(Environment.CurrentDirectory, "..", "..", "..", "..",
                                                    "DatabaseManagement", "DatabaseManagement", "bin", "Debug", "DatabaseManagement.exe");
#endif
                                if (File.Exists(path)) {
                                    Exception exc;
                                    if (!FileHelper.IsFileBeingUsed(path, out exc)) {
                                        AppDomain myDomain = AppDomain.CreateDomain("DatabaseManagement");
                                        myDomain.ExecuteAssembly(path);
                                        AppDomain.Unload(myDomain);
                                    }
                                } else {
                                    throw new Exception(ResourcesLogin.DatabaseManagementNotFound);
                                }
                            }
                            //MessageBox.Show(
                            //    ex.Message + Environment.NewLine,
                            //    ResourcesLogin.Hint,
                            //    MessageBoxButton.OK,
                            //    MessageBoxImage.Information);
                        } else {
                            MessageBox.Show(
                                ex.Message + Environment.NewLine,
                                ResourcesLogin.Error,
                                MessageBoxButton.OK,
                                MessageBoxImage.Error);
                        }
                        _keepAppConfig = false;
                        isDead = true;
                        Close();
                    }));
            }

            if (isDead) {
                return;
            }

            Dispatcher.Invoke(
                DispatcherPriority.Background,
                new Action(delegate {
                    try {
                        _progress.DialogResult = true;
                        //_progress = null;
                    } catch (Exception) {
                        Debug.Fail("Can't close the valid window (we are after catch(Exception))");
                    } finally {
                        _progress.Close();
                    }
                }));
        }
        #endregion Init

        #region Logon
        private void Logon() {
            
            Dispatcher.Invoke(
                DispatcherPriority.Background,
                new Action(delegate {
                    try {
                        User user = ((User) username.SelectedValue);
                        if (UserManager.Instance.Logon(user, password.Password) || 
                            (cbAutoLogin.IsChecked == true && UserManager.Instance.AutoLogin(user, UserConfig.PasswordHash))) {
                            // update and save user config

                            UserConfig.LastUser = user.UserName;

                            // use the built upgrade data context in UserManager.cs
                            UpgradeDataContext upgradeDataContext = user.UpgradeDataContext;
                            if (upgradeDataContext.Rows.Count > 0) {
                                // the welcome dialog.
                                DlgOpenResourcePdf dlgOpenResourcePdf = new DlgOpenResourcePdf
                                                                        {DataContext = upgradeDataContext};
                                dlgOpenResourcePdf.ShowDialog();
                                Debug.Assert(dlgOpenResourcePdf.cbKeepNextOpen.IsChecked != null, "dlgOpenResourcePdf.cbKeepNextOpen.IsChecked != null");
                                if (dlgOpenResourcePdf.cbKeepNextOpen.IsChecked.Value) {
                                    // user's last login only used in comparison of releases' available_from.
                                    // here we can overwrite the user's last login, so the update will be shown once more. 
                                    // The lastLogin will be less than the visible update's MIN(available_from)
                                    user.LastLogin = upgradeDataContext.PossibleLastLogin;
                                    UserManager.Instance.UpdateUser(user);
                                }
                            }

                            if (cbAutoLogin.IsChecked == true) {
                                UserConfig.PasswordHash = user.PasswordHash;
                            } else {
                                UserConfig.PasswordHash = null;
                            }

                            UserConfig.Save();

                            Window window = null;

                            try {
                                // open main window
                                window = new MainWindow();
                                window.Show();
                                ((MainWindow) window).Init();

                                // initialization succeed, close login window
                                _keepAppConfig = true;
                                Close();
                            } catch (Exception ex) {
                                if (window != null) window.Close();
                                throw new Exception(ex.Message, ex);
                            }
                        } else {
                            MessageBox.Show(ResourcesLogin.InvalidUserOrPassword);
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

        #endregion Logon

        private void cbAutoLogin_Unchecked(object sender, RoutedEventArgs e) {
            if (password.Password.Equals(((User)username.SelectedValue).PasswordHash)) {
                password.Password = string.Empty;
                UserConfig.PasswordHash = null;
            }
        }

        private void username_SelectionChanged(object sender, SelectionChangedEventArgs e) {            
            ((User) username.SelectedValue).AutoLogin = false;
            imgIsDomainUser.Visibility = ((User) username.SelectedValue).IsDomainUser
                                             ? Visibility.Visible
                                             : Visibility.Collapsed;
        }

    }
}