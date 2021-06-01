using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using Business.CsvImporter;
using Business.CsvImporter.Events;
using Business.CsvImporter.Structures;
using Business.Structures.MetadataAgents;
using Config.Config;
using Config.Interfaces.Config;
using Config.Interfaces.DbStructure;
using Config.Structures;
using DbAccess;
using System.Linq;
using Ionic.Zip;

namespace DbImport.CsvImport {    

    /// <summary>
    /// This is the base class for the import process of a table. The subnamespace CsvImporters contains a derived
    /// class for each csv-style. These classes contain style-specific adaptions for the import process, wheras this
    /// class provides the base functionality, which is equal for all csv-styles.
    /// </summary>
    internal abstract class CsvImporterBase : ICsvImporter {
        
        #region constructors
        
        protected CsvImporterBase(StateCsvImport state, CsvInputConfig csvInputConfig, IDatabaseOutputConfig outputConfig) {
            this.State = state;
            this.CsvInputConfig = csvInputConfig;
            OutputConfig = outputConfig;
        }
         
        #endregion constructors

        ///////////////////////////////////////////////////////////////////////

        #region properties

        protected IDatabaseOutputConfig OutputConfig { get; set; }
        protected CsvInputConfig CsvInputConfig { get; set; }
        
        /// <summary>
        /// Gets or sets a value indicating whether this <see cref="CsvImport"/> is cancelled.
        /// </summary>
        /// <value><c>true</c> if cancelled; otherwise, <c>false</c>.</value>
        protected bool IsCancelled { get; set; }

        /// <summary>
        /// Gets or sets the state.
        /// </summary>
        /// <value>The state.</value>
        protected StateCsvImport State { get; set; }

        #endregion properties

        ///////////////////////////////////////////////////////////////////////

        #region events

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

        /// <summary>
        /// Occurs when the state is updated.
        /// </summary>
        //public event EventHandler<CsvImportStateEventArgs> UpdateState;

        #endregion events

        ///////////////////////////////////////////////////////////////////////

        #region event-handler

        /// <summary>
        /// Called when a log message should be send to the GUI.
        /// </summary>
        /// <param name="message">The message.</param>
        protected void OnLog(string tableName, string fileName, string message) {
            if (Log != null) Log(this, new CsvImportMessageEventArgs(tableName, fileName, message));
        }

        /// <summary>
        /// Called when an info message should be send to the GUI.
        /// </summary>
        /// <param name="message">The message.</param>
        protected void OnInfo(string tableName, string fileName, string message) {
            if (Info != null) Info(this, new CsvImportMessageEventArgs(tableName, fileName, message));
        }
        
        /// <summary>
        /// Called when a warning message should be send to the GUI.
        /// </summary>
        /// <param name="message">The message.</param>
        protected void OnWarn(string tableName, string fileName, string message) {
            if (Warn != null) Warn(this, new CsvImportMessageEventArgs(tableName, fileName, message));
        }

        /// <summary>
        /// Called when an error occurs.
        /// </summary>
        /// <param name="message">The message.</param>
        protected void OnError(string tableName, string fileName, string message) {
            if (Error != null) Error(this, new CsvImportMessageEventArgs(tableName, fileName, message));
        }

        /// <summary>
        /// Called when the state is updated.
        /// </summary>
        //protected void OnUpdateState(TableState_CsvImport state) {
        //    if (UpdateState != null) UpdateState(this, new CsvImportStateEventArgs(state));
        //}
        
        #endregion event-handler

        ///////////////////////////////////////////////////////////////////////

        #region methods

        public static IEnumerable<string> GetFieldNamesFromCsv(string filename, ICsvInputConfig csvInputConfig) {
            string removeFile = string.Empty;
            string header = string.Empty;
            FileInfo fi = new FileInfo(filename);

            if(filename.ToLower().EndsWith(".zip")) {
                ZipFile zip = new ZipFile(filename);
                zip.ExtractAll(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData));
                filename = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\" + fi.Name.Substring(0, fi.Name.Length - 4); ;
                removeFile = filename;
            }

            //Stopwatch sw0 = Stopwatch.StartNew();
            //// read the header
            //using (var sReader = new StreamReader(filename))
            //{
            //    var sLine = new StringBuilder();
            //    while (!sLine.ToString().EndsWith(csvInputConfig.GetLineEndSeperator()))
            //    {
            //        int nextChar = sReader.Read();
            //        //Read to end
            //        if (nextChar == -1)
            //            break;
            //        sLine.Append((char)nextChar);
            //        if (sLine.Length > 1000000)
            //            throw new Exception("Unter den ersten 1000000 Zeichen wurde nicht das Ende der Zeile gefunden");
            //    }

            //    header = sLine.ToString().Substring(0, sLine.Length - csvInputConfig.GetLineEndSeperator().Length);
            //    header = header.Trim();
            //}
            //sw0.Stop();
            ////998, 415,433 millisec

            // DEVNOTE: this approach is much more quicker on network !
            //Stopwatch sw = Stopwatch.StartNew();
            using (StreamReader streamReader = new StreamReader(filename))
            {
                StringBuilder stringBuilder = new StringBuilder();
                //int bufferSize = 1024 * 64; // worst case
                int bufferSize = 1024 * 4;
                string endSeparator = csvInputConfig.GetLineEndSeperator();
                char[] fileContents = new char[bufferSize];
                int readCount = 0;
                int charsRead = 0;

                while (string.IsNullOrEmpty(header))
                {
                    charsRead = streamReader.Read(fileContents, 0, bufferSize);
                    if (charsRead <= 0) break;
                    readCount += charsRead;
                    stringBuilder.Append(fileContents);
                    if (DetectEndSeparator(stringBuilder, ref header, endSeparator)) break;
                    if (readCount > 1000000)
                        throw new Exception("Unter den ersten 1000000 Zeichen wurde nicht das Ende der Zeile gefunden");
                }
            }
            //sw.Stop();

            if(!string.IsNullOrEmpty(removeFile)) File.Delete(removeFile);
            IEnumerable<string> fileNames = header.Split(new string[] { csvInputConfig.GetFieldSeperator() }, StringSplitOptions.None).Select(h => h.Trim());
            return fileNames;
        }

        private static bool DetectEndSeparator(StringBuilder stringBuilder, ref string header, string endSeparator) {
            string data = stringBuilder.ToString();
            int index = data.IndexOf(endSeparator, StringComparison.InvariantCulture);
            if (index >= 0) {
                header = data.Substring(0, index);
                return true;
            }
            return false;
        }

        public virtual bool ImportTable(CsvTableInfo tableInfo) {
            System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();
            sw.Start();

            List<string> missingFiles = CheckFiles(tableInfo);
            if (missingFiles.Count > 0) {
                string errMsg = "Import wegen fehlender CSV-Dateien nicht möglich:";
                foreach (string missingFile in missingFiles) {
                    errMsg += Environment.NewLine + missingFile;
                }
                throw new FileNotFoundException(errMsg);
            }

            // create database connection
            using (IDatabase db = ConnectionManager.CreateConnection(OutputConfig.DbConfig)) {
                db.Open();

                // drop table, if it already exists
                db.DropTableIfExists(tableInfo.TableName);
                if (tableInfo.Filenames.Count == 1)
                    ImportFile(db, tableInfo.TableName, tableInfo.GetPath(tableInfo.Filenames[0]));
                else {
                    string headerFile =
                        tableInfo.Filenames.FirstOrDefault(
                            (name) => name.EndsWith("_0.csv", StringComparison.InvariantCultureIgnoreCase));
                    if (headerFile == null)
                        throw new InvalidOperationException(
                            string.Format("Die Datei \"{0}\" mit den Spaltenüberschriften existiert nicht",
                                          MetadataAgentCsv.ReplacePartNumber(tableInfo.Filenames[0], 0)));

                    CreateTable(db, tableInfo.TableName, headerFile);
                    foreach (string filename in tableInfo.Filenames)
                        LoadData(db, tableInfo.TableName, tableInfo.GetPath(filename),
                                    filename == headerFile ? 1 : 0);
                }
                // validate imported table
                long destCount;
                try {
                    destCount = db.CountTable(tableInfo.TableName);
                } catch (Exception) {
                    destCount = db.CountTable(tableInfo.TableName.Replace("___SLASH___", "_"));
                }
                return Validate(destCount, tableInfo.Table);
            }
            //return true;
        }

        public virtual void Init() {
            //Nothing to do
        }

        protected virtual bool Validate(long destCount, ITable table) {
            if (destCount != table.Count) {
                table.TransferState.State = TransferStates.TransferedCountDifference;
                table.TransferState.Message =
                    string.Format(
                        "Anzahl der Zeilen weicht. Die CSV Datei hat {0} Einträge, die Datenbank-Tabelle hat {1} Einträge.",
                        table.Count, destCount);
                return false;
            }
            return true;
        }

        protected virtual List<string> CheckFiles(CsvTableInfo tableInfo) {

            List<string> missingFiles = new List<string>();

            foreach (var filename in tableInfo.Filenames) {

                if (!System.IO.File.Exists(tableInfo.GetPath(filename))) {
                    missingFiles.Add(filename);
                }
            }

            return missingFiles;
        }

        /// <summary>
        /// Imports the specified csv file.
        /// </summary>
        /// <param name="db">The db.</param>
        /// <param name="fileInfo">The file info.</param>
        protected virtual bool ImportFile(IDatabase db, string tableName, string filename) {
            //using (ISession session = ConfigCsvImport.DbPersist.OpenSession()) {
            //    FileInfo_CsvImport oFileInfo = ConfigCsvImport.DbPersist.GetFileInfo(session, id);

            //    this.CsvInputConfig.Watchdog.PingAlive("Importiere Tabelle " + tableName + " / Datei " + oFileInfo.Filename);
                
            //try {
                CreateTable(db, tableName, filename);
                LoadData(db, tableName, filename, 1);
                //this.State.IncProcessedBytes(oFileInfo.TotalSize);
                return true;

            //} catch (Exception ex) {
            //    OnInfo(tableName, "", "Import fehlgeschlagen.");
            //    OnError(filename, tableName, "Fehler beim Importieren:" + Environment.NewLine + ex.Message);
            //    System.Diagnostics.Debug.WriteLine(ex.Message);
            //    //this.State.IncProcessedBytes(oFileInfo.TotalSize);
            //    return false;
            //}                
            //}
        }

        /// <summary>
        /// Returns the filename of the first csv-part.
        /// </summary>
        /// <param name="fileName">Name of the file.</param>
        /// <returns></returns>
        //protected abstract string GetMainFilename(string fileName);

        //protected abstract void LoadData(IDatabase db, string tableName, string fileName, int ignoreLines);

        //protected abstract void CreateTable(IDatabase db, string tableName, string fileName);

        protected virtual void LoadData(IDatabase db, string tableName, string fileName, int ignoreLines) {
            string cmd = "";
            try {
                string enc = "utf8";
                if (!string.IsNullOrEmpty(CsvInputConfig.FileEncoding))
                    enc = CsvInputConfig.FileEncoding;

                cmd =
                    "LOAD DATA LOCAL INFILE '" + (fileName).Replace("\\", "/") + "'" +
                    " INTO TABLE " + tableName +
                    " CHARACTER SET " + enc +
                    " COLUMNS TERMINATED BY '" + CsvInputConfig.GetFieldSeperator() + "'" +
                    (string.IsNullOrEmpty(CsvInputConfig.OptionallyEnclosedBy) ? "" : (" OPTIONALLY ENCLOSED BY '" + CsvInputConfig.OptionallyEnclosedBy + "'")) +
                    " OPTIONALLY ENCLOSED BY '" + "\"" + "'" +
                    " ESCAPED BY ''" +
                    " LINES TERMINATED BY '" + CsvInputConfig.GetLineEndSeperator() + "'" +
                    " IGNORE " + ignoreLines + " LINES";
                db.ExecuteNonQuery(cmd);
            } catch (Exception ex) {
                throw new Exception("Fehler beim Absetzen des Befehls: " + cmd + Environment.NewLine + ex.Message);
            }
        }


        protected virtual void CreateTable(IDatabase db, string tableName, string fileName) {
            // Get column names and types (+lengths)

            try {
                IEnumerable<string> arrFieldNames = GetFieldNamesFromCsv(fileName, CsvInputConfig);


                // create table
                string sCommand = "CREATE TABLE IF NOT EXISTS " + tableName + " (";
                foreach (string sField in arrFieldNames) {
                    sCommand += db.Enquote(sField) + " TEXT,";
                }
                sCommand = sCommand.Remove(sCommand.Length - 1);
                sCommand += ") ENGINE = MyISAM;";

                db.ExecuteNonQuery(sCommand);

            } catch (Exception ex) {
                throw new Exception("Fehler beim Anlegen der Tabelle: " + Environment.NewLine + ex.Message);
            }
        }

        #endregion methods
    }
}
