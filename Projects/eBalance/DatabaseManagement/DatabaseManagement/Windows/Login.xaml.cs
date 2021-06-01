using System;
using System.Globalization;
using System.Threading;
using System.Windows;
using System.Windows.Input;
using System.Collections.Generic;
using DbAccess;
using System.Collections.ObjectModel;
using Utils;
using eBalanceKitBase;
using DatabaseManagement.Manager;

namespace DatabaseManagement.Windows {

    public partial class Login : Window {

        DatabaseConfig databaseConfig = new DatabaseConfig();

        #region constructor
        public Login() {
            InitializeComponent();
            this.txtVersion.Text = "Version " +
                System.Reflection.Assembly.GetEntryAssembly().GetName().Version.Major + "." +
                System.Reflection.Assembly.GetEntryAssembly().GetName().Version.Minor +
                (System.Reflection.Assembly.GetEntryAssembly().GetName().Version.Build > 0 ? "." + System.Reflection.Assembly.GetEntryAssembly().GetName().Version.Build : "");

            // set initial focus to password
            dbPassword.Focus();

            try {
                databaseConfig.LoadConfig();
                //For MySQL and SQLServer we need to create the database if it does not exist
                if (databaseConfig.DbConfig.DbType == "MySQL" || databaseConfig.DbConfig.DbType == "SQLServer") {
                    DbAccess.Structures.DbConfig dbConfig = new DbAccess.Structures.DbConfig(databaseConfig.DbConfig.DbType) {
                        Username = databaseConfig.DbConfig.Username,
                        Password = databaseConfig.DbConfig.Password,
                        Hostname = databaseConfig.DbConfig.Hostname
                    };

                    // create database if it does not yet exist
                    using (IDatabase conn = ConnectionManager.CreateConnection(dbConfig)) {
                        conn.Open();
                        conn.CreateDatabaseIfNotExists(databaseConfig.DbConfig.DbName);
                    }
                }
            } catch (Exception) {
                MessageBox.Show(this, "Die Datenbank Konfiguration konnte nicht gelesen werden, bitte führen Sie zunächst das eBilanz-Kit Config Tool aus.");
                this.Close();
            }
            try {
                DatabaseManager.Init(databaseConfig);
            } catch (Exception) {
                MessageBox.Show(this, "Die Verbindung zur Datenbank konnte nicht hergestellt werden.");
                this.Close();
            }

        }
        #endregion constuctor

        //        #region event handler

        #region btnOk_Click
        private void btnOk_Click(object sender, RoutedEventArgs e) {
            btnOk.IsEnabled = false;
            Logon();

        }
        #endregion

        #region Logon
        private void Logon() {
            //Window window = null;

            if (dbPassword.Password == databaseConfig.GetDbConfig().Password) {
                try {
                    new MainWindow().Show();
                    Close();

                } catch (Exception ex) {
                    throw new Exception(ex.Message, ex);
                }

            } else {
                MessageBox.Show(this, "Das Datenbank Passwort ist ungültig.", "", MessageBoxButton.OK, MessageBoxImage.Error);
                dbPassword.SelectAll();
                dbPassword.Focus();
                btnOk.IsEnabled = true;
            }

        }
        #endregion

        private void dbPassword_KeyDown(object sender, KeyEventArgs e) {
            if (e.Key == Key.Return) {
                e.Handled = true;
                btnOk.IsEnabled = false;
                Logon();
            }

            if (e.Key == Key.Escape) {
                e.Handled = true;
                this.Close();
            }

        }

        private void Window_Loaded(object sender, RoutedEventArgs e) {
            if (databaseConfig.DbConfig.DbType == "SQLite") {
                new MainWindow().Show();
                Close();
            }
        }
    }
}