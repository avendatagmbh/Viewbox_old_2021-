// --------------------------------------------------------------------------------
// author: Benjamin Held, Mirko Dibbert
// since:  2011-06-26
// copyright 2011 AvenDATA GmbH
// --------------------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using DatabaseManagement.Localisation;
using DatabaseManagement.Manager;
using DbAccess;
using DbAccess.Structures;
using Taxonomy;
using Taxonomy.Enums;
using Taxonomy.Interfaces;
using Taxonomy.Interfaces.PresentationTree;
using Taxonomy.PresentationTree;
using eBalanceKitBase;
using eBalanceKitBase.Structures;

namespace DatabaseManagement.DbUpgrade {
    /// <summary>
    /// This class provides methods to upgrade a database to the current version.
    /// </summary>
    /// <author>Mirko Dibbert</author>
    /// <since>2011-05-22</since>
    public class Upgrader {
        private const string TaxonomyVersion = "2010-12-16";
        private readonly IDatabase _conn;

        #region constructor
        public Upgrader(ProgressInfo progress) {
            Progress = progress;
            _conn = DatabaseManager.ConnectionManager.GetConnection();
            Info = new Info(_conn);
        }
        #endregion

        #region events
        public event EventHandler Finished;
        private void OnFinished() { if (Finished != null) Finished(this, new EventArgs()); }

        public event EventHandler<ErrorEventArgs> Error;
        private void OnError(Exception ex) { if (Error != null) Error(this, new ErrorEventArgs(ex)); }
        #endregion

        #region properties
        private ProgressInfo Progress { get; set; }
        public string PreviousVersion { get; set; }
        #endregion properties

        #region methods
        public bool UpgradeExists() {
            string curVersion = Info.DbVerion;
            if (curVersion == null) {
                return false;
            }
            return VersionInfo.Instance.NewerDbVersionExists(curVersion);
        }

        public string GetDatabaseVersion() {
            string curVersion = Info.DbVerion;
            if (curVersion == null) {
                return VersionInfo.Instance.CurrentDbVersion;
            }
            return curVersion;
        }

        private void setInfoValue(IDatabase conn, string key, string value, bool setIdentity) {
            if (setIdentity && conn.DbConfig.DbType == "SQLServer") conn.ExecuteNonQuery("SET IDENTITY_INSERT dbo." + conn.Enquote("info") + " ON");
            Info.SetValue(key, value, conn);
            if (setIdentity && conn.DbConfig.DbType == "SQLServer") conn.ExecuteNonQuery("SET IDENTITY_INSERT dbo." + conn.Enquote("info") + " OFF");
        }

        private class TableStructure {
            public TableStructure(string tableName) {
                TableName = tableName;
                ColumnNames = new HashSet<string>();
            }
            public TableStructure(string tableName, IEnumerable<string> columns) {
                TableName = tableName;
                ColumnNames = columns;
            }
            public string TableName;
            public IEnumerable<string> ColumnNames;
        }

        private class TableCopyStructure {
            
            #region private constructor
            private TableCopyStructure(TableStructure source, TableStructure destination) {
                Source = source;
                Destination = destination;
                ColumnDict = new Dictionary<string, string>();
                TableDict = new Dictionary<string, string> { { source.TableName, destination.TableName } };
            }
            private TableCopyStructure(string sourceTableName, string destinationTableName) {
                Source = new TableStructure(sourceTableName);
                Destination = new TableStructure(destinationTableName);
                ColumnDict = new Dictionary<string, string>();
                TableDict = new Dictionary<string, string> { { sourceTableName, destinationTableName } };
            }
            private TableCopyStructure(string sourceTableName, IEnumerable<string> sourceColumns, string destinationTableName, IEnumerable<string> destinationColumns) {
                Source = new TableStructure(sourceTableName, sourceColumns);
                Destination = new TableStructure(destinationTableName, destinationColumns);
                ColumnDict = new Dictionary<string, string>();
                TableDict = new Dictionary<string, string> { { sourceTableName, destinationTableName } };

                for (int i = 0; i < sourceColumns.Count(); i++) {
                    ColumnDict.Add(sourceColumns.ToList()[i], destinationColumns.ToList()[i]);
                }
            }
            #endregion

            /// <summary>
            /// Structure that can be used to store the requiered informations to copy values between tables. The old table will be droped afterwards
            /// </summary>
            /// <param name="sourceTableName">Name of the old table that contains the ColumnChanges-Keys as columns</param>
            /// <param name="destinationTableName">Name of the new table that contains the ColumnChanges-Values as columns</param>
            /// <param name="ColumnChanges">An Dictionary that contains oldColumnName (or values that should be insert) as Keys and newColumName as Value</param>
            public TableCopyStructure(string sourceTableName, string destinationTableName, Dictionary<string, string> ColumnChanges) {
                Source = new TableStructure(sourceTableName, ColumnChanges.Keys);
                Destination = new TableStructure(destinationTableName, ColumnChanges.Values);
                ColumnDict = ColumnChanges;
                TableDict = new Dictionary<string, string> {{sourceTableName, destinationTableName}};
            }

            #region private props
            private TableStructure Source;
            private TableStructure Destination;
            #endregion

            #region public props
            /// <summary>
            /// Dictionary that contains oldColumnName as Keys and newColumName as Value
            /// </summary>
            public Dictionary<string, string> ColumnDict { get; private set; }
            /// <summary>
            /// Dictionary that contains oldTableName as Keys and newTableName as Value
            /// </summary>
            public Dictionary<string, string> TableDict { get; private set; }
            #endregion
        }


        ///// <summary>
        ///// Create a new table and insert the values from an old tables specified columns in the new tables specified column.
        ///// </summary>
        ///// <param name="copyStructure">A structure that contains (OldTableName, NewTableName) and (OldColumnNames, NewColumnNames)</param>
        ///// <returns>The new progress number. (progressStart + 3)</returns>
        //private int CreateNewTablesAndCopyDataFromAnOther(int progressStartStep, int progress, string key, string version,
        //                                              TableCopyStructure copyStructure, bool dropTableAfterFinished = true,
        //                                              bool setIdentity = true, bool setIdentityOnInit = true,
        //                                              string tableAlias = null) {

        //    return CreateNewTablesAndCopyDataFromAnOther(progressStartStep, progress, key, version,
        //                                                 new List<Dictionary<string, string>>() {copyStructure.TableDict},
        //                                                 new List<Dictionary<string, string>>() {copyStructure.ColumnDict}, 
        //                                                 dropTableAfterFinished,
        //                                                 setIdentity,
        //                                                 setIdentityOnInit, tableAlias);
        //}

        ///// <summary>
        ///// Create new tables and insert the values from the old tables specified columns in the new tables specified columns.
        ///// </summary>
        ///// <param name="copyStructure">A structure that contains (OldTableName, NewTableName) and (OldColumnNames, NewColumnNames)</param>
        ///// <returns>The new progress number. (progressStart + 3) * copyStructure.Count</returns>
        //private int CreateNewTablesAndCopyDataFromAnOther(int progressStartStep, int progress, string key, string version,
        //                                                      List<TableCopyStructure> copyStructure, bool dropTableAfterFinished = true,
        //                                                      bool setIdentity = true, bool setIdentityOnInit = true,
        //                                                      string tableAlias = null) {

        //    return CreateNewTablesAndCopyDataFromAnOther(progressStartStep, progress, key, version,
        //                                                 copyStructure.Select(x => x.TableDict),
        //                                                 copyStructure.Select(x => x.ColumnDict), dropTableAfterFinished,
        //                                                 setIdentity,
        //                                                 setIdentityOnInit, tableAlias);
        //}



        ///// <summary>
        ///// Create a new table and insert the values from an old tables specified columns in the new tables specified column.
        ///// </summary>
        ///// <param name="tableNames">A tables that should be changed (OldTableName, NewTableName)</param>
        ///// <param name="columns">A dictionary that contains OldColumnName, NewColumnName for the table</param>
        ///// <returns>The new progress number (progressStart + 3)</returns>
        //private int CreateNewTablesAndCopyDataFromAnOther(int progressStartStep, int progress, string key, string version,
        //                                              Dictionary<string, string> tableNames, Dictionary<string, string> columns,
        //    bool dropTableAfterFinished = true,
        //                                              bool setIdentity = true, bool setIdentityOnInit = true,
        //                                              string tableAlias = null) {
        //    return CreateNewTablesAndCopyDataFromAnOther(progressStartStep, progress, key, version,
        //                                                 new List<Dictionary<string, string>>() {tableNames},
        //                                                 new List<Dictionary<string, string>>() {columns}, dropTableAfterFinished, setIdentity,
        //                                                 setIdentityOnInit, tableAlias);
        //}

        ///// <summary>
        ///// Create new tables and insert the columns from old tables
        ///// </summary>
        ///// <param name="tableNames">A list of tables that should be changed (OldTableName, NewTableName)</param>
        ///// <param name="columns">A dictionary that contains OldColumnName, NewColumnName for each table</param>
        ///// <returns>The new progress number (progressStart + 4) * tableNames.Count()</returns>
        //private int CreateNewTablesAndCopyDataFromAnOther(int progressStartStep, int progress, string key, string version,
        //                                              IEnumerable<Dictionary<string, string>> tableNames, IEnumerable<Dictionary<string, string>> columns, 
        //                                                bool dropTableAfterFinished = true,
        //                                              bool setIdentity = true, bool setIdentityOnInit = true,
        //                                              string tableAlias = null) {
        //    int curProgress = progress;
        //    int progressStart = progressStartStep;
        //    const int increaseCounter = 4;
        //    int columnDictIterationCounter = 0;
        //    Dictionary<string, string> columnDict = columns.ToList()[columnDictIterationCounter];

        //    System.Diagnostics.Debug.Assert(tableNames.Count() == columns.Count(), "The number of tables is not equal to the number of columnDicts");



        //    // iterate trough all tables
        //    foreach (var tables in tableNames) {

        //    // rename existing tables
        //        if (curProgress <= progressStart + 0) {
        //            try {
        //                
        //                //Rename all source tables to tablename_bak, but if there are bak tables already, then use them and delete the original tables
        //                foreach (var table in tables) {
        //                    if (table.Key.EndsWith("_bak")) continue;

        //                    if (_conn.TableExists(table.Key + "_bak")) {
        //                        _conn.DropTableIfExists(table.Key);
        //                    } else if (_conn.TableExists(table.Key)) _conn.RenameTable(table.Key, table.Key + "_bak");
        //                }

        //            } catch (Exception ex) {
        //                throw new Exception(ExceptionMessages.FailedToRenameTables + ":" + Environment.NewLine +
        //                                    ex.Message + ".");
        //            }
        //            if (_conn.DbConfig.DbType == "SQLServer" && setIdentityOnInit)
        //                _conn.ExecuteNonQuery("SET IDENTITY_INSERT dbo." + _conn.Enquote("info") + " ON");
        //            // Opinion by sev: would lead to skip the first task (creating new table) because the curProgress is increased by 2 at the end of this methode
        //            //Info.SetValue(key, Convert.ToString(curProgress + 1), _conn);
        //            if (_conn.DbConfig.DbType == "SQLServer" && setIdentityOnInit)
        //                _conn.ExecuteNonQuery("SET IDENTITY_INSERT dbo." + _conn.Enquote("info") + " OFF");

        //            UpdateProgress(_conn, key, ref curProgress);
        //        }
        //        // create new destination tables
        //        if (curProgress <= progressStart + 1) {
        //            foreach (var table in tables) {
        //                DatabaseCreator.Instance.CreateTable(version, _conn.DbConfig.DbType, _conn, table.Value);
        //            }
        //            
        //            UpdateProgress(_conn, key, ref curProgress);
        //        }

        //        // copy data
        //        if (curProgress <= progressStart + 2) {
        //            foreach (var table in tables) {
        //                try {
        //                    if (!_conn.TableExists(table.Key + "_bak")) continue;

        //                    // copy new data
        //                    try {
        //                        
        //                        //var sourceColumnsList = columnDict.Keys.Select(column => _conn.Enquote(column)).ToList();
        //                        
        //                        var sourceColumnsList = new List<string>();
        //                        var sourceCols = _conn.GetColumnNames(table.Key + "_bak");
        //                        foreach (var columnEntry in columnDict.Keys) {
        //                            // The KeyEntry is an existing columnName in the old table
        //                            if (sourceCols.Contains(columnEntry)) {
        //                                sourceColumnsList.Add(_conn.Enquote(columnEntry));
        //                            } else {
        //                                // The KeyEntry is not an existing column in the old table so we handle it as value
        //                                sourceColumnsList.Add("'" + columnEntry + "'");
        //                            }
        //                        }

        //                        // Set the new column name to the old column name if the caller was to lazy and gave just null as newColumnName
        //                        foreach (KeyValuePair<string, string> keyValuePair in columnDict.Where(x => x.Value == null).ToList()) {
        //                            columnDict[keyValuePair.Key] = keyValuePair.Key;
        //                        }

        //                        var destColumnsList = columnDict.Values.Select(column => _conn.Enquote(column)).ToList();
        //                        
        //                        //var tableCols = _conn.GetColumnNames(table.Value);
        //                        //foreach (var x in destColumnsList) {
        //                        //    Debug.Assert(!tableCols.Contains(x));
        //                        //}
        //                        //Debug.Assert(destColumnsList.All(_conn.GetColumnNames(table.Value).Contains));

        //                        ////Debug.Assert(!destColumnsList.Where(_conn.GetColumnNames(table.Value).Contains).Any(),
        //                        ////             "Destination column list contains an entry of an column that is not exisiting in " + table.Value);

        //                        
        //                        string columnString = String.Join(",", destColumnsList);
        //                        string replacedColumns = String.Join(",", sourceColumnsList);
        //                        
        //                        string sql = "INSERT INTO " + _conn.Enquote(table.Value) + " (" + columnString + ") SELECT " +
        //                                     replacedColumns + " FROM " + _conn.Enquote(table.Key + "_bak") +
        //                                     (tableAlias == null ? "" : " " + tableAlias);

        //                        if (_conn.DbConfig.DbType == "SQLServer" && setIdentity)
        //                            _conn.ExecuteNonQuery("SET IDENTITY_INSERT dbo." + _conn.Enquote(table.Value) + " ON");
        //                        _conn.ExecuteNonQuery(sql);
        //                        if (_conn.DbConfig.DbType == "SQLServer" && setIdentity)
        //                            _conn.ExecuteNonQuery("SET IDENTITY_INSERT dbo." + _conn.Enquote(table.Value) + " OFF");
        //                    } catch (Exception ex) {
        //                        throw new Exception(ex.Message, ex);
        //                    }
        //                } catch (Exception ex) {
        //                    throw new Exception(
        //                        string.Format(
        //                            ExceptionMessages.FailedToCopyData + ":" + Environment.NewLine + ex.Message +
        //                            ".", table));
        //                }
        //            }

        //            UpdateProgress(_conn, key, ref curProgress);
        //        }

        //        // delete backup tables
        //        if (curProgress <= progressStart + 3 && dropTableAfterFinished) {
        //            try {
        //                foreach (var table in tables) _conn.DropTableIfExists(table.Key + "_bak");
        //            } catch (Exception ex) {
        //                throw new Exception(ExceptionMessages.FailedToDeleteBackupTables + Environment.NewLine +
        //                                    ex.Message);
        //            }

        //            UpdateProgress(_conn, key, ref curProgress);
        //        }

        //        progressStart = progressStart + increaseCounter;
        //        columnDictIterationCounter++;

        //        if (columns.Count() > columnDictIterationCounter) {
        //            columnDict = columns.ToList()[columnDictIterationCounter];
        //        }
        //    }
        //    return curProgress;
        //}

        private int CreateNewTablesAndCopyDataFromOld(int progressStartStep, int progress, string key, string version,
                                                      List<string> tableNames, Dictionary<string, List<string>> columns,
                                                      Dictionary<string, string> replaceColumns = null,
                                                      bool setIdentity = false, bool setIdentityOnInit = false,
                                                      string tableAlias = null) {
            int curProgress = progress;
            // rename existing tables
            if (curProgress <= progressStartStep + 0) {
                try {
                    //List<string> tables = conn.GetTableList();

                    //Rename all tables to tablename_bak, but if there are bak tables already, then use them and delete the original tables
                    foreach (string table in tableNames) {
                        //if (!tableNames.Contains(table)) continue;
                        if (table.EndsWith("_bak")) continue;

                        if (_conn.TableExists(table + "_bak")) {
                            _conn.DropTableIfExists(table);
                        } else if (_conn.TableExists(table)) _conn.RenameTable(table, table + "_bak");
                    }
                } catch (Exception ex) {
                    throw new Exception(ExceptionMessages.FailedToRenameTables + ":" + Environment.NewLine +
                                        ex.Message + ".");
                }
                if (_conn.DbConfig.DbType == "SQLServer" && setIdentityOnInit) _conn.ExecuteNonQuery("SET IDENTITY_INSERT dbo." + _conn.Enquote("info") + " ON");
                Info.SetValue(key, Convert.ToString(curProgress + 1), _conn);
                if (_conn.DbConfig.DbType == "SQLServer" && setIdentityOnInit) _conn.ExecuteNonQuery("SET IDENTITY_INSERT dbo." + _conn.Enquote("info") + " OFF");

                curProgress++;
                Progress.Value = curProgress;
            }
            // create new tables
            if (curProgress <= progressStartStep + 1) {
                foreach (string table in tableNames) DatabaseCreator.Instance.CreateTable(version, _conn.DbConfig.DbType, _conn, table);

                curProgress++;
                Info.SetValue(key, Convert.ToString(curProgress), _conn);
                Progress.Value = curProgress;
            }

            // copy data
            if (curProgress <= progressStartStep + 2) {
                //List<string> tables = conn.GetTableList();
                foreach (string table in tableNames) {
                    try {
                        //if (!tableNames.Contains(table)) continue;
                        if (!_conn.TableExists(table + "_bak")) continue;
                        // clear table
                        _conn.ExecuteNonQuery("DELETE FROM " + _conn.Enquote(table));

                        // copy new data
                        try {
                            List<string> currentColumns;
                            if (columns != null && columns.ContainsKey(table)) currentColumns = columns[table];
                            else currentColumns = _conn.GetColumnNames(table);

                            var destColumnsList = new List<string>();
                            var sourceColumnsList = new List<string>();
                            foreach (string column in currentColumns) {
                                destColumnsList.Add(_conn.Enquote(column));
                                sourceColumnsList.Add(replaceColumns != null && replaceColumns.ContainsKey(column)
                                                          ? replaceColumns[column]
                                                          : _conn.Enquote(column));
                            }
                            string columnString = String.Join(",", destColumnsList);
                            string replacedColumns = String.Join(",", sourceColumnsList);
                            string sql = "INSERT INTO " + _conn.Enquote(table) + " (" + columnString + ") SELECT " +
                                         replacedColumns + " FROM " + _conn.Enquote(table + "_bak") +
                                         (tableAlias == null ? "" : " " + tableAlias);

                            if (_conn.DbConfig.DbType == "SQLServer" && setIdentity) _conn.ExecuteNonQuery("SET IDENTITY_INSERT dbo." + _conn.Enquote(table) + " ON");
                            _conn.ExecuteNonQuery(sql);
                            if (_conn.DbConfig.DbType == "SQLServer" && setIdentity) _conn.ExecuteNonQuery("SET IDENTITY_INSERT dbo." + _conn.Enquote(table) + " OFF");
                        } catch (Exception ex) {
                            throw new Exception(ex.Message, ex);
                        } finally {}
                    } catch (Exception ex) {
                        throw new Exception(
                            string.Format(
                                ExceptionMessages.FailedToCopyData + ":" + Environment.NewLine + ex.Message +
                                ".", table));
                    }
                }

                curProgress++;
                Info.SetValue(key, Convert.ToString(curProgress), _conn);
                Progress.Value = curProgress;
            }

            // delete backup tables
            if (curProgress <= progressStartStep + 3) {
                try {
                    foreach (string table in tableNames) _conn.DropTableIfExists(table + "_bak");
                } catch (Exception ex) {
                    throw new Exception(ExceptionMessages.FailedToDeleteBackupTables + Environment.NewLine +
                                        ex.Message);
                }
                curProgress++;
                Info.SetValue(key, Convert.ToString(curProgress), _conn);
                Progress.Value = curProgress;
            }
            return curProgress;
        }

        private void SaveRename(IDatabase conn, string table) {
            if (conn.TableExists(table + "_bak")) {
                conn.DropTableIfExists(table);
            } else {
                if (conn.TableExists(table)) conn.RenameTable(table, table + "_bak");
            }
        }

        private void UpdateProgress(IDatabase conn, string key, ref int curProgress) {
            curProgress++;
            setInfoValue(conn, key, Convert.ToString(curProgress), false);
            Progress.Value = curProgress;
        }

        #region SetEngineToInnoDb
        /// <summary>
        /// This method changes the database engine of all tables to InnoDb, if the selected
        /// database type equals "MySQL". This is nessesary, since to enable transactions.
        /// </summary>
        private void SetEngineToInnoDb() {
            if (_conn.DbConfig.DbType != "MySQL") return; // notihing to do for other db-types
            foreach (string table in _conn.GetTableList()) {
                _conn.ExecuteNonQuery("ALTER TABLE " + _conn.Enquote(table) + " ENGINE = InnoDb");
            }
        }
        #endregion

        #region Upgrade
        //public void Upgrade(object configObject) {
        //    MainWindowModel config = configObject as MainWindowModel;
        //    if (config == null)
        //        //Sollte niemals vorkommen
        //        OnError(new Exception("Keine Konfiguration vorhanden."));
        //    try {
        //        Upgrade(config);
        //    } catch (Exception ex) {
        //        OnError(ex);
        //    }
        //}
        public void Upgrade() {
            try {
                UpgradeImpl();
            } catch (Exception ex) {
                OnError(ex);
            }
        }

        /// <summary>
        /// Upgrades the database to the current version.
        /// </summary>
        public void UpgradeImpl() {
            //((DbAccess.DbSpecific.SQLite.Database)ConnectionManager.GetConnection()).SetPassword("");
            try {
                PreviousVersion = Info.DbVerion;
                string curVersion = PreviousVersion;
                if (curVersion == null) return;
                
                var userInfo = new eBalanceBackup.UserInfo();
                userInfo.Comment = "Automatisches Datenbank-Backup beim Updaten von Version " + Info.DbVerion +
                                   " auf Version " + VersionInfo.Instance.CurrentDbVersion + ".";

                string filename = DatabaseManager.DatabaseConfig.BackupDirectory + "eBalanceKitBackup_" +
                                  DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss") + ".bak";
                var backup = new eBalanceBackup();
                try {
                    using (IDatabase conn = DatabaseManager.ConnectionManager.GetConnection()) {
                        //conn.Open();
                        backup.ExportDatabase(conn, filename, userInfo, true);
                    }
                } catch (Exception ex) {
                    throw new Exception(
                        "Die aktuelle Datenbank konnte nicht exportiert werden, das Datenbank Upgrade wird nicht durchgeführt. Fehler: " +
                        ex.Message);
                }

                while (VersionInfo.Instance.NewerDbVersionExists(curVersion)) {
                    curVersion = VersionInfo.Instance.GetNextDbVersion(curVersion);
                    try {
                        Progress.Caption = string.Format(ExceptionMessages.DbUpgradeProgress, curVersion);
                        UpgradeTo(curVersion);
                    } catch (Exception ex) {
                        throw new Exception(
                            string.Format(ExceptionMessages.DbUpgradeFailed, curVersion) + ": " + ex.Message, ex);
                    }
                }

                Info.DbVerion = VersionInfo.Instance.CurrentDbVersion;
                OnFinished();
            } finally {
                Progress.CloseParent();
            }
        }
        #endregion

        #region UpgradeTo
        /// <summary>
        /// Upgrades the database to the specified version.
        /// </summary>
        /// <param name="version">Version, to which the upgrade should be proceded.</param>
        private void UpgradeTo(string version) {
            // no changes occured in earlier versions (except of 1.0.0 to 1.0.1, 
            // which has only be deployed to one customer and is already updated)
            //if (version.CompareTo("1.1.6") < 0) {
            //if (version.CompareTo("1.1.6") < 0) {
            //    Info.DbVerion = version;
            //    return;
            //}
            /*if (Info.VersionHistory[version] < Info.VersionHistory["1.1.6"]) {
                Info.DbVerion = version;
                return;
            }*/

            switch (version) {
                case "1.1.6":
                    UpgradeTo_1_1_6();
                    break;

                case "1.1.8":
                    UpgradeTo_1_1_8();
                    break;

                case "1.1.9":
                    UpgradeTo_1_1_9();
                    break;

                case "1.2.0":
                    UpgradeTo_1_2_0();
                    break;

                case "1.2.1":
                    UpgradeTo_1_2_1();
                    break;

                case "1.3.0":
                    UpgradeTo_1_3_0();
                    break;

                case "1.3.1":
                    UpgradeTo_1_3_1();
                    break;

                case "1.4.0":
                    UpgradeTo_1_4_0();
                    break;
                case "1.5.0":
                    UpgradeTo_1_5_0();
                    break;
                case "1.5.9":
                    UpgradeTo_1_5_9();
                    break;
                case "1.6.0":
                    UpgradeTo_1_6_0();
                    break;
            }
        }

        #endregion

        #region UpgradeTo_1_1_6
        private void UpgradeTo_1_1_6() {
            string key = "Ugrade_1_1_6";
            string keyInfo = "Ugrade_1_1_6_Info";

            string progressStr = Info.GetValue(key);
            int curProgress = (progressStr == null ? 0 : Int32.Parse(progressStr));

            Progress.Maximum = 8;
            Progress.Value = curProgress;


            // switsch storage engine to InnoDb (MySQL only)
            if (curProgress == 0) {
                SetEngineToInnoDb();

                if (_conn.DbConfig.DbType == "SQLServer") _conn.ExecuteNonQuery("SET IDENTITY_INSERT dbo." + _conn.Enquote("info") + " ON");

                Info.SetValue(key, "1", _conn);
                if (_conn.DbConfig.DbType == "SQLServer") _conn.ExecuteNonQuery("SET IDENTITY_INSERT dbo." + _conn.Enquote("info") + " OFF");
                curProgress = 1;
                Progress.Value = curProgress;
            }

            // copy data from old company tables into table "values_gcd_company"
            if (curProgress <= 1) {
                var processedCompanyIds = new List<string>();
                string tmp = Info.GetValue(keyInfo, _conn);
                if (tmp != null) foreach (string companyId in tmp.Split(',')) processedCompanyIds.Add(companyId);
                var taxonomyInfo = new TaxonomyInfo();
                taxonomyInfo.Filename = "de-gcd-2010-12-16-shell.xsd";
                taxonomyInfo.Path = "Taxonomy\\base\\de-gcd-2010-12-16";
                taxonomyInfo.Type = TaxonomyType.GCD;

                var taxonomy = new Taxonomy.Taxonomy(taxonomyInfo);


                //taxonomy.Load(TaxonomyVersion);

                var upgraderTo_1_6_6 = new UpgraderTo_1_6_6(_conn);
                upgraderTo_1_6_6.MigrateCompaniesTable();

                Info.SetValue(key, "2", _conn);
                curProgress = 2;
                Progress.Value = curProgress;
            }

            // drop unused table
            if (curProgress <= 2) {
                try {
                    _conn.DropTableIfExists("company_contacts");
                } catch (Exception ex) {
                    throw new Exception(
                        string.Format(
                            ExceptionMessages.FailedToDeleteTable + ":" + Environment.NewLine + ex.Message,
                            "company_contacts"));
                }
                Info.SetValue(key, "3", _conn);
                curProgress = 3;
                Progress.Value = curProgress;
            }

            // drop unused table
            if (curProgress <= 3) {
                try {
                    _conn.DropTableIfExists("shareholders");
                } catch (Exception ex) {
                    throw new Exception(
                        string.Format(
                            ExceptionMessages.FailedToDeleteTable + ":" + Environment.NewLine + ex.Message,
                            "shareholders"));
                }
                Info.SetValue(key, "4", _conn);
                curProgress = 4;
                Progress.Value = curProgress;
            }

            var tableNames = new List<string> {
                "accounts",
                "assignment_template_lines",
                "document_rights",
                "financial_years",
                "systems",
                "users",
                "values_gaap",
                "values_gcd",
                "assignment_template_heads",
                "companies",
                "documents",
                "balance_lists"
            };
            curProgress = CreateNewTablesAndCopyDataFromOld(4, curProgress, key, "1.1.6", tableNames,
                                                            new Dictionary<string, List<string>>());

            Info.DbVerion = "1.1.6";
            Info.RemoveValue(key);
        }
        #endregion

        #region UpgradeTo_1_1_8
        private void UpgradeTo_1_1_8() {
            var taxonomyInfo = new TaxonomyInfo();
            taxonomyInfo.Path = "Taxonomy\\base\\de-gaap-ci-2010-12-16";
            taxonomyInfo.Filename = "de-gaap-ci-2010-12-16-shell-fiscal.xsd";
            taxonomyInfo.Type = TaxonomyType.GAAP;
            var taxonomy = new Taxonomy.Taxonomy(taxonomyInfo);
            //taxonomy.Load(TaxonomyVersion);

            try {
                _conn.BeginTransaction();

                foreach (IElement elem in taxonomy.Elements.Values) {
                    // reset computed values
                    if (elem.HasComputationTargets) {
                        _conn.ExecuteNonQuery(
                            "UPDATE " + _conn.Enquote("values_gaap") + " set " + _conn.Enquote("value") + " = null " +
                            "WHERE " + _conn.Enquote("xbrl_elem_id") + "='" + elem.Name + "'");
                    }
                }

                _conn.CommitTransaction();
            } catch (Exception ex) {
                _conn.RollbackTransaction();
                throw new Exception(ex.Message, ex);
            }

            try {
                // reset saved account sum values
                _conn.BeginTransaction();
                List<string> docIds =
                    _conn.GetColumnStringValues("SELECT " + _conn.Enquote("id") + " FROM " + _conn.Enquote("documents"));
                foreach (string docId in docIds) {
                    string balListId =
                        _conn.ExecuteScalar("SELECT " + _conn.Enquote("balance_list_id") + " FROM " +
                                            _conn.Enquote("documents") + " WHERE " + _conn.Enquote("id") + "=" + docId).
                            ToString();
                    //var elemNames = conn.GetColumnStringValues("SELECT DISTINCT " + conn.Enquote("assigned_element_id") + " FROM " + conn.Enquote("accounts") +
                    //    " WHERE " + conn.Enquote("balance_list_id") + "=" + balListId + " AND " + conn.Enquote("assigned_element_id") + " IS NOT NULL");
                    List<string> elemNames =
                        _conn.GetColumnStringValues("SELECT " + _conn.Enquote("assigned_element_id") + " FROM " +
                                                    _conn.Enquote("accounts") +
                                                    " WHERE " + _conn.Enquote("balance_list_id") + "=" + balListId +
                                                    " AND " + _conn.Enquote("assigned_element_id") + " IS NOT NULL");
                    foreach (string elemName in elemNames.Distinct()) {
                        _conn.ExecuteNonQuery(
                            "UPDATE " + _conn.Enquote("values_gaap") + " set " + _conn.Enquote("value") + " = null " +
                            "WHERE " + _conn.Enquote("xbrl_elem_id") + "='" + elemName + "' " +
                            "AND " + _conn.Enquote("document_id") + "=" + docId);
                    }
                }
                _conn.CommitTransaction();
            } catch (Exception ex) {
                _conn.RollbackTransaction();
                throw new Exception(ex.Message, ex);
            }

            Info.DbVerion = "1.1.8";
        }
        #endregion

        #region UpgradeTo_1_1_9
        private void UpgradeTo_1_1_9() {
            string key = "Ugrade_1_1_9";
            string progressStr = Info.GetValue(key);
            int curProgress = (progressStr == null ? 0 : Int32.Parse(progressStr));

            Progress.Maximum = 4;
            Progress.Value = curProgress;
            var taxonomyInfo = new TaxonomyInfo {
                Path = "Taxonomy\\base\\" + "de-gaap-ci-2010-12-16",
                Filename = "de-gaap-ci-2010-12-16-shell-fiscal.xsd",
                Type = TaxonomyType.GAAP
            };
            var taxonomy = new Taxonomy.Taxonomy(taxonomyInfo);
            //taxonomy.Load(TaxonomyVersion);

            // search orphaned elements in computation tree for which all presentation tree parents are not computed
            var orphanedElements = new List<string>();
            foreach (IElement elem in taxonomy.Elements.Values) {
                if (!elem.HasComputationItems) {
                    bool foundComputedParent = false;
                    foreach (IPresentationTreeNode ptreeNode in elem.PresentationTreeNodes) {
                        if (ptreeNode.Parents.Any(parent => (parent as PresentationTreeNode).IsComputed)) {
                            foundComputedParent = true;
                        }
                    }
                    if (!foundComputedParent) orphanedElements.Add(elem.Name);
                }
            }


            //List<string> tableNames = new List<string>{"accounts", "assignment_template_element_info", "assignment_template_heads", "assignment_template_lines",
            //                        "balance_lists", "companies", "document_rights", "documents", "financial_years", "systems", "transfer_hbst_heads", "transfer_hbst_lines",
            //                        "users","values_gaap","values_gcd","values_gcd_company"};
            var tableNames = new List<string> {
                "accounts",
                "assignment_template_element_info",
                "assignment_template_lines",
                "companies",
                "document_rights",
                "documents",
                "financial_years",
                "systems",
                "transfer_hbst_heads",
                "transfer_hbst_lines",
                "users",
                "values_gaap",
                "values_gcd",
                "values_gcd_company",
                "assignment_template_heads",
                "balance_lists"
            };

            var relevantTables = new List<string> {"values_gaap", "values_gcd", "values_gcd_company"};
            var columns = new Dictionary<string, List<string>>();
            columns["values_gaap"] = new List<string> {
                "parent_id",
                "document_id",
                "id",
                "xbrl_elem_id",
                "value",
                "cb_value_other",
                "is_manual_value",
                "auto_computation_enabled",
                "supress_warning_messages",
                "send_account_balances"
            };

            columns["values_gcd"] = new List<string> {
                "parent_id",
                "document_id",
                "id",
                "xbrl_elem_id",
                "value",
                "cb_value_other",
                "is_manual_value",
                "auto_computation_enabled",
                "supress_warning_messages",
                "send_account_balances"
            };

            columns["values_gcd_company"] = new List<string> {
                "parent_id",
                "company_id",
                "id",
                "xbrl_elem_id",
                "value",
                "cb_value_other",
                "is_manual_value",
                "auto_computation_enabled",
                "supress_warning_messages",
                "send_account_balances"
            };

            var columnReplacements = new Dictionary<string, string>
            {{"auto_computation_enabled", "flag4"}, {"supress_warning_messages", "0"}, {"send_account_balances", "0"}};

            curProgress = CreateNewTablesAndCopyDataFromOld(0, curProgress, key, "1.1.9", tableNames, columns,
                                                            columnReplacements, false);

            // set auto_computation_enabled flag
            if (curProgress <= 4) {
                try {
                    _conn.BeginTransaction();

                    foreach (string elemName in orphanedElements) {
                        _conn.ExecuteNonQuery(
                            "UPDATE " + _conn.Enquote("values_gaap") + " set " +
                            _conn.Enquote("auto_computation_enabled") + " = 1 " +
                            "WHERE " + _conn.Enquote("xbrl_elem_id") + "='" + elemName + "' ");
                    }

                    _conn.CommitTransaction();

                    Info.SetValue(key, "5", _conn);
                    curProgress = 5;
                } catch (Exception ex) {
                    _conn.RollbackTransaction();

                    throw new Exception(string.Format(
                        ExceptionMessages.FailedToUpdateTable + ":" + Environment.NewLine + ex.Message,
                        "values_gaap"));
                    throw new Exception(ex.Message, ex);
                }
            }

            if (curProgress <= 6) {
                //Recreate info table
                string table = "info";
                try {
                    //conn.BeginTransaction();
                    _conn.DropTableIfExists(table + "_bak");
                    _conn.RenameTable(table, table + "_bak");
                    DatabaseCreator.Instance.CreateTable("1.1.9", _conn.DbConfig.DbType, _conn, table);
                    string columnString = _conn.Enquote("id") + "," + _conn.Enquote("key") + "," +
                                          _conn.Enquote("value");

                    _conn.ExecuteNonQuery(
                        "INSERT INTO " + _conn.Enquote(table) + " (" + columnString + ") SELECT " +
                        columnString +
                        " FROM " + _conn.Enquote(table + "_bak"));
                    _conn.DropTableIfExists(table + "_bak");
                } catch (Exception ex) {
                    throw new Exception(string.Format(
                        ExceptionMessages.FailedToUpdateTable + ":" + Environment.NewLine + ex.Message,
                        "info"));
                }
                Info.SetValue(key, "6", _conn);
                curProgress = 6;
            }

            if (curProgress <= 6) {
                TransferValueUpdate.UeberleitungsrechnungUpdate(_conn);
            }

            Info.DbVerion = "1.1.9";
            Info.RemoveValue(key);
        }
        #endregion UpgradeTo_1_1_9

        #region UpgradeTo_1_2_0
        private void UpgradeTo_1_2_0() {
            string key = "Ugrade_1_2_0";
            string progressStr = Info.GetValue(key);
            int curProgress = (progressStr == null ? 0 : Int32.Parse(progressStr));

            Progress.Maximum = 5;
            Progress.Value = curProgress;

            if (curProgress <= 0) {
                SetEngineToInnoDb();

                //Create new tables
                var tables = new[] {"taxonomy_ids", "splitted_accounts", "account_groups"};
                foreach (string table in tables) {
                    _conn.DropTableIfExists(table);
                    DatabaseCreator.Instance.CreateTable("1.2.0", _conn.DbConfig.DbType, _conn, table);
                }

                UpdateProgress(_conn, key, ref curProgress);
            }
            var tableNames = new List<string> {"accounts", "documents", "balance_lists"};

            if (curProgress <= 1) {
                //Rename tables
                foreach (string tableName in tableNames) SaveRename(_conn, tableName);
                UpdateProgress(_conn, key, ref curProgress);
            }
            if (curProgress <= 2) {
                //Create new tables
                foreach (string tableName in tableNames) {
                    _conn.DropTableIfExists(tableName);
                    DatabaseCreator.Instance.CreateTable("1.2.0", _conn.DbConfig.DbType, _conn, tableName);
                }
                UpdateProgress(_conn, key, ref curProgress);
            }
            if (curProgress <= 3) {
                #region WriteAccounts
                //Init taxonomie
                TaxonomyAssignments assignments = TaxonomyAssignments.Instance;
                assignments.InsertIntoTaxonomyTable(_conn);
                //Copy old data into account table, need to adjust the column "assigned_element_id" which was a string and is now a id
                using (IDatabase writeConn = ConnectionManager.CreateConnection(_conn.DbConfig)) {
                    writeConn.Open();
                    writeConn.BeginTransaction();

                    var reader = _conn.ExecuteReader("SELECT * FROM " + _conn.Enquote("accounts_bak"));
                    try {
                        var sameColumns = new[] {"id", "balance_list_id", "number", "name", "dc_flag"};

                        while (reader.Read()) {
                            var values = new DbColumnValues();
                            foreach (string column in sameColumns) values[column] = reader[column];
                            values["additional_info"] = null;
                            values["amount"] = reader["balance"];
                            values["group_id"] = 0;

                            if (reader["assigned_element_id"] is DBNull) values["assigned_element_id"] = 0;
                            else {
                                var element = reader["assigned_element_id"] as string;
                                //if(TaxonomyIdManager.Instance.HasId(TaxonomyManager.GAAP_Taxonomy.ElementsByName[element].Id))
                                if (assignments.HasId(element)) values["assigned_element_id"] = assignments.GetId(element);
                                    //values["assigned_element_id"] = TaxonomyIdManager.Instance.GetId(TaxonomyManager.GAAP_Taxonomy.ElementsByName[element].Id);
                                else {
                                    values["assigned_element_id"] = 0;
                                    Debug.WriteLine("Could not find element " + element +
                                                    " in taxonomy.");
                                }
                            }

                            writeConn.InsertInto("accounts", values);
                        }
                        writeConn.CommitTransaction();
                    } catch (Exception ex) {
                        writeConn.RollbackTransaction();
                        throw ex;
                    } finally {
                        if (reader != null && !reader.IsClosed) {
                            reader.Close();
                        }
                    }
                }
                UpdateProgress(_conn, key, ref curProgress);
                #endregion
            }
            if (curProgress <= 4) {
                try {
                    //Copy document table (the column "balance_list_id" has been dropped)
                    var currentColumns = new List<string> {
                        "id",
                        "name",
                        "comment",
                        "company_id",
                        "system_id",
                        "financial_year_id",
                        "creator_id"
                        ,
                        "creation_date",
                        "last_modifier_id",
                        "last_modified_date"
                    };
                    for (int i = 0; i < currentColumns.Count; ++i) currentColumns[i] = _conn.Enquote(currentColumns[i]);
                    string columnString = String.Join(",", currentColumns);
                    string sql = "INSERT INTO " + _conn.Enquote("documents") + " (" + columnString + ") SELECT " +
                                 columnString + " FROM " + _conn.Enquote("documents_bak");
                    _conn.ExecuteNonQuery(sql);
                } catch (Exception ex) {
                    throw ex;
                }
                UpdateProgress(_conn, key, ref curProgress);
            }
            if (curProgress <= 5) {
                //Copy balance lists
                _conn.BeginTransaction();
                var reader =
                    _conn.ExecuteReader("SELECT id,balance_list_id FROM " + _conn.Enquote("documents_bak"));
                try {
                    var documentIds = new List<Tuple<long, long>>();
                    while (reader.Read()) {
                        if (reader["id"] is long) documentIds.Add(new Tuple<long, long>((long) reader["id"], (long) reader["balance_list_id"]));
                        else documentIds.Add(new Tuple<long, long>((int) reader["id"], (int) reader["balance_list_id"]));
                    }
                    reader.Close();
                    foreach (var documentId in documentIds) {
                        var currentColumns = new List<string>
                        {"document_id", "id", "comment", "name", "imported_from_id", "import_date", "source"};
                        for (int i = 0; i < currentColumns.Count; ++i) currentColumns[i] = _conn.Enquote(currentColumns[i]);
                        string columnString = String.Join(",", currentColumns);
                        string oldColumns = documentId.Item1 + "," + _conn.Enquote("id") + "," + _conn.GetSqlString("") +
                                            "," + _conn.GetSqlString("Summen- und Saldenliste") + "," +
                                            _conn.Enquote("imported_from_id") + "," +
                                            _conn.Enquote("import_date") + "," + _conn.Enquote("source");


                        string sql = "INSERT INTO " + _conn.Enquote("balance_lists") + " (" + columnString + ") SELECT " +
                                     oldColumns + " FROM " + _conn.Enquote("balance_lists_bak") + " WHERE id=" +
                                     documentId.Item2;
                        _conn.ExecuteNonQuery(sql);
                    }
                    _conn.CommitTransaction();
                } catch (Exception ex) {
                    throw new Exception(ex.Message);
                }
                UpdateProgress(_conn, key, ref curProgress);
            }

            if (curProgress <= 6) {
                //Delete bak tables
                foreach (string tableName in tableNames) _conn.DropTableIfExists(tableName + "_bak");
                UpdateProgress(_conn, key, ref curProgress);
            }

            //For SQLite we need to recreate the tables which had reals in them
            if (_conn.DbConfig.DbType == "SQLite") {
                var tablesToRecreate = new List<string> {"transfer_hbst_lines"};
                CreateNewTablesAndCopyDataFromOld(7, 7, key, "1.2.0", tablesToRecreate,
                                                  new Dictionary<string, List<string>>());
            }
            Info.DbVerion = "1.2.0";
            Info.RemoveValue(key);
        }
        #endregion

        #region UpgradeTo_1_2_1
        private void UpgradeTo_1_2_1() {
            string key = "Ugrade_1_2_1";
            string progressStr = Info.GetValue(key);
            int curProgress = (progressStr == null ? 0 : Int32.Parse(progressStr));

            Progress.Maximum = 5;
            Progress.Value = curProgress;

            if (curProgress <= 0) {
                SetEngineToInnoDb();

                //Create new tables
                var tables = new[] {"splitted_accounts", "account_groups"};
                foreach (string table in tables) {
                    _conn.DropTableIfExists(table);
                    DatabaseCreator.Instance.CreateTable("1.2.1", _conn.DbConfig.DbType, _conn, table);
                }

                UpdateProgress(_conn, key, ref curProgress);
            }
            var tableNames = new List<string> {"accounts", "assignment_template_lines"};

            if (curProgress <= 1) {
                //Rename tables
                foreach (string tableName in tableNames) SaveRename(_conn, tableName);
                UpdateProgress(_conn, key, ref curProgress);
            }
            if (curProgress <= 2) {
                //Create new tables
                foreach (string tableName in tableNames) {
                    _conn.DropTableIfExists(tableName);
                    DatabaseCreator.Instance.CreateTable("1.2.1", _conn.DbConfig.DbType, _conn, tableName);
                }
                UpdateProgress(_conn, key, ref curProgress);
            }
            if (curProgress <= 3) {
                #region AccountsTable
                using (IDatabase writeConn = ConnectionManager.CreateConnection(_conn.DbConfig)) {
                    writeConn.Open();
                    writeConn.BeginTransaction();

                    var reader = _conn.ExecuteReader("SELECT * FROM " + _conn.Enquote("accounts_bak"));
                    try {
                        var sameColumns = new[] {"id", "balance_list_id", "number", "name", "assigned_element_id"};
                        //id, balance_list_id, number, name, balance, assigned_element_id, dc_flag
                        while (reader.Read()) {
                            var values = new DbColumnValues();
                            foreach (string column in sameColumns) values[column] = reader[column];
                            values["comment"] = reader["additional_info"];
                            var amount = (decimal) reader["amount"];
                            if (!(reader["dc_flag"] is DBNull)) {
                                switch ((string) reader["dc_flag"]) {
                                    case "C":
                                    case "c":
                                    case "H":
                                    case "h":
                                        // credit/Haben
                                        amount = (-1)*(decimal) reader["amount"];
                                        break;
                                }
                            }
                            values["amount"] = amount;
                            writeConn.InsertInto("accounts", values);
                        }
                        writeConn.CommitTransaction();
                    } catch (Exception ex) {
                        writeConn.RollbackTransaction();
                        throw ex;
                    } finally {
                        if (reader != null && !reader.IsClosed) {
                            reader.Close();
                        }
                    }
                }
                UpdateProgress(_conn, key, ref curProgress);
                #endregion
            }
            if (curProgress <= 4) {
                using (IDatabase writeConn = ConnectionManager.CreateConnection(_conn.DbConfig)) {
                    writeConn.Open();
                    writeConn.BeginTransaction();

                    var reader =
                        _conn.ExecuteReader("SELECT * FROM " + _conn.Enquote("assignment_template_lines_bak"));
                    try {
                        var sameColumns = new[] {"id", "template_id", "account_number", "account_name"};
                        //debit_element_id, credit_element_id
                        while (reader.Read()) {
                            var values = new DbColumnValues();
                            foreach (string column in sameColumns) values[column] = reader[column];
                            if (reader["debit_element_id"] is DBNull) values["debit_element_id"] = reader["debit_element_id"];
                            else {
                                var id = (string) reader["debit_element_id"];
                                if (!id.Contains("de-gaap-ci_")) id = "de-gaap-ci_" + id;
                                values["debit_element_id"] = id;
                            }
                            if (reader["credit_element_id"] is DBNull) values["credit_element_id"] = reader["credit_element_id"];
                            else {
                                var id = (string) reader["credit_element_id"];
                                if (!id.Contains("de-gaap-ci_")) id = "de-gaap-ci_" + id;
                                values["credit_element_id"] = id;
                            }

                            writeConn.InsertInto("assignment_template_lines", values);
                        }
                        writeConn.CommitTransaction();
                    } catch (Exception ex) {
                        writeConn.RollbackTransaction();
                        throw ex;
                    } finally {
                        if (reader != null && !reader.IsClosed) {
                            reader.Close();
                        }
                    }
                }
                UpdateProgress(_conn, key, ref curProgress);
            }

            if (curProgress <= 5) {
                //Delete bak tables
                foreach (string tableName in tableNames) _conn.DropTableIfExists(tableName + "_bak");
                UpdateProgress(_conn, key, ref curProgress);
            }

            Info.DbVerion = "1.2.1";
            Info.RemoveValue(key);
        }
        #endregion

        #region UpgradeTo_1_3_0
        private void UpgradeTo_1_3_0() {
            string key = "Ugrade_1_3_0";
            string progressStr = Info.GetValue(key);
            int curProgress = (progressStr == null ? 0 : Int32.Parse(progressStr));

            Progress.Maximum = 5;
            Progress.Value = curProgress;

            if (curProgress <= 0) {
                SetEngineToInnoDb();

                //Create new tables
                var tables = new[] {
                    "splitted_accounts", "split_account_groups", "account_groups",
                    "log_main", "log_send", "log_admin", "log_value_change", "send_log", "log_report_1_value_change",
                    "log_report_1"
                };
                foreach (string table in tables) {
                    _conn.DropTableIfExists(table);
                    if (DatabaseCreator.Instance.GetTableNames("1.3.0", _conn.DbConfig.DbType).Contains(table)) DatabaseCreator.Instance.CreateTable("1.3.0", _conn.DbConfig.DbType, _conn, table);
                }

                UpdateProgress(_conn, key, ref curProgress);
            }

            //
            {
                var table = new List<string> {"users"};
                CreateNewTablesAndCopyDataFromOld(1, curProgress, key, "1.3.0", table,
                                                  new Dictionary<string, List<string>>(),
                                                  new Dictionary<string, string> {{"is_deleted", "0"}}, false, false);
            }
            Info.DbVerion = "1.3.0";
            Info.RemoveValue(key);
        }
        #endregion

        #region UpgradeTo_1_3_1
        private void UpgradeTo_1_3_1() {
            string key = "Ugrade_1_3_1";
            string progressStr = Info.GetValue(key);
            int curProgress = (progressStr == null ? 0 : Int32.Parse(progressStr));

            Progress.Maximum = 4;
            Progress.Value = curProgress;

            var userTable = new List<string> {"users"};
            curProgress = CreateNewTablesAndCopyDataFromOld(0, curProgress, key, "1.3.1", userTable,
                                                            new Dictionary<string, List<string>>(),
                                                            null, false, false);

            if (curProgress <= 4) {
                try {
                    var tables = new List<string> {"splitted_accounts", "split_account_groups"};

                    _conn.BeginTransaction();
                    foreach (string table in tables) {
                        List<string> balanceListIds =
                            _conn.GetColumnStringValues("SELECT " + _conn.Enquote("balance_list_id") + " FROM " +
                                                        _conn.Enquote(table));
                        foreach (string balanceListId in balanceListIds.Distinct()) {
                            object balListId =
                                _conn.ExecuteScalar("SELECT " + _conn.Enquote("id") + " FROM " +
                                                    _conn.Enquote("balance_lists") + " WHERE " + _conn.Enquote("id") +
                                                    "=" + balanceListId);
                            if (balListId == null) {
                                _conn.ExecuteNonQuery(
                                    "DELETE FROM " + _conn.Enquote(table) + " WHERE " + _conn.Enquote("balance_list_id") +
                                    " = " + balanceListId
                                    );
                            }
                        }
                    }
                    _conn.CommitTransaction();
                } catch (Exception ex) {
                    _conn.RollbackTransaction();
                    throw new Exception(ex.Message, ex);
                }


                UpdateProgress(_conn, key, ref curProgress);
            }

            Info.DbVerion = "1.3.1";
            Info.RemoveValue(key);
        }
        #endregion

        #region UpgradeTo_1_4_0
        private void UpgradeTo_1_4_0() {
            const string key = "Ugrade_1_4_0";
            string progressStr = Info.GetValue(key);
            int curProgress = (progressStr == null ? 0 : Int32.Parse(progressStr));

            Progress.Maximum = 38;
            Progress.Value = curProgress;

            // remove potentially existing hypercube tables (these tables are not yet used and will be replaced in future versions)
            if (curProgress <= 0) {
                DropTables(ref curProgress, key, new[] {
                    "hypercube_ass_gross", "hypercube_ass_gross_short",
                    "hypercube_ass_net",
                    "hypercube_changes_equity_statement", "document_rights"
                });
            }

            // (re)create new tables
            if (curProgress <= 1) {
                foreach (string table in new[] {"roles", "splitted_accounts"}) {
                    _conn.DropTableIfExists(table);
                    if (DatabaseCreator.Instance.GetTableNames("1.4.0", _conn.DbConfig.DbType).Contains(table)) DatabaseCreator.Instance.CreateTable("1.4.0", _conn.DbConfig.DbType, _conn, table);
                }

                UpdateProgress(_conn, key, ref curProgress);
            }

            // update accounts table
            if (curProgress <= 5) {
                var replaceColumns = new Dictionary<string, string> {
                    {"user_defined", "0"},
                    {"group_id", "0"},
                    {"hidden", "0"},
                    {"sort_index", "0"},
                    {"in_tray", "0"}
                };
                curProgress = CreateNewTablesAndCopyDataFromOld(2, curProgress, key, "1.4.0",
                                                                new List<string> {"accounts"},
                                                                new Dictionary<string, List<string>>(),
                                                                replaceColumns, false, false);
            }

            // update accounts groups table
            if (curProgress <= 9) {
                var replaceColumns = new Dictionary<string, string>
                {{"hidden", "0"}, {"sort_index", "0"}, {"in_tray", "0"}};
                curProgress = CreateNewTablesAndCopyDataFromOld(6, curProgress, key, "1.4.0",
                                                                new List<string> {"account_groups"},
                                                                new Dictionary<string, List<string>>(),
                                                                replaceColumns, false, false);
            }

            // update splitted_accounts table
            if (curProgress <= 13) {
                var replaceColumns = new Dictionary<string, string>
                {{"hidden", "0"}, {"sort_index", "0"}, {"in_tray", "0"}};
                curProgress = CreateNewTablesAndCopyDataFromOld(10, curProgress, key, "1.4.0",
                                                                new List<string> {"splitted_accounts"},
                                                                new Dictionary<string, List<string>>(),
                                                                replaceColumns, false, false);
            }

            // update documents table
            if (curProgress <= 17) {
                var replaceColumns = new Dictionary<string, string> {{"owner_id", _conn.Enquote("creator_id")}};
                curProgress = CreateNewTablesAndCopyDataFromOld(14, curProgress, key, "1.4.0",
                                                                new List<string> {"documents"},
                                                                new Dictionary<string, List<string>>(),
                                                                replaceColumns, false, false);
            }

            // update users table
            if (curProgress <= 21) {
                var replaceColumns = new Dictionary<string, string> {{"salt", _conn.Enquote("username")}};
                curProgress = CreateNewTablesAndCopyDataFromOld(18, curProgress, key, "1.4.0",
                                                                new List<string> {"users"},
                                                                new Dictionary<string, List<string>>(),
                                                                replaceColumns, false, false);
            }

            // update value tables
            if (curProgress <= 25) {
                var replaceColumns = new Dictionary<string, string> {{"comment", "NULL"}};
                curProgress = CreateNewTablesAndCopyDataFromOld(22, curProgress, key, "1.4.0",
                                                                new List<string>
                                                                {"values_gaap", "values_gcd", "values_gcd_company"},
                                                                new Dictionary<string, List<string>>(),
                                                                replaceColumns, false, false);
            }

            // update companies table (new owner_id column)
            if (curProgress <= 29) {
                var replaceColumns = new Dictionary<string, string> {{"owner_id", "0"}};
                curProgress = CreateNewTablesAndCopyDataFromOld(26, curProgress, key, "1.4.0",
                                                                new List<string> {"companies"},
                                                                new Dictionary<string, List<string>>(),
                                                                replaceColumns, false, false);
            }

            // recreate taxonomy_ids table due to problems (empty number column) by some customers
            if (curProgress <= 33) {
                TaxonomyAssignments assignments = TaxonomyAssignments.Instance;
                assignments.InsertIntoTaxonomyTable(_conn);
                UpdateProgress(_conn, "1.4.0", ref curProgress);
            }

            if (curProgress <= 37) {
                try {
                    _conn.BeginTransaction();
                    string owner_id =
                        _conn.GetColumnStringValues(
                            "SELECT " + _conn.Enquote("id") + " FROM " + _conn.Enquote("users") +
                            " WHERE " + _conn.Enquote("is_admin") + "=1").FirstOrDefault();

                    _conn.ExecuteNonQuery(
                        "UPDATE " + _conn.Enquote("companies") +
                        " SET " + _conn.Enquote("owner_id") + "=" + owner_id
                        );
                    _conn.CommitTransaction();
                } catch (Exception ex) {
                    _conn.RollbackTransaction();
                    throw new Exception(ex.Message, ex);
                }

                UpdateProgress(_conn, key, ref curProgress);

                Info.DbVerion = "1.4.0";
                Info.RemoveValue(key);
            }
        }

        private void DropTables(ref int curProgress, string version, string[] tables) {
            foreach (string tableName in tables) {
                _conn.DropTableIfExists(tableName);
            }
            UpdateProgress(_conn, version, ref curProgress);
        }
        #endregion UpgradeTo_1_4_0

        #region UpgradeTo_1_5_0
        private void UpgradeTo_1_5_0() {
            const string key = "Ugrade_1_5_0";
            string progressStr = Info.GetValue(key);
            int curProgress = (progressStr == null ? 0 : Int32.Parse(progressStr));

            Progress.Maximum = 40;
            Progress.Value = curProgress;


            //rename tables and add new columns
            if (curProgress <= 0) {
                try {
                    _conn.BeginTransaction();
                    _conn.RenameTable("assignment_template_element_info", "mapping_template_element_info");
                    _conn.RenameTable("assignment_template_heads", "mapping_template_heads");
                    _conn.RenameTable("assignment_template_lines", "mapping_template_lines");
                    _conn.CommitTransaction();
                    UpdateProgress(_conn, key, ref curProgress);
                } catch (Exception ex) {
                    _conn.RollbackTransaction();
                    throw new Exception(ex.Message);
                }
            }

            try {
                _conn.BeginTransaction();
                //add more colums to the mapping_template_heads table
                if (curProgress <= 4) {
                    var replaceColumns = new Dictionary<string, string>
                    {{"modify_date", "Null"}, {"taxonomy_info_id", "2"}};
                    curProgress = CreateNewTablesAndCopyDataFromOld(curProgress, curProgress, key, "1.5.0",
                                                                    new List<string> {"mapping_template_heads"},
                                                                    new Dictionary<string, List<string>>(),
                                                                    replaceColumns, false, false);
                }
                //add is_account_of_exchange to the mapping_template_lines table
                if (curProgress <= 8) {
                    var replaceColumns = new Dictionary<string, string> {{"is_account_of_exchange", "0"}};
                    curProgress = CreateNewTablesAndCopyDataFromOld(curProgress, curProgress, key, "1.5.0",
                                                                    new List<string> {"mapping_template_lines"},
                                                                    new Dictionary<string, List<string>>(),
                                                                    replaceColumns, false, false);
                }
                _conn.CommitTransaction();
            } catch (Exception ex) {
                _conn.RollbackTransaction();
                throw new Exception(ex.Message);
            }

            // 

            // update documents table
            if (curProgress <= 12) {
                try {
                    _conn.BeginTransaction();

                    _conn.ExecuteNonQuery("UPDATE " + _conn.Enquote("mapping_template_lines") + " SET " +
                                          _conn.Enquote("is_account_of_exchange") + "=1 "
                                          + " WHERE debit_element_id is not null and credit_element_id is not null");

                    UpdateProgress(_conn, key, ref curProgress);
                    _conn.CommitTransaction();
                } catch (Exception ex) {
                    _conn.RollbackTransaction();
                    throw new Exception(ex.Message);
                }
            }

            
            // update documents table
            if (curProgress <= 13) {
                try {
                    _conn.BeginTransaction();
                    var replaceColumns = new Dictionary<string, string>
                    {{"gcd_taxonomy_info_id", "1"}, {"main_taxonomy_info_id", "2"}};
                    curProgress = CreateNewTablesAndCopyDataFromOld(curProgress, curProgress, key, "1.5.0",
                                                                    new List<string> {"documents"},
                                                                    new Dictionary<string, List<string>>(),
                                                                    replaceColumns, false, false);
                    _conn.CommitTransaction();
                } catch (Exception ex) {
                    _conn.RollbackTransaction();
                    throw new Exception(ex.Message);
                }
            }


            // update taxonomy_ids table: rename column name
            if (curProgress <= 17) {
                try {
                    _conn.BeginTransaction();
                    var replaceColumns = new Dictionary<string, string> {
                        {"taxonomy_id", "0"},
                        {"xbrl_element_id", _conn.Enquote("taxonomy_id")}
                    };
                    curProgress = CreateNewTablesAndCopyDataFromOld(curProgress, curProgress, key, "1.5.0",
                                                                    new List<string> {"taxonomy_ids"},
                                                                    new Dictionary<string, List<string>>(),
                                                                    replaceColumns, false, false);
                    _conn.CommitTransaction();
                } catch (Exception ex) {
                    _conn.RollbackTransaction();
                    throw new Exception((ex.Message));
                }
            }


            //update the taxonomy_id in taxonomy_ids
            if (curProgress <= 18) {
                try {
                    _conn.BeginTransaction();
                    _conn.ExecuteNonQuery("UPDATE " + _conn.Enquote("taxonomy_ids") + " SET " +
                                          _conn.Enquote("taxonomy_id") + "=1 "
                                          + " WHERE " + _conn.Enquote("xbrl_element_id") + " LIKE "
                                          + _conn.GetSqlString("de-gc%"));

                    _conn.ExecuteNonQuery("UPDATE " + _conn.Enquote("taxonomy_ids") + " SET " +
                                          _conn.Enquote("taxonomy_id") + "=2 "
                                          + " WHERE " + _conn.Enquote("xbrl_element_id") + " LIKE "
                                          + _conn.GetSqlString("de-ga%"));
                    UpdateProgress(_conn, key, ref curProgress);
                    _conn.CommitTransaction();
                    
                } catch (Exception ex) {
                    _conn.RollbackTransaction();
                    throw new Exception(ex.Message);
                }
            }

            if (curProgress <= 19) {
                //create temp table which is used in the next 3 following if statements
                try {
                    /**************************/
                    var taxonomyInfoGcd = new TaxonomyInfo {
                        Path = "Taxonomy\\base\\de-gcd-2010-12-16",
                        Filename = "de-gcd-2010-12-16-shell.xsd",
                        Type = TaxonomyType.GCD
                    };
                    var gcdTaxonomy = new Taxonomy.Taxonomy(taxonomyInfoGcd);

                    var taxonomyInfoGaap = new TaxonomyInfo {
                        Path = "Taxonomy\\base\\de-gaap-ci-2010-12-16",
                        Filename = "de-gaap-ci-2010-12-16-shell.xsd",
                        Type = TaxonomyType.GAAP
                    };

                    var gaapTaxonomy = new Taxonomy.Taxonomy(taxonomyInfoGaap);
                    /*************************/

                    //merge gcdTaxonomy and gaapTaxonomy to one list
                    List<KeyValuePair<string, IElement>> taxonomyElements =
                        gcdTaxonomy.Elements.ToList().Union(gaapTaxonomy.Elements.ToList()).ToList();


                    _conn.BeginTransaction();

                    var taxonomyIDs = _conn.GetColumnStringValues("SELECT " + _conn.Enquote("id") +
                                                                  " FROM " + _conn.Enquote("taxonomy_ids"));
                    var xbrlElementIDs = _conn.GetColumnStringValues("SELECT " + _conn.Enquote("xbrl_element_id") +
                                                                     " FROM " + _conn.Enquote("taxonomy_ids"));

                    //create dictionay from 2 selected columns
                    Dictionary<string, string> taxonomyDataTable =
                        xbrlElementIDs.Zip(taxonomyIDs, (k, v) => new {k, v}).ToDictionary(x => x.k,
                                                                                           x => x.v);

                    var columns = new List<DbColumnInfo>();
                    var dbColumnInfoId = new DbColumnInfo {Name = "id", Type = DbColumnTypes.DbInt};
                    var dbColumnInfoXbrlElemId = new DbColumnInfo
                    {Name = "xbrl_element_id", Type = DbColumnTypes.DbLongText};
                    columns.Add(dbColumnInfoId);
                    columns.Add(dbColumnInfoXbrlElemId);

                    _conn.CreateTable("taxonomy_ids_tmp", columns);

                    var idValues = new List<DbColumnValues>();
                    foreach (var pair in taxonomyDataTable) {
                        // get assigned element
                        IElement element;
                        gcdTaxonomy.Elements.TryGetValue(pair.Key, out element);
                        if (element == null) gaapTaxonomy.Elements.TryGetValue(pair.Key, out element);
                        if (element == null) throw new Exception("Should never ever happen exception...");

                        var value = new DbColumnValues();
                        value["id"] = int.Parse(pair.Value);
                        value["xbrl_element_id"] = element.Name;
                        idValues.Add(value);
                    }
                    _conn.InsertInto("taxonomy_ids_tmp", idValues);

                    _conn.CreateIndex("taxonomy_ids_tmp", "_idx", new List<string> {"xbrl_element_id"}, 1024);

                    UpdateProgress(_conn, key, ref curProgress);
                    _conn.CommitTransaction();
                } catch (Exception ex) {
                    _conn.RollbackTransaction();
                    throw new Exception(ex.Message);
                }
            }

            //updata the values_gcd table using taxonomy_ids_tmp table
            if (curProgress <= 23) {
                try {
                    _conn.BeginTransaction();
                    string selectTaxonomyId = " (SELECT " + _conn.Enquote("id") +
                                              " FROM " + _conn.Enquote("taxonomy_ids_tmp") + " t" +
                                              " WHERE t." + _conn.Enquote("xbrl_element_id") + " = b." +
                                              _conn.Enquote("xbrl_elem_id") + ")";

                    var replaceColumns = new Dictionary<string, string> {{"elem_id", selectTaxonomyId}};
                    curProgress = CreateNewTablesAndCopyDataFromOld(curProgress, curProgress, key, "1.5.0",
                                                                    new List<string> {"values_gcd"},
                                                                    new Dictionary<string, List<string>>(),
                                                                    replaceColumns, false, false, tableAlias: " b");

                    
                    _conn.CommitTransaction();

                } catch (Exception ex) {
                    _conn.RollbackTransaction();
                    throw new Exception(ex.Message);
                }
            }

            //update the values_gcd table using taxonomy_ids_tmp table
            if (curProgress <= 24) {
                try {
                    _conn.BeginTransaction();

                    _conn.ExecuteNonQuery(" UPDATE " + _conn.Enquote("values_gcd") + " SET " + _conn.Enquote("elem_id") +
                                          " = 0  WHERE " + _conn.Enquote("elem_id") + " is NULL");
                    UpdateProgress(_conn, key, ref curProgress);
                    _conn.CommitTransaction();
                } catch (Exception ex) {
                    _conn.RollbackTransaction();
                    throw new Exception(ex.Message);
                }
            }

            //updata the values_gaap table using taxonomy_ids_tmp table
            if (curProgress <= 28) {
                try {
                    _conn.BeginTransaction();
                    string selectTaxonomyId = " (SELECT " + _conn.Enquote("id") +
                                              " FROM " + _conn.Enquote("taxonomy_ids_tmp") + " t" +
                                              " WHERE t." + _conn.Enquote("xbrl_element_id") + " = c." +
                                              _conn.Enquote("xbrl_elem_id") + ") ";

                    var replaceColumns = new Dictionary<string, string> {{"elem_id", selectTaxonomyId}};
                    curProgress = CreateNewTablesAndCopyDataFromOld(curProgress, curProgress, key, "1.5.0",
                                                                    new List<string> {"values_gaap"},
                                                                    new Dictionary<string, List<string>>(),
                                                                    replaceColumns, false, false, tableAlias: " c");
                    _conn.CommitTransaction();
                } catch (Exception ex) {
                    _conn.RollbackTransaction();
                    throw new Exception(ex.Message);
                }
            }


            //updata the values_gaap table using taxonomy_ids_tmp table
            if (curProgress <= 29) {
                try {
                    _conn.BeginTransaction();

                    _conn.ExecuteNonQuery(" UPDATE " + _conn.Enquote("values_gaap") + " SET " + _conn.Enquote("elem_id") +
                                          " = 0  WHERE " + _conn.Enquote("elem_id") + " is NULL");
                    UpdateProgress(_conn, key, ref curProgress);
                    _conn.CommitTransaction();
                } catch (Exception ex) {
                    _conn.RollbackTransaction();
                    throw new Exception(ex.Message);
                }
            }

            //update the values_gcd_company using taxonomy_ids_tmp table
            if (curProgress <= 33) {
                try {
                    _conn.BeginTransaction();
                    string selectTaxonomyId = " (SELECT " + _conn.Enquote("id") +
                                              " FROM " + _conn.Enquote("taxonomy_ids_tmp") + " t" +
                                              " WHERE t." + _conn.Enquote("xbrl_element_id") + " = d." +
                                              _conn.Enquote("xbrl_elem_id") + ") ";

                    var replaceColumns = new Dictionary<string, string> { { "elem_id", selectTaxonomyId } };
                    curProgress = CreateNewTablesAndCopyDataFromOld(curProgress, curProgress, key, "1.5.0",
                                                                    new List<string> { "values_gcd_company" },
                                                                    new Dictionary<string, List<string>>(),
                                                                    replaceColumns, false, false, tableAlias: " d");

                    _conn.CommitTransaction();
                } catch (Exception ex) {
                    _conn.RollbackTransaction();
                    throw new Exception(ex.Message);
                }
            }

            //update the values_gcd_company using taxonomy_ids_tmp table
            if (curProgress <= 34) {
                try {
                    _conn.BeginTransaction();

                    _conn.ExecuteNonQuery(" UPDATE " + _conn.Enquote("values_gcd_company") +
                                          " SET " + _conn.Enquote("elem_id") + " = 0  " +
                                          " WHERE " + _conn.Enquote("elem_id") + " is NULL");
                    UpdateProgress(_conn, key, ref curProgress);
                    _conn.CommitTransaction();
                } catch (Exception ex) {
                    _conn.RollbackTransaction();
                    throw new Exception(ex.Message);
                }
            }

            //add is_domain_user column to users table for marking user if imported from active directoy
            if (curProgress <= 38) {
                try {
                    _conn.BeginTransaction();
                    var replaceColumns = new Dictionary<string, string> { { "is_domain_user", "0" }, { "domain", "null" } };
                    curProgress = CreateNewTablesAndCopyDataFromOld(curProgress, curProgress, key, "1.5.0",
                                                                    new List<string> {"users"},
                                                                    new Dictionary<string, List<string>>(),
                                                                    replaceColumns, false, false);
                    _conn.CommitTransaction();
                } catch (Exception ex) {
                    _conn.RollbackTransaction();
                    throw new Exception(ex.Message);
                }
            }

            //add taxonomy_info_id  to companies table
            if (curProgress <= 42) {
                try {
                    _conn.BeginTransaction();
                    var replaceColumns = new Dictionary<string, string> {{"taxonomy_info_id", "1"}};
                    curProgress = CreateNewTablesAndCopyDataFromOld(curProgress, curProgress, key, "1.5.0",
                                                                    new List<string> {"companies"},
                                                                    new Dictionary<string, List<string>>(),
                                                                    replaceColumns, false, false);
                    _conn.CommitTransaction();
                } catch (Exception ex) {
                    _conn.RollbackTransaction();
                    throw new Exception(ex.Message);
                }
            }

            //remove the taxonomy_ids_tmp table
            try {
                _conn.BeginTransaction();
                _conn.DropTable(("taxonomy_ids_tmp"));
                UpdateProgress(_conn, key, ref curProgress);
                Info.DbVerion = "1.5.0";
                Info.RemoveValue(key);
                _conn.CommitTransaction();
            } catch (Exception ex) {
                _conn.RollbackTransaction();
                throw new Exception(ex.Message);
            }
        }
        #endregion


        #region UpgradeTo_1_5_9
        private void UpgradeTo_1_5_9() {
            try {
                
                _conn.BeginTransaction();
                try {
                    //This can only happen when upgrading directly from prior to version 1.5 to 1.5.9
                    if (!_conn.TableExists("taxonomy_info")) {
                        DatabaseCreator.Instance.CreateTable("1.5.0", _conn.DbConfig.DbType, _conn, "taxonomy_info");
                        //INSERT INTO `taxonomy_info` (`id`, `name`, `path`, `filename`, `type`, `version`) VALUES (1, 'de-gcd-2010-12-16', 'Taxonomy\\base\\de-gcd-2010-12-16', 'de-gcd-2010-12-16-shell.xsd', 1, '2010-12-16');
                        //INSERT INTO `taxonomy_info` (`id`, `name`, `path`, `filename`, `type`, `version`) VALUES (2, 'de-gaap-2010-12-16', 'Taxonomy\\base\\de-gaap-ci-2010-12-16', 'de-gaap-ci-2010-12-16-shell-fiscal.xsd', 2, '2010-12-16');
                        //INSERT INTO `taxonomy_info` (`id`, `name`, `path`, `filename`, `type`, `version`) VALUES (3, 'de-gcd-2011-09-14', 'Taxonomy\\base\\de-gcd-2011-09-14', 'de-gcd-2011-09-14-shell.xsd', 1, '2011-09-14');
                        //INSERT INTO `taxonomy_info` (`id`, `name`, `path`, `filename`, `type`, `version`) VALUES (4, 'de-gaap-2011-09-14', 'Taxonomy\\base\\de-gaap-ci-2011-09-14', 'de-gaap-ci-2011-09-14-shell-fiscal.xsd', 2, '2011-09-14');
                        //INSERT INTO `taxonomy_info` (`id`, `name`, `path`, `filename`, `type`, `version`) VALUES (5, 'de-bra-2011-09-14', 'Taxonomy\\base\\de-bra-2011-09-14', 'de-bra-2011-09-14-shell-fiscal.xsd', 3, '2011-09-14');
                        //INSERT INTO `taxonomy_info` (`id`, `name`, `path`, `filename`, `type`, `version`) VALUES (6, 'de-fi-2011-09-14', 'Taxonomy\\fi\\de-fi-2011-09-14', 'de-fi-2011-09-14-shell-staffelform-fiscal.xsd', 4, '2011-09-14');
                        //INSERT INTO `taxonomy_info` (`id`, `name`, `path`, `filename`, `type`, `version`) VALUES (7, 'de-ins-2011-09-14', 'Taxonomy\\ins\\de-ins-2011-09-14', 'de-ins-2011-09-14-shell-fiscal.xsd', 5, '2011-09-14');
                        List<Tuple<int, string, string, string, int, string>> ids =
                            new List<Tuple<int, string, string, string, int, string>>() {
                                new Tuple<int, string, string, string, int, string>(1, "de-gcd-2010-12-16", "Taxonomy\\base\\de-gcd-2010-12-16", "de-gcd-2010-12-16-shell.xsd", 1, "2010-12-16"),
                                new Tuple<int, string, string, string, int, string>(2, "de-gaap-2010-12-16", "Taxonomy\\base\\de-gaap-ci-2010-12-16", "de-gaap-ci-2010-12-16-shell-fiscal.xsd", 2, "2010-12-16"),
                                new Tuple<int, string, string, string, int, string>(3, "de-gcd-2011-09-14", "Taxonomy\\base\\de-gcd-2011-09-14", "de-gcd-2011-09-14-shell.xsd", 1, "2011-09-14"),
                                new Tuple<int, string, string, string, int, string>(4, "de-gaap-2011-09-14", "Taxonomy\\base\\de-gaap-ci-2011-09-14", "de-gaap-ci-2011-09-14-shell-fiscal.xsd", 2, "2011-09-14"),
                                new Tuple<int, string, string, string, int, string>(5, "de-bra-2011-09-14", "Taxonomy\\base\\de-bra-2011-09-14", "de-bra-2011-09-14-shell-fiscal.xsd", 3, "2011-09-14"),
                                new Tuple<int, string, string, string, int, string>(6, "de-fi-2011-09-14", "Taxonomy\\fi\\de-fi-2011-09-14", "de-fi-2011-09-14-shell-staffelform-fiscal.xsd", 4, "2011-09-14"),
                                new Tuple<int, string, string, string, int, string>(7, "de-ins-2011-09-14", "Taxonomy\\ins\\de-ins-2011-09-14", "de-ins-2011-09-14-shell-fiscal.xsd", 5, "2011-09-14"),
                            };

                        foreach (var id in ids) {
                            DbColumnValues value = new DbColumnValues();
                            value["id"] = id.Item1;
                            value["name"] = id.Item2;
                            value["path"] = id.Item3;
                            value["filename"] = id.Item4;
                            value["type"] = id.Item5;
                            value["version"] = id.Item6;
                            _conn.InsertInto("taxonomy_info", value);
                        }

                    }

//INSERT INTO `taxonomy_info` (`id`, `name`, `path`, `filename`, `type`, `version`) VALUES (8, 'de-gcd-2012-06-01', 'Taxonomy\\base\\de-gcd-2012-06-01', 'de-gcd-2012-06-01-shell.xsd', 1, '2012-06-01');
//INSERT INTO `taxonomy_info` (`id`, `name`, `path`, `filename`, `type`, `version`) VALUES (9, 'de-gaap-2012-06-01', 'Taxonomy\\base\\de-gaap-ci-2012-06-01', 'de-gaap-ci-2012-06-01-shell-fiscal.xsd', 2, '2012-06-01');
//INSERT INTO `taxonomy_info` (`id`, `name`, `path`, `filename`, `type`, `version`) VALUES (10, 'de-bra-2012-06-01', 'Taxonomy\\base\\de-bra-2012-06-01', 'de-bra-2012-06-01-shell-fiscal.xsd', 3, '2012-06-01');
//INSERT INTO `taxonomy_info` (`id`, `name`, `path`, `filename`, `type`, `version`) VALUES (11, 'de-fi-2012-06-01', 'Taxonomy\\fi\\de-fi-2012-06-01', 'de-fi-2012-06-01-shell-staffelform-fiscal.xsd', 4, '2012-06-01');
//INSERT INTO `taxonomy_info` (`id`, `name`, `path`, `filename`, `type`, `version`) VALUES (12, 'de-ins-2012-06-01', 'Taxonomy\\ins\\de-ins-2012-06-01', 'de-ins-2012-06-01-shell-fiscal.xsd', 5, '2012-06-01');
                    
                    DbColumnValues newValues = new DbColumnValues();
                    newValues["id"] = 8;
                    newValues["name"] = "de-gcd-2012-06-01";
                    newValues["path"] = "Taxonomy\\base\\de-gcd-2012-06-01";
                    newValues["filename"] = "de-gcd-2012-06-01-shell.xsd";
                    newValues["type"] = 1;
                    newValues["version"] = "2012-06-01";
                    _conn.InsertInto("taxonomy_info", newValues);

                    newValues["id"] = 9;
                    newValues["name"] = "de-gaap-2012-06-01";
                    newValues["path"] = "Taxonomy\\base\\de-gaap-ci-2012-06-01";
                    newValues["filename"] = "de-gaap-ci-2012-06-01-shell-fiscal.xsd";
                    newValues["type"] = 2;
                    newValues["version"] = "2012-06-01";
                    _conn.InsertInto("taxonomy_info", newValues);

                    newValues["id"] = 10;
                    newValues["name"] = "de-bra-2012-06-01";
                    newValues["path"] = "Taxonomy\\base\\de-bra-2012-06-01";
                    newValues["filename"] = "de-bra-2012-06-01-shell-fiscal.xsd";
                    newValues["type"] = 3;
                    newValues["version"] = "2012-06-01";
                    _conn.InsertInto("taxonomy_info", newValues);

                    newValues["id"] = 11;
                    newValues["name"] = "de-fi-2012-06-01";
                    newValues["path"] = "Taxonomy\\fi\\de-fi-2012-06-01";
                    newValues["filename"] = "de-fi-2012-06-01-shell-staffelform-fiscal.xsd";
                    newValues["type"] = 4;
                    newValues["version"] = "2012-06-01";
                    _conn.InsertInto("taxonomy_info", newValues);
                    
                    newValues["id"] = 12;
                    newValues["name"] = "de-ins-2012-06-01";
                    newValues["path"] = "Taxonomy\\ins\\de-ins-2012-06-01";
                    newValues["filename"] = "de-ins-2012-06-01-shell-fiscal.xsd";
                    newValues["type"] = 5;
                    newValues["version"] = "2012-06-01";
                    _conn.InsertInto("taxonomy_info", newValues);
                    
                    _conn.CommitTransaction();
                }
                catch (Exception ex) {
                    //conn.RollbackTransaction(); 
                    throw new Exception(ex.Message);
                }
                Info.DbVerion = "1.5.9";
            } catch (Exception) {
                // Ups
                    if(_conn.HasTransaction())
                        _conn.RollbackTransaction();
                throw;
            }
        }
        #endregion


        #region UpgradeTo_1_6_0
        private void UpgradeTo_1_6_0() {

            string key = "Ugrade_1_6_0";
            const string version = "1.6.0";
            string progressStr = Info.GetValue(key);
            int curProgress = (progressStr == null ? 0 : Int32.Parse(progressStr));

            Progress.Maximum = 5;
            Progress.Value = curProgress;


            if (curProgress <= 1) {

                //// Get all tables that should be at the database after executing this update
                //var newTables = DatabaseCreator.Instance.GetTableNames(version, _conn.DbConfig.DbType);
                //// Remove the tables from the list that are already created by the update to the version before this one
                //newTables.RemoveAll(
                //        DatabaseCreator.Instance.GetTableNames(VersionInfo.Instance.GetPreviousDbVersion(version),
                //                                               _conn.DbConfig.DbType).Contains);

                //Debug.Assert(!newTables.Where(_conn.GetTableList().Contains).Any(),
                //             "There are already tables that should not be there. (not created by the previous upgrade)",
                //             string.Join(Environment.NewLine, newTables.Where(_conn.GetTableList().Contains)));

                //System.Diagnostics.Debug.WriteLine("var newTables = new[] {\"" + string.Join("\",\"", newTables) + "\"};");
                //Debug.WriteLine(Environment.NewLine);


                var newTables = new[] {
                    "templates_account_groups", "templates_account_splittings",
                    "reconciliations", "reconciliation_transactions",
                    "hypercube_dimension_keys", "hyper_cube_dimension_ordinals",
                    "report_federal_gazette", "hypercube_items", "hypercube_dimensions",
                    "values_gaap_fg", "hypercubes", "federal_gazette_info",
                    //"log_report_1","log_report_1_value_change", 
                    "hypercube_import_templates", "upgrade_information"
                };
                //Create new tables
                //var tables = new[] {
                //    "hyper_cube_dimension_ordinals", "hypercube_dimension_keys", "hypercube_dimensions",
                //    "hypercube_import_templates", "hypercube_items", "hypercubes",
                //    "reconciliation_transactions", "reconciliations",
                //    "report_federal_gazette", "values_gaap_fg"
                //};

                foreach (string table in newTables) {
                    _conn.DropTableIfExists(table);
                    if (DatabaseCreator.Instance.GetTableNames(version, _conn.DbConfig.DbType).Contains(table))
                        DatabaseCreator.Instance.CreateTable(version, _conn.DbConfig.DbType, _conn, table);
                }

                UpdateProgress(_conn, key, ref curProgress);
            }

            #region log_send

            var tableName = "log_send";

            if (curProgress <= 2) {
                try {
                    _conn.BeginTransaction();
                    curProgress =
                        CreateNewTablesAndCopyDataFromOld(curProgress, curProgress, key, version,
                                                          new List<string>() {tableName}, null);
                    _conn.CommitTransaction();
                } catch (Exception ex) {
                    _conn.RollbackTransaction();
                    throw new Exception(ex.Message);
                }
            }


            #endregion

            if (curProgress <= 10) {
                try {
                    //var changes = new TableCopyStructure("transfer_hbst_lines", "reconciliation_transactions",
                    //                                     new Dictionary<string, string>() {
                    //                                         {"id", null},
                    //                                         {"transfer_value", "value"},
                    //                                         {"document_id", null},
                    //                                         {"header_id", "reconciliation_id"},
                    //                                         {"value_id", "element_id"},
                    //                                         //{"123", "transaction_type1"}
                    //                                         // 123 as value because this info didn't exist before
                    //                                     });
                    _conn.BeginTransaction();
                    //curProgress = CreateNewTablesAndCopyDataFromAnOther(curProgress, curProgress, key, "1.6.0", changes, false);


                    UpdateProgress(_conn, key, ref curProgress);

                    ReconciliationUpdater16.UpgradeReconTables(_conn);
                    UpdateProgress(_conn, key, ref curProgress);
                    
                    ReconciliationUpdater16.UpgradePreviousYearValues(_conn);
                    UpdateProgress(_conn, key, ref curProgress);

                    _conn.DropTableIfExists("transfer_hbst_heads");
                    UpdateProgress(_conn, key, ref curProgress);
                    
                    _conn.DropTableIfExists("transfer_hbst_lines");
                    UpdateProgress(_conn, key, ref curProgress);

                    _conn.CommitTransaction();
                } catch (Exception ex) {
                    if(_conn.HasTransaction())
                        _conn.RollbackTransaction();
                    throw;
                }
            } // curProgress should be 15

            //if (curProgress <= 15) {
            //    try {
            //    } catch (Exception ex) {
            //        _conn.RollbackTransaction();
            //        throw;
            //    }
            //}

            if (curProgress <= 20) {
                InvariantCultureUpdater updater = new InvariantCultureUpdater(_conn);
                List<string> columnsToUpdate = new List<string>() {"value", "cb_value_other"};
                updater.UpdateTaxonomyTable("values_gaap", columnsToUpdate, "id");
                updater.UpdateTaxonomyTable("values_gcd", columnsToUpdate, "id");
                updater.UpdateTaxonomyTable("values_gcd_company", columnsToUpdate, "id");

                columnsToUpdate = new List<string>() {"old_value", "new_value"};
                //Update all log tables (need to read the report ids from the database)
                List<long> ids = new List<long>();
                using (var reader = _conn.ExecuteReader(string.Format("SELECT {0} FROM {1}", _conn.Enquote("id"), _conn.Enquote("documents")))) {
                    while (reader.Read()) {
                        long id = Convert.ToInt64(reader[0]);
                        ids.Add(id);
                    }
                }
                foreach (var id in ids) {
                    string logReportTableName = string.Format("log_report_{0}", id);
                    string logValueChangeTableName = string.Format("log_report_{0}_value_change", id);
                    if (_conn.TableExists(logReportTableName))
                        updater.UpdateCommonTable(logReportTableName, columnsToUpdate, "id");
                    if (_conn.TableExists(logValueChangeTableName))
                        updater.UpdateCommonTable(logValueChangeTableName, columnsToUpdate, "id");
                }
                UpdateProgress(_conn, key, ref curProgress);
            }

            //User table changed
            if (curProgress <= 21) {
                var replaceColumns = new Dictionary<string, string> {
                    {"is_companyadmin", "0"},
                    {"assigned_companies", "''"},
                    {"last_login", "NULL"}
                };
                curProgress = CreateNewTablesAndCopyDataFromOld(curProgress, curProgress, key, "1.6.0",
                                                                new List<string> { "users" },
                                                                new Dictionary<string, List<string>>(),
                                                                replaceColumns, false, false);
            }

            if (curProgress <= 25) {
                var replaceColumns = new Dictionary<string, string> { { "template", "''"} };
                curProgress = CreateNewTablesAndCopyDataFromOld(curProgress, curProgress, key, "1.6.0",
                                                                new List<string> { "templates_balance_list" },
                                                                new Dictionary<string, List<string>>(),
                                                                replaceColumns, false, false);
            }

            if (curProgress <= 29) {
                DbColumnValues values = new DbColumnValues {
                    Columns =
                        {"ordinal", "upgrade_available_from", "resource_name", "version_string", "id"}
                };
                values["ordinal"] = 1.6d;
                values["upgrade_available_from"] = new DateTime(2012, 06, 22, 11, 23, 0);
                values["resource_name"] = "dummy.pdf";
                values["version_string"] = "1.6";
                values["id"] = 1;
                _conn.InsertInto("upgrade_information", values);
                UpdateProgress(_conn, key, ref curProgress);
            }

            // documents table changed
            if (curProgress <= 30) {
                var replaceColumns = new Dictionary<string, string> {
                    {"reconciliation_mode", "0"}
                };
                curProgress = CreateNewTablesAndCopyDataFromOld(curProgress, curProgress, key, "1.6.0",
                                                                new List<string> { "documents" },
                                                                new Dictionary<string, List<string>>(),
                                                                replaceColumns, false, false);
                UpdateProgress(_conn, key, ref curProgress);
            }
            // Update values_gcd SET value = value.replace("de-gcd_") WHERE value.StartsWith("de-gcd_") 

//            throw new NotImplementedException();
        } 
        #endregion

        #endregion methods

        private Info Info { get; set; }
    }
}