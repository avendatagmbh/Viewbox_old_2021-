// --------------------------------------------------------------------------------
// author: Benjamin Held
// since:  2011-07-26
// copyright 2011 AvenDATA GmbH
// --------------------------------------------------------------------------------
using System;
using System.Windows;
using DatabaseManagement.DbUpgrade;
using DatabaseManagement.Manager;
using DatabaseManagement.Models;
using DbAccess;

namespace DatabaseManagement.Windows {
    public partial class DlgCreateBackup {
        public DlgCreateBackup(Window owner) {
            InitializeComponent();
            Owner = owner;
        }

        private MainWindowModel Model { get { return DataContext as MainWindowModel; } }

        private void BtnOkClick(object sender, RoutedEventArgs e) {
            var userInfo = new eBalanceBackup.UserInfo {Comment = textComment.Text};

            string filename = Model.BackupConfig.BackupDirectory + "eBalanceKitBackup_" +
                              DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss") + ".bak";
            var backup = new eBalanceBackup();
            try {
                using (IDatabase conn = DatabaseManager.ConnectionManager.GetConnection()) {
                    //conn.Open();
                    backup.ExportDatabase(conn, filename, userInfo, chkEncrypt.IsChecked.Value);
                }
                MessageBox.Show(Owner, "Die Datenbank wurde erfolgreich exportiert.", "", MessageBoxButton.OK,
                                MessageBoxImage.Information);
            } catch (Exception ex) {
                MessageBox.Show(Owner, "Die Datenbank konnte nicht exportiert werden: " + ex.Message, "",
                                MessageBoxButton.OK, MessageBoxImage.Error);
            }
            Close();
        }

        private void BtnCancelClick(object sender, RoutedEventArgs e) { Close(); }
    }
}