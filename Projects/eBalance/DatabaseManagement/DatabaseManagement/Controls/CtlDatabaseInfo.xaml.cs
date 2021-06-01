// -----------------------------------------------------------
// Created by Benjamin Held - 26.07.2011 13:55:08
// Copyright AvenDATA 2011
// -----------------------------------------------------------
using System;
using System.Windows;
using System.Windows.Controls;
using eBalanceKitBase.Windows;
using DatabaseManagement.DbUpgrade;
using System.Threading;
using eBalanceKitBase;
using DatabaseManagement.Models;

namespace DatabaseManagement.Controls {
    /// <summary>
    /// Interaktionslogik für CtlDatabaseInfo.xaml
    /// </summary>
    public partial class CtlDatabaseInfo : UserControl {
        public CtlDatabaseInfo() {
            InitializeComponent();
        }

        DatabaseInfoModel Model { get { return this.DataContext as DatabaseInfoModel; } }

        private void btnUpgradeDatabase_Click(object sender, RoutedEventArgs e) {
            Window owner = UIHelpers.TryFindParent<Window>(this);

            DlgProgress dlgProgress = new DlgProgress(owner);
            try {
                Upgrader upgrader = new Upgrader(dlgProgress.ProgressInfo);
                if (upgrader.UpgradeExists()) {
                    if (MessageBox.Show(owner, "Ihre Datenbank hat die Version " + upgrader.GetDatabaseVersion() + ", die aktuelle Version ist " + VersionInfo.Instance.CurrentDbVersion +
                        ". Möchten Sie die Datenbank auf den neuesten Stand bringen (es wird vorher ein Backup erstellt)?", ""
                        , MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes) {
                        upgrader.Finished += new EventHandler(upgrader_Finished);
                        upgrader.Error += new EventHandler<System.IO.ErrorEventArgs>(upgrader_Error);

                        Thread t = new Thread(upgrader.Upgrade) { CurrentCulture = Thread.CurrentThread.CurrentCulture, CurrentUICulture = Thread.CurrentThread.CurrentUICulture };
                        t.Start();
                        dlgProgress.ShowDialog();
                    }
                } else
                    MessageBox.Show(owner, "Die Datenbank ist auf dem neusten Stand.");
            } catch (Exception ex) {
                MessageBox.Show(owner, "Ein Fehler ist aufgetreten beim Testen der Datenbank-Version: " + ex.Message, "", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        void upgrader_Error(object sender, System.IO.ErrorEventArgs e) {
            this.Dispatcher.Invoke(
                System.Windows.Threading.DispatcherPriority.Background,
                new Action(delegate {
                    Model.DatabaseMayHaveChanged();
                    MessageBox.Show(UIHelpers.TryFindParent<Window>(this), "Das Datenbank Upgrade war nicht erfolgreich. " + e.GetException().Message, "", MessageBoxButton.OK, MessageBoxImage.Error);
            }));
        }

        void upgrader_Finished(object sender, EventArgs e) {
            this.Dispatcher.Invoke(
                System.Windows.Threading.DispatcherPriority.Background,
                new Action(delegate {
                    Model.DatabaseMayHaveChanged();

                    var upgrader = (Upgrader)sender;
                    if (VersionInfo.Instance.VersionToDouble(upgrader.PreviousVersion) < VersionInfo.Instance.VersionToDouble("1.4")) {
                        MessageBox.Show(UIHelpers.TryFindParent<Window>(this),
                                        "Das Datenbank Upgrade war erfolgreich." + Environment.NewLine + Environment.NewLine +
                                        "Aufgrund umfangreicher Erweiterungen am Berechtigungssystem konnten die vorhandenen Reportberechtigungen leider nicht übernommen werden. " + Environment.NewLine + Environment.NewLine +
                                        "Zur erneuten Vergabe der Berechtigungen müssen von einem Benutzer mit administrativen Berechtigungen neue Benutzerrollen mit entsprechenden Rechten " +
                                        "auf Unternehmens-, Geschäftsjahres oder Reportebene angelegt und den jeweiligen Nutzern zugewiesen werden. " +
                                        "Details hierzu entnehmen Sie bitte dem Benutzerhandbuch.", "", MessageBoxButton.OK,
                                        MessageBoxImage.Information);
                    } else {
                        MessageBox.Show(UIHelpers.TryFindParent<Window>(this), "Das Datenbank Upgrade war erfolgreich.", "", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
            }));
        }       
    }
}
