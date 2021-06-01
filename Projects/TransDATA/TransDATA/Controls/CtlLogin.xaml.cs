// --------------------------------------------------------------------------------
// author: Mirko Dibbert
// since:  2011-08-25
// copyright 2011 AvenDATA GmbH
// --------------------------------------------------------------------------------
using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using AV.Log;
using Base.Localisation;
using Business;
using Config.Interfaces.DbStructure;
using TransDATA.Models;
using TransDATA.Windows;
using log4net;

namespace TransDATA.Controls {
    /// <summary>
    /// Interaktionslogik für CtlLogin.xaml
    /// </summary>
    public partial class CtlLogin : UserControl {
        internal ILog _log = LogHelper.GetLogger();

        public CtlLogin() {
            InitializeComponent();

            if (AppController.IsInitialized) {
                password.Focus();

                // set auto login password
                foreach (String itm in Environment.GetCommandLineArgs().Where(itm => itm.ToLower() == "-autologin")) {
                    password.Password = "admin";
                }
            }
        }

        private LoginModel Model { get { return DataContext as LoginModel; } }

        private Window Owner {
            get { return UIHelpers.TryFindParent<Window>(this); }
        }

        private void BtnOkClick(object sender, RoutedEventArgs e) {
            Logon();
        }

        private void PasswordKeyDown(object sender, KeyEventArgs e) {
            if (e.Key == Key.Return) {
                e.Handled = true;
                Logon();
            }
        }

        private void UserControlPreviewKeyDown(object sender, KeyEventArgs e) {
            if (e.Key == Key.Escape) {
                e.Handled = true;
                Owner.Close();
            }
        }

        private void Logon() {
            if (username.SelectedItem != null && username.SelectedItem is IUser) {
                btnOk.IsEnabled = false;
                if (AppController.UserManager.Logon(username.SelectedItem as IUser, password.Password)) {
                    _log.ContextLog( LogLevelEnum.Info,"User succesfully logged on: {0}", ((IUser)username.SelectedItem).UserName);
                    new DlgMain(username.SelectedItem as IUser).Show();
                    Owner.Close();
                } else {
                    _log.ContextLog( LogLevelEnum.Info,"Unsuccessful logon: {0}", ((IUser)username.SelectedItem).UserName);
                    MessageBox.Show(ExceptionMessages.InvalidUserOrPassword);
                    password.Focus();
                    password.SelectAll();
                    btnOk.IsEnabled = true;
                }
            }
        }

        private void username_SelectionChanged(object sender, SelectionChangedEventArgs e) {
            if (username.SelectedItem != null && username.SelectedItem is IUser) {
                IUser user = username.SelectedItem as IUser;
                if(!user.IsInitialized) {
                    var dlg = new DlgInitPassword(new InitPasswordModel(user));
                    dlg.ShowDialog();
                }
            }
        }
    }
}