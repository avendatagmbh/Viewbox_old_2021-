// -----------------------------------------------------------
// Created by Benjamin Held - 26.07.2011 13:55:04
// Copyright AvenDATA 2011
// -----------------------------------------------------------
using System;
using System.Globalization;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using DatabaseManagement.DbUpgrade;
using DatabaseManagement.Manager;
using DatabaseManagement.Models;
using DatabaseManagement.Windows;
using DbAccess.Structures;
using eBalanceKitBase;
using eBalanceKitBase.Windows;

namespace DatabaseManagement.Controls {
    /// <summary>
    /// Interaktionslogik für CtlBackup.xaml
    /// </summary>
    public partial class CtlBackup : UserControl {
        public CtlBackup() {
            InitializeComponent();
        }
        MainWindowModel Model { get { return DataContext as MainWindowModel; } }
        DlgProgress dlgProgress = null;

        private void CreateBackup_Click(object sender, RoutedEventArgs e) {
            DlgCreateBackup dlgCreateBackup = new DlgCreateBackup(this.Model.Owner) { DataContext = this.DataContext };
            dlgCreateBackup.ShowDialog();
        }

        private void ChooseDirectory_Click(object sender, RoutedEventArgs e) {
            System.Windows.Forms.FolderBrowserDialog objDialog = new System.Windows.Forms.FolderBrowserDialog();
            objDialog.Description = "Bitte wählen Sie ein Verzeichnis zum Speichern der Datenbank-Backups.";
            objDialog.SelectedPath = Model.BackupConfig.BackupDirectory;
            System.Windows.Forms.DialogResult result = objDialog.ShowDialog(WpfExtension.GetIWin32Window(this.Model.Owner));
            if (result == System.Windows.Forms.DialogResult.OK) {
                this.Model.BackupConfig.BackupDirectory = objDialog.SelectedPath;
                try {
                    Manager.DatabaseManager.DatabaseConfig.BackupDirectory = this.Model.BackupConfig.BackupDirectory;
                    Manager.DatabaseManager.DatabaseConfig.Save();
                } catch (Exception) {
                    throw new Exception("Das Config-Datei konnte nicht aktualisiert werden - das Backup Verzeichnis wird nicht gespeichert.");
                }
            }
        }

        private void RestoreBackup_Click(object sender, RoutedEventArgs e) {
            string selectedFile = (string)this.Model.BackupConfig.SelectedFile;
            if (string.IsNullOrEmpty(selectedFile)) {
                MessageBox.Show("Keine Datei zum importieren ausgewählt.");
                return;
            }
            if (MessageBox.Show(this.Model.Owner, "Achtung: Bei dieser Aktion wird die aktuelle Datenbank gelöscht " +
                "und das ausgewählte Backup eingespielt. Sind Sie sicher, dass Sie den Vorgang fortsetzen möchten?","",
                MessageBoxButton.YesNo, MessageBoxImage.Warning, MessageBoxResult.No) == MessageBoxResult.No)
                return;
            
          
            DatabaseManagement.DbUpgrade.eBalanceBackup.UserInfo userInfo = new DbUpgrade.eBalanceBackup.UserInfo();
            string filename = this.Model.BackupConfig.BackupDirectory + selectedFile;
            
            eBalanceBackup backup = new eBalanceBackup();
            try {
                userInfo = backup.ReadInformation(filename);
            } catch (Exception ex) {
                MessageBox.Show(this.Model.Owner, "Die Datenbank konnte nicht importiert werden: " + ex.Message, "", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            backup.Error += new EventHandler<System.IO.ErrorEventArgs>(backup_Error);
            backup.Progress += new EventHandler(backup_Progress);
            backup.Finished += new EventHandler(backup_Finished);
            dlgProgress = new DlgProgress(Model.Owner);

            dlgProgress.ProgressInfo.Value = 0;
            dlgProgress.ProgressInfo.Maximum = DatabaseCreator.Instance.GetTableCount(VersionInfo.Instance.GetLastDbVersion(userInfo.DbVersion));
            dlgProgress.ProgressInfo.Caption = "Importiere Tabellen.";

            try {
                Thread t = new Thread(backup.ImportDatabase){CurrentCulture = Thread.CurrentThread.CurrentCulture, CurrentUICulture = Thread.CurrentThread.CurrentUICulture};
                t.Start(new Tuple<DbConfig,string>(DatabaseManager.DatabaseConfig.GetDbConfig(), filename));
                dlgProgress.ShowDialog();
            } catch (Exception ex) {
                MessageBox.Show(this.Model.Owner, "Die Datenbank konnte nicht importiert werden: " + ex.Message, "", MessageBoxButton.OK,MessageBoxImage.Error);
            }
        }

        void backup_Finished(object sender, EventArgs e) {
            dlgProgress.ProgressInfo.CloseParent();
            this.Dispatcher.Invoke(
                System.Windows.Threading.DispatcherPriority.Background,
                new Action(delegate {
                Model.DatabaseMayHaveChanged();
                MessageBox.Show(this.Model.Owner, "Das Datenbank-Backup wurde erfolgreich importiert.", "",
                    MessageBoxButton.OK, MessageBoxImage.Information);                  
            }));
        }

        void backup_Progress(object sender, EventArgs e) {
            dlgProgress.ProgressInfo.Value = dlgProgress.ProgressInfo.Value + 1;
        }

        void backup_Error(object sender, System.IO.ErrorEventArgs e) {
            dlgProgress.ProgressInfo.CloseParent();
            this.Dispatcher.Invoke(
                System.Windows.Threading.DispatcherPriority.Background,
                new Action(delegate {
                    Model.DatabaseMayHaveChanged();
                    MessageBox.Show(this.Model.Owner, "Die Datenbank konnte nicht importiert werden: " + e.GetException().Message, "", MessageBoxButton.OK, MessageBoxImage.Error);
            }));
        }

        private void button1_Click(object sender, RoutedEventArgs e) {

            string selectedFile = (string)this.Model.BackupConfig.SelectedFile;
            DatabaseManagement.DbUpgrade.eBalanceBackup.UserInfo userInfo = new DbUpgrade.eBalanceBackup.UserInfo();
            string filename = this.Model.BackupConfig.BackupDirectory + selectedFile;

            eBalanceBackup backup = new eBalanceBackup();
            try {
                userInfo = backup.ReadInformation(filename);
            }
            catch (Exception ex) {
                MessageBox.Show(this.Model.Owner, "Die Datenbank konnte nicht importiert werden: " + ex.Message, "", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            backup.UncryptDatabase(DatabaseManager.DatabaseConfig.GetDbConfig(), filename);
        }
    }
}
