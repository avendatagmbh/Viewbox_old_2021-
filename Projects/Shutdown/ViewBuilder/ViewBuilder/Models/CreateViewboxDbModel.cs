using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using DbAccess;
using DbAccess.Structures;
using Utils;
using ViewBuilder.Windows;
using ViewBuilderBusiness.Structures.Config;

namespace ViewBuilder.Models {
    class CreateViewboxDbModel {
        #region Constructor

        public CreateViewboxDbModel(ProfileConfig profile) {
            Profile = profile;
            //ViewboxDbName = profile.ViewboxDbName;
            SystemDbName = Profile.DbConfig.DbName + "_system";

            if (Profile.ViewboxDb.Objects.All(o => o.Database.ToLower() != Profile.DbConfig.DbName.ToLower()))
                HeaderText = "Das System ist in der Viewbox-Datenbank noch\nnicht verzeichnet. Sie können es nun zur\nViewbox-Datenbank hinzufügen.";
            else {
                using (var db = Profile.ViewboxDb.ConnectionManager.GetConnection()) {
                    if (db.DatabaseExists(Profile.ViewboxDbName)) HeaderText = "Die Viewbox-Datenbank existiert bereits!";
                    else HeaderText = "Die Viewbox-Datenbank existiert noch nicht!";
                }
            }
        }
        #endregion Constructor

        #region Properties
        public ProfileConfig Profile { get; set; }
        public string HeaderText { get; set; }
        public string ViewboxDbName { get { return Profile.ViewboxDbName; } }
        public string SystemDbName { get; set; }
        public bool ReorderTables { get; set; }
        public bool CheckDatatypes { get; set; }
        private PopupProgressBar _progressDialog;
        private Window _parent;
        #endregion Properties

        #region Methods
        #endregion Methods

        public void CreateViewboxDb(DlgCreateViewboxDb dlgCreateViewboxDb) {
            _parent = dlgCreateViewboxDb;
            if (string.IsNullOrEmpty(ViewboxDbName)) {
                MessageBox.Show(dlgCreateViewboxDb, "Bitte einen Namen für die Viewbox Datenbank eingeben");
                return;
            }

            //Check if user wishes to overwrite if necessary
            bool dropDatabase = false;
            using (IDatabase conn = Profile.ConnectionManager.GetConnection()) {
                if (conn.DatabaseExists(ViewboxDbName) && conn.TableExists(ViewboxDbName,"users")) {
                    //var messageBox = MessageBox.Show(dlgCreateViewboxDb,
                    //                                 string.Format("Die Datenbank \"{0}\" existiert bereits. Wie möchten Sie fortfahren? Drücken Sie Ja für Viewbox-Datenbank überschreiben, Nein für System zur Viewbox-Datenbank hinzufügen.",ViewboxDbName), "", MessageBoxButton.YesNoCancel);
                    var messageBox = MessageBox.Show(dlgCreateViewboxDb,
                                                     string.Format("The database \"{0}\" already exists. Would you like to continue? Press YES for recreating Viewbox database, NO for adding new system / or merge and existing system into the Viewbox database.", ViewboxDbName), "", MessageBoxButton.YesNoCancel);
                    if (messageBox == MessageBoxResult.Cancel) return;
                    dropDatabase = messageBox == MessageBoxResult.Yes;
                }
            }
            ProgressCalculator progress = new ProgressCalculator();
            progress.Title = "Erstelle Viewbox Datenbank";
            progress.DoWork += progress_DoWork;
            progress.RunWorkerCompleted += progress_RunWorkerCompleted;
            _progressDialog = new PopupProgressBar() {DataContext = progress, Owner=dlgCreateViewboxDb};
            progress.RunWorkerAsync(dropDatabase);
            _progressDialog.ShowDialog();
        }

        void progress_RunWorkerCompleted(object sender, System.ComponentModel.RunWorkerCompletedEventArgs e) {
            if (e.Error != null)
                MessageBox.Show(_parent,
                                "Beim Erstellen der Viewbox Datenbank ist ein Fehler aufgetreten: " +
                                Environment.NewLine + e.Error.Message, "", MessageBoxButton.OK, MessageBoxImage.Error);
            else MessageBox.Show(_parent, "Viewboxdatenbank erfolgreich erstellt", "", MessageBoxButton.OK, MessageBoxImage.Information);
            _progressDialog.Close();
        }

        void progress_DoWork(object sender, System.ComponentModel.DoWorkEventArgs e) {
            var dropDatabase = (bool) e.Argument;

            #region Progressbar logic

            var progressBar = (ProgressCalculator) sender;
            var stepCount = 0;
            const int progressBarInterval = 100;
            // adds a step to the progressbar
            Action addProgressBarStep = () =>
                {
                    if (stepCount%progressBarInterval == 0) progressBar.StepDone();
                    stepCount++;
                };
            // set the progressbar a relative total step count
            var progressBarStepMaxCount = (Profile.ViewboxDb.Objects.Count/progressBarInterval) + 2 +
                                          (CheckDatatypes ? 1 : 0) +
                                          (ReorderTables ? 1 : 0);
            progressBar.SetWorkSteps(progressBarStepMaxCount, false);

            #endregion

            using (IDatabase conn = Profile.ConnectionManager.GetConnection()) {
                if (dropDatabase) {
                    conn.DropDatabase(ViewboxDbName);
                    conn.CreateDatabase(ViewboxDbName);
                }
            }
            progressBar.Description = "Lade Tabellen";
            using (IDatabase viewboxConn = Profile.ViewboxDb.ConnectionManager.GetConnection())
                Profile.ViewboxDb.LoadTables(viewboxConn);

            var highestId = !dropDatabase && Profile.ViewboxDb.Objects.Count > 0 ? Profile.ViewboxDb.Objects.Max(o => o.Id) : 0;

            progressBar.StepDone();
            progressBar.Description = "Importiere Systemdatenbank";
            Profile.ViewboxDb.ImportSystemDb(SystemDbName);
            progressBar.StepDone();
            if (CheckDatatypes) {
                progressBar.Description = "Checke Datentypen auf Konsistenz";
                Profile.ViewboxDb.CheckDatatypes(highestId);
                progressBar.StepDone();
            }
            if (ReorderTables) {
                progressBar.Description = "Order-Area aufbauen";
                Profile.ViewboxDb.ReorderTables(highestId);
                progressBar.StepDone();
            }
            //Creates info table with correct version
            using (IDatabase viewboxConn = Profile.ViewboxDb.ConnectionManager.GetConnection())
                Profile.ViewboxDb.CreateInfoTable(viewboxConn);

            // Creates indexes
            progressBar.Description = "Indizes Erstellen";
            using (IDatabase viewboxConn = Profile.ViewboxDb.ConnectionManager.GetConnection()) {
                viewboxConn.SetHighTimeout();
                Profile.ViewboxDb.DropIndexes(viewboxConn);
                Profile.ViewboxDb.PopulateWithIndexes(viewboxConn, addProgressBarStep);
                Profile.ViewboxDb.LoadIndexesObjects(viewboxConn);
            }
        }
    }
}
