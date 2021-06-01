using System;
using System.Collections.Generic;
using Business.CsvImporter.Events;
using Business.CsvImporter.Specific;
using Business.CsvImporter.Structures;
using Business.Interfaces;
using Business.Structures;
using Config.Config;
using Config.Interfaces.Config;
using Config.Structures;
using DbAccess;
using DbAccess.Structures;
using DbImport.CsvImport.CsvImporter;

namespace Business.CsvImporter {

    /// <summary>
    /// Base class for the csv-import. This class initiates the import process of all specified tables. 
    /// Alternatively it is possible to import all csv-files in the configurated directory.
    /// </summary>
    public class CsvImport {

        #region constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="CsvImport"/> class.
        /// </summary>
        public CsvImport(StateCsvImport state, CsvInputConfig config, IDatabaseOutputConfig outputConfig) {
            this.CsvInputConfig = config;
            OutputConfig = outputConfig;
            // select the CSV importer, depending on the configurated CsvStyle
            if (config.IsSapCsv)
                CsvImporter = new SapAbap(state, config, outputConfig);
            else 
                CsvImporter = new Default(state, config, outputConfig);

            this.CsvImporter.Log += CsvImporter_Log;
            this.CsvImporter.Info += CsvImporter_Info;
            this.CsvImporter.Warn += CsvImporter_Warn;
            this.CsvImporter.Error += CsvImporter_Error;
            //this.CsvImporter.UpdateState += CsvImporter_UpdateState;
        }

        #endregion constructors

        ///////////////////////////////////////////////////////////////////////

        #region properties
        private IDatabaseOutputConfig OutputConfig { get; set; }
        private CsvInputConfig CsvInputConfig { get; set; }
        private ICsvImporter CsvImporter { get; set; }
        private bool IsCancelled { get; set; }

        #endregion properties

        ///////////////////////////////////////////////////////////////////////

        #region events

        /// <summary>
        /// Occurs when the process has been finished.
        /// </summary>
        public event EventHandler Finished;

        /// <summary>
        /// Occurs when the process has been cancelled.
        /// </summary>
        public event EventHandler Cancelled;

        /// <summary>
        /// Occurs when a log message should be send to the GUI.
        /// </summary>
        public event EventHandler<CsvImportMessageEventArgs> Log;

        /// <summary>
        /// Occurs when an info message should be send to the GUI.
        /// </summary>
        public event EventHandler<CsvImportMessageEventArgs> Info;

        /// <summary>
        /// Occurs when a warning message should be send to the GUI.
        /// </summary>
        public event EventHandler<CsvImportMessageEventArgs> Warn;

        /// <summary>
        /// Occurs when an error occurs.
        /// </summary>
        public event EventHandler<CsvImportMessageEventArgs> Error;

        #endregion events

        ///////////////////////////////////////////////////////////////////////

        #region event-handler
        
        /// <summary>
        /// Called when the process has been finished.
        /// </summary>
        private void OnFinished() {
            if (Finished != null) Finished(this, new EventArgs());
        }

        /// <summary>
        /// Called when the process has been started.
        /// </summary>
        private void OnCancelled() {
            if (Cancelled != null) Cancelled(this, new EventArgs());
        }

        /// <summary>
        /// Called when an error occurs.
        /// </summary>
        /// <param name="message">The message.</param>
        private void OnError(string message) {
            if (Error != null) Error(this, new CsvImportMessageEventArgs("", "", message));
        }

        /// <summary>
        /// Handles the Log event of the CsvImporter control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="CsvImportMessageEventArgs"/> instance containing the event data.</param>
        void CsvImporter_Log(object sender, CsvImportMessageEventArgs e) {
            if (Log != null) Log(this, e);
        }

        /// <summary>
        /// Handles the Info event of the CsvImporter control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="Avd.Config.Events.MessageEventArgs"/> instance containing the event data.</param>
        void CsvImporter_Info(object sender, CsvImportMessageEventArgs e) {
            if (Info != null) Info(this, e);
        }

        /// <summary>
        /// Handles the Warning event of the CsvImporter control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="CsvImportMessageEventArgs"/> instance containing the event data.</param>
        void CsvImporter_Warn(object sender, CsvImportMessageEventArgs e) {
            if (Warn != null) Warn(this, e);
        }

        /// <summary>
        /// Handles the Error event of the CsvImporter control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="Avd.Config.Events.MessageEventArgs"/> instance containing the event data.</param>
        void CsvImporter_Error(object sender, CsvImportMessageEventArgs e) {
            if (Error != null) Error(this, e);
        }

        #endregion event-handler

        ///////////////////////////////////////////////////////////////////////

        #region methods

        /// <summary>
        /// Cancells the import process.
        /// </summary>
        public void Cancel() {
            this.IsCancelled = true;
        }

        /// <summary>
        /// Starts importing all specified csv-files.
        /// </summary>
        public void Start(List<CsvTableInfo> tables, ITransferProgress transferProgress, CancelObject cancelObject) {
            
            //this.Config.Watchdog.Start();
            CsvImporter.Error += Error;
            try {
                // create destination database
                if (!CreateDestDatabase()) {
                    // destination database could not be created
                    OnError(string.Format("Die Zieldatenbank \"{0}\" konnte nicht erstellt werden", OutputConfig.DbConfig.DbName));
                    OnCancelled();
                    return;
                }

                CsvImporter.Init();

                // import tables
                foreach (var tableInfo in tables) {
                    if (cancelObject.Cancelled) {
                        OnCancelled();
                        break;
                    }
                    try {
                        transferProgress.AddProcessedEntity(tableInfo.Table);
                        if (CsvImporter.ImportTable(tableInfo)) {
                            tableInfo.Table.TransferState.State = TransferStates.TransferedOk;
                            tableInfo.Table.DoExport = false;
                            tableInfo.Table.TransferState.Message = "Tabelle wurde korrekt transferiert";
                        }

                    } catch (Exception ex) {
                        tableInfo.Table.TransferState.State = TransferStates.TransferedError;
                        tableInfo.Table.TransferState.Message = "Unerwarteter Fehler beim CsvImport: " +
                                                                Environment.NewLine + ex.Message;
                        OnError("Unerwarteter Fehler beim CsvImport: " + Environment.NewLine + ex.Message);
                    } finally {
                        tableInfo.Table.Profile.SaveTable(tableInfo.Table);
                        transferProgress.RemoveProcessedEntity(tableInfo.Table);
                    }
                }
                // import succeed
                //this.Config.Watchdog.SendApplicationFinishedMail();
                OnFinished();

            } catch (Exception ex) {
                OnError("Unerwarteter Fehler beim CsvImport: " + Environment.NewLine + ex.Message);
                OnCancelled();
            }
           
            //this.Config.Watchdog.Stop();

        }

        /// <summary>
        /// Creates the destination database.
        /// </summary>
        private bool CreateDestDatabase() {
            try {
                // create database connection
                DbConfig dbConfig = OutputConfig.DbConfig;
                string dbName = dbConfig.DbName;
                dbConfig.DbName = "";
                using (IDatabase conn = ConnectionManager.CreateConnection(dbConfig)) {
                    conn.Open();

                    // create database if it does not yet exist
                    conn.CreateDatabaseIfNotExists(dbName);
                }

            } catch (Exception ex) {
                OnError("Fehler beim Erstellen der Zieldatenbank: " + Environment.NewLine + ex.Message);
                return false;
            
            }

            return true;
        }

        #endregion methods
    }
}
