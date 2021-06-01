using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.IO;
using System.Linq;
using System.Threading;
using DbAccess;
using DbAccess.Structures;
using DbSearchDatabase.DistinctDb;
using DbSearchDatabase.DistinctDb.Events;
using MySql.Data.MySqlClient;
using log4net;
using AV.Log;

namespace DbSearchDatabase.DistinctDb {
    public class DbDistincter {

        internal ILog _log = LogHelper.GetLogger();

        private enum DistinctColumnResultEnum {
            Success = 0,
            Failed = 1,
            HasNoRowNo = 2,
        }

        private object _lock;

        // List of tables, which should be processed.
        private List<DistinctTable> _tables;
        private List<DistinctColumn> _columns;
        private TableWorkManager _tableWorkManager = new TableWorkManager();
        #region constants 

        //private int THREAD_COUNT = (int)(Environment.ProcessorCount * 0.5);
        //private const int THREAD_COUNT = 1;
        private int _threadCount = 1;
        private const string ROW_NO_COLUMN = "_row_no_";
        //private const int MAX_ROWS_PER_STREAM = 1000000;
        public const int MAX_ROWS_PER_STREAM = 100000;
        public const string DISTINCT_DB_SUFFIX = "_idx";

        #endregion constants

        /******************************************************************************/

        #region constructors
        /// <summary>
        /// Initializes a new instance of the <see cref="DbDistincter"/> class.
        /// </summary>
        /// <param name="configDbSrc"> </param>
        /// <param name="threadCount"> </param>
        /// <param name="relevantTables"> </param>
        /// <param name="configDbSrc[threadId]">The config db SRC.</param>
        public DbDistincter(DbConfig configDbSrc, int threadCount, List<string> relevantTables = null) {
            _threadCount = threadCount;
            this.ConfigDbSrc = configDbSrc;
            this.ConfigDbDst = (DbConfig) configDbSrc.Clone();
            this.ConfigDbDst.DbName = configDbSrc.DbName + DISTINCT_DB_SUFFIX;
            if(relevantTables != null)
                _tables = (from table in relevantTables select new DistinctTable(table)).ToList();

            DbSrc = null;
            DbDst = null;

            _lock = new object();
        }

        #endregion constructors

        /******************************************************************************/

        #region properties

        /// <summary>
        /// Gets or sets the config for the source db.
        /// </summary>
        /// <value>The config for the sourcedb.</value>
        private DbConfig ConfigDbSrc { get; set; }

        /// <summary>
        /// Gets or sets the config dest db.
        /// </summary>
        /// <value>The config dest db.</value>
        private DbConfig ConfigDbDst { get; set; }

        /// <summary>
        /// Gets or sets the source db.
        /// </summary>
        /// <value>The source database.</value>
        private IDatabase[] DbSrc { get; set; }

        /// <summary>
        /// Gets or sets the dest db.
        /// </summary>
        /// <value>The destination database.</value>
        private IDatabase[] DbDst { get; set; }

        #endregion properties

        /******************************************************************************/

        #region events

        /// <summary>
        /// Occurs when process has been started.
        /// </summary>
        public event EventHandler<DbDistincterStartEvent> Starting;
       
        /// <summary>
        /// Occurs when an error occurs.
        /// </summary>
        public event EventHandler<DbDistincterPerformActionEvent> Error;
        
        /// <summary>
        /// Occurs when the process has been finished.
        /// </summary>
        public event EventHandler<DbDistincterFinishedEvent> Finished;
        
        /// <summary>
        /// Occurs when a new table is processed.
        /// </summary>
        public event EventHandler<DbDistincterStartTableEvent> StartTable;
        
        /// <summary>
        /// Occurs when a new column is processed.
        /// </summary>
        public event EventHandler<DbDistincterStartColumnEvent> StartColumn;

        /// <summary>
        /// Called when the process has been started.
        /// </summary>
        /// <param name="totalTableCount">The total table count.</param>
        public void OnStart(int totalTableCount) {
            if (Starting != null) Starting(this, new DbDistincterStartEvent(totalTableCount));
        }

        /// <summary>
        /// Called when an error occurs.
        /// </summary>
        /// <param name="error">The error.</param>
        public void OnError(string error) {
            if (Error != null) Error(this, new DbDistincterPerformActionEvent(error));
        }

        /// <summary>
        /// Called when the process has been finished.
        /// </summary>
        /// <param name="start">The start time.</param>
        /// <param name="end">The end time.</param>
        private void OnFinished(DateTime start, DateTime end) {
            if (Finished != null) Finished(this, new DbDistincterFinishedEvent(start, end));
        }

        /// <summary>
        /// Called when a new table is processed..
        /// </summary>
        /// <param name="tableName">Name of the table.</param>
        public void OnStartTable(string tableName) {
            if (StartTable != null) StartTable(this, new DbDistincterStartTableEvent(tableName));
        }

        /// <summary>
        /// Called when a new column is processed.
        /// </summary>
        /// <param name="columnName">Name of the column.</param>
        public void OnStartColumn(string columnName) {
            if (StartColumn != null) StartColumn(this, new DbDistincterStartColumnEvent(columnName));
        }

        #endregion events

        /******************************************************************************/

        #region methods

        /// <summary>
        /// Starts the process.
        /// </summary>
        public void Start(bool updateDatabase) {
            try {

                // Initialize connections and creates the destination database and neccesary tables if they does not yet exist.
                if (!InitConnections()) {
                    ShutdownConnections();
                    return;
                }
                
                if (DbDst[0].TableExists("columns") && !DbDst[0].GetColumnNames("columns").Contains("started")) {
                    if (updateDatabase) {
                        DbDst[0].DbMapping.AddColumn("columns", "DateTime", new DbColumnAttribute("started"),null );
                        if(!DbDst[0].GetColumnNames("columns").Contains("finished"))
                            DbDst[0].DbMapping.AddColumn("columns", "DateTime", new DbColumnAttribute("finished"), null);
                        if(!DbDst[0].GetColumnNames("tables").Contains("started"))
                            DbDst[0].DbMapping.AddColumn("tables", "DateTime", new DbColumnAttribute("started"), null, "count");
                    }
                    else
                        throw new Exception(
                            "Die Distinct-Datenbank ist zu alt, sie muss neu erstellt werden, da das Fortsetzen bei der veralteten Version nicht möglich ist.");
                }
                DistinctTableInfo.CreateTable(DbDst[0]);
                DistinctColumnInfo.CreateTable(DbDst[0]);

                DateTime start = System.DateTime.Now;

                // init list of relevant tables if none is present
                if (_tables == null) _tables = GetRelevantTables();
                _columns = GetRelevantColumns(_tables);
                OnStart(_columns.Count);

                ManualResetEvent[] doneEvents = new ManualResetEvent[_threadCount];
                if (_threadCount == 1) {
                    doneEvents[0] = new ManualResetEvent(false);
                    DistincterThread(new object[] { 0, doneEvents[0] });
                } else {
                    for (int i = 0; i < _threadCount; i++) {
                        doneEvents[i] = new ManualResetEvent(false);
                        Thread worker = new Thread(DistincterThread);
                        worker.Start(new object[] { i, doneEvents[i] });
                    }

                    // Wait until all thread finished.
                    foreach (var handle in doneEvents)
                        handle.WaitOne();
                }
                //WaitHandle.WaitAll(doneEvents);

                OnFinished(start, System.DateTime.Now);
            } finally {
                ShutdownConnections();
            }


        }

        private List<DistinctColumn> GetRelevantColumns(IEnumerable<DistinctTable> tables) {
            if (DbSrc[0].DatabaseExists(DbSrc[0].DbConfig.DbName + "_system")) {
                string sql = "SELECT name, table_id FROM " + DbSrc[0].Enquote(DbSrc[0].DbConfig.DbName + "_system", "col");
                List<DistinctColumn> columns = new List<DistinctColumn>();
                using(IDataReader oReader = DbSrc[0].ExecuteReader(sql)) {

                    while (oReader.Read()) {
                        int id = oReader.GetInt32(1);
                        var table = tables.FirstOrDefault(t => t.Id == id);
                        string colName = oReader.GetString(0);
                        //table may be null as only tables with system_id == 1 are loaded
                        if(table != null)
                            columns.Add(new DistinctColumn(table, colName));
                    }

                }
                return columns;

            } else {
                List<DistinctColumn> columns = new List<DistinctColumn>();
                foreach(var table in tables) {
                    foreach(var col in DbSrc[0].GetColumnNames(table.Name))
                        columns.Add(new DistinctColumn(table, col));

                }
                return columns;
            }
        }

        /// <summary>
        /// Gets the relevant tables. A table is considered as relevant, if it contains a row_number column. 
        /// 
        /// If the database is an optimized database, e.g. if a database xxx_system could be found, only tables 
        /// with the system_id 1 are returned. Usually these are all tables from the original database, whereas 
        /// system_id 2 is reserved for generated views.
        /// </summary>
        /// <returns></returns>
        private List<DistinctTable> GetRelevantTables() {
            try {
                if (DbSrc[0].DatabaseExists(DbSrc[0].DbConfig.DbName + "_system")) {
                    string sql =
                        "SELECT t." + DbSrc[0].Enquote("TABLE_NAME") + ", t2.table_id " +
                        "FROM " + DbSrc[0].Enquote("INFORMATION_SCHEMA", "TABLES") + " t " +
                        "INNER JOIN " + DbSrc[0].Enquote(DbSrc[0].DbConfig.DbName + "_system", "TABLE") + " t2 " +
                        "ON " + "t." + DbSrc[0].Enquote("TABLE_NAME") + " = " + "t2." + DbSrc[0].Enquote("NAME") + " " +
                        "WHERE t." + DbSrc[0].Enquote("TABLE_SCHEMA") + " = '" + DbSrc[0].DbConfig.DbName + "' " +
                        "AND " + DbSrc[0].Enquote("SYSTEM_ID") + " = " + "1 " +
                        "AND TABLE_ROWS > 0 " +
                        "ORDER BY TABLE_ROWS DESC";
                    List<DistinctTable> tables = new List<DistinctTable>();
                    using (IDataReader oReader = DbSrc[0].ExecuteReader(sql)) {

                        while (oReader.Read()) {
                            tables.Add(new DistinctTable(oReader.GetString(0), oReader.GetInt32(1)));
                        }

                    }
                    return tables;

                }
                else {
                    return (from table in DbSrc[0].GetTableList() select new DistinctTable(table)).ToList();
                }
            } catch (Exception ex) {
                OnError("Fehler beim Analysieren der relevanten Tabellen: " + Environment.NewLine + ex.Message);
                return new List<DistinctTable>();
            }
        }

        /// <summary>
        /// Initializes the connections.
        /// </summary>
        private bool InitConnections() {
            
            try {
                DbSrc = new IDatabase[_threadCount];
                for (int i = 0; i < _threadCount; i++) {
                    DbSrc[i] = ConnectionManager.CreateConnection(ConfigDbSrc);
                    DbSrc[i].Open();
                    MySqlCommand cmd = new MySqlCommand("set net_write_timeout=99999; set net_read_timeout=99999", (MySqlConnection)DbSrc[i].Connection); // Setting timeout on mysqlServer
                    cmd.ExecuteNonQuery();
                }

                // Create index-database if not already exists
                DbSrc[0].CreateDatabaseIfNotExists(ConfigDbDst.DbName);

                DbDst = new IDatabase[_threadCount];
                for (int i = 0; i < _threadCount; i++) {
                    DbDst[i] = ConnectionManager.CreateConnection(ConfigDbDst);
                    DbDst[i].Open();
                    DbDst[i].ExecuteNonQuery("SET GLOBAL max_allowed_packet = 1073741824");
                    MySqlCommand cmd = new MySqlCommand("set net_write_timeout=99999; set net_read_timeout=99999", (MySqlConnection)DbDst[i].Connection); // Setting timeout on mysqlServer
                    cmd.ExecuteNonQuery();
                }

                // Set package-size. This value must be increased to save the row-number arrays to the database.
                //DbDst[0].ExecuteNonQuery("SET GLOBAL max_allowed_packet = 1073741824");

                return true;
            
            } catch (Exception ex) {
                OnError("Fehler beim Initialisieren der Datenbankverbindungen: " + Environment.NewLine + ex.Message);
                return false;
            }
        }

        /// <summary>
        /// Shutdowns the connections.
        /// </summary>
        private void ShutdownConnections() {
            // Shutdown connection to source database.
            for (int i = 0; i < _threadCount; i++) {
                try {
                    if (DbSrc[i] != null) {
                        if (DbSrc[i].IsOpen) DbSrc[i].Close();
                        DbSrc[i].Dispose();
                    }

                } catch (Exception ex) {
                    OnError(ex.Message);
                }

                // Shutdown connection to destination database.
                try {
                    if (DbDst[i] != null) {
                        if (DbSrc[i].IsOpen) DbSrc[i].Close();
                        DbSrc[i].Dispose();
                    }

                } catch (Exception ex) {
                    OnError(ex.Message);
                }
            }
        }

        #region TestConnections

        /// <summary>
        /// Tests the connections.
        /// </summary>
        /// <returns>True iif. source and dest connection is avaliable.</returns>
        private bool TestConnections(int threadId) {

            // test source connection and try to reinit if not succeed
            if (!DbSrc[threadId].IsOpen) {
                int remainingTries = 3;
                while (!DbSrc[threadId].IsOpen && (remainingTries-- > 0)) {
                    DbSrc[threadId].Reconnect();
                }

                // check if the reconnection succeed
                if((remainingTries == 0) && !DbSrc[threadId].IsOpen) return false;
            }

            // test destination connection and try to reinit if not succeed
            if (!DbDst[threadId].IsOpen) {
                int remainingTries = 5;
                while (!DbDst[threadId].IsOpen && (remainingTries-- > 0)) {
                    DbSrc[threadId].Reconnect();
                }

                // check if the reconnection succeed
                if ((remainingTries == 0) && !DbDst[threadId].IsOpen) return true;
            }

            return true;
        }
        #endregion TestConnections

        private void DistincterThread(object paramArr) {
            int threadId = (int)((object[])paramArr)[0];
            ManualResetEvent doneEvent = (ManualResetEvent)((object[])paramArr)[1];

            string tableToSkipp = null;
            DistinctColumn column;
            while ((column = GetNextColumn()) != null) {
                if (column.Table.Name == tableToSkipp) continue;
                DistinctColumnResultEnum result = DistinctColumn(threadId, column);
                if (result == DistinctColumnResultEnum.HasNoRowNo) {
                    tableToSkipp = column.Table.Name;
                    _log.Log(LogLevelEnum.Error, string.Format("Table has no [{0}] column, skipping table [{1}] !", ROW_NO_COLUMN, column.Table.Name));
                } else {
                    tableToSkipp = null;
                    if (result == DistinctColumnResultEnum.Failed)
                        _log.Log(LogLevelEnum.Error, string.Format("Distinction of column [{0}] in table [{1}] failed !", column.Name, column.Table.Name));
                }
            }

            doneEvent.Set();
        }

        private DistinctColumnResultEnum DistinctColumn(int threadId, DistinctColumn column) {
            DistinctColumnResultEnum result = DistinctColumnResultEnum.Success;
            DistinctColumnInfo columnInfo = _tableWorkManager.GetColumnInfo(column, DbSrc[threadId], DbDst[threadId]);
            if (columnInfo != null) {
                columnInfo.Started = DateTime.Now;
                columnInfo.Save(DbDst[threadId]);

                result = DistinctColumn(threadId, columnInfo.DbColumnInfo, columnInfo.TableInfo);
                _tableWorkManager.ColumnFinished(columnInfo, DbDst[threadId]);
            }
            return result;
        }

        private DistinctColumn GetNextColumn() {
            DistinctColumn column = null;

            lock (_columns) {
                if (_columns.Count > 0) {
                    column = _columns[0];
                    _columns.RemoveAt(0);
                }
            }

            return column;
            
        }

        /// <summary>
        /// Distincts the table.
        /// </summary>
        /// <param name="table">The table.</param>
        private void DistinctTable(int threadId, string table) {

            // if (!table.Equals("dbo_ledgertrans")) return;

            try {
                TestConnections(threadId); 
                
                OnStartTable(table);


                DistinctTableInfo ti = DistinctTableInfo.OpenTableInfo(DbDst[threadId], table);
                if (ti == null) {
                    // create new table info
                    ti = new DistinctTableInfo(table);
                    ti.Count = (uint)DbDst[threadId].CountTable(ConfigDbSrc.DbName, table);
                    ti.Save(DbDst[threadId]);
                } else if (ti.Count != DbDst[threadId].CountTable(DbDst[threadId].DbConfig.DbName.Substring(0, DbDst[threadId].DbConfig.DbName.IndexOf(DISTINCT_DB_SUFFIX)), table)) {
                    // truncate the columns, datasets changed
                    string sSql = "SELECT `cid` FROM `columns` WHERE `tid` = " + ti.Id;
                    IDataReader oReader = DbDst[threadId].ExecuteReader(sSql);
                    List<string> lColumnIds = new List<string>();
                    while (oReader.Read()) {
                        lColumnIds.Add(oReader[0].ToString());
                    }
                    oReader.Close();
                    foreach(string sColumnId in lColumnIds){
                        sSql = "DROP TABLE IF EXISTS `idx_" + sColumnId + "`";
                        DbDst[threadId].ExecuteNonQuery(sSql);
                        sSql = "DROP TABLE IF EXISTS `rowno_" + sColumnId + "`";
                        DbDst[threadId].ExecuteNonQuery(sSql);
                    }
                    sSql = "DELETE FROM `columns` WHERE `tid` = " + ti.Id;
                    DbDst[threadId].ExecuteNonQuery(sSql);
                    sSql = "DELETE FROM `tables` WHERE `tid` = " + ti.Id;
                    DbDst[threadId].ExecuteNonQuery(sSql);

                    ti = new DistinctTableInfo(table);
                    ti.Count = (uint)DbDst[threadId].CountTable(ConfigDbSrc.DbName, table);
                    ti.Save(DbDst[threadId]);
                }

                // Process columns.
                DistinctColumnResultEnum result = DistinctColumnResultEnum.Success;
                foreach (var columnInfo in DbSrc[threadId].GetColumnInfos(table)) {
                    if (columnInfo.Name != ROW_NO_COLUMN) {
                        if (ti.Columns.ContainsKey(columnInfo.Name)) {
                            if (ti.Columns[columnInfo.Name].Size > 0) continue;
                        }
                        result = DistinctColumn(threadId, columnInfo, ti);
                        if (result == DistinctColumnResultEnum.HasNoRowNo) break;
                        else if (result == DistinctColumnResultEnum.Failed)
                            _log.Log(LogLevelEnum.Error, string.Format("Distinction of column [{0}] in table [{1}] failed !", columnInfo.Name, ti.Name));
                    }
                }
                if (result == DistinctColumnResultEnum.HasNoRowNo)
                    _log.Log(LogLevelEnum.Error, string.Format("Table has no [{0}] column, skipping table [{1}] !", ROW_NO_COLUMN, ti.Name));

            } catch (Exception ex) {
                OnError("Error while distinction of table [" + table + "]" + Environment.NewLine + ex.Message);
            }
        }

        /// <summary>
        /// Distincts the column.
        /// </summary>
        /// <param name="table">The table.</param>
        /// <param name="column">The column.</param>
        /// <param name="tableId">The table id.</param>
        private DistinctColumnResultEnum DistinctColumn(int threadId, DbColumnInfo column, DistinctTableInfo ti, bool secondTry = false) {

            DistinctColumnInfo ci = null;
            try {
                TestConnections(threadId);
                //DbFieldInfo fieldInfoValue = DbSrc[threadId].GetFieldInfo(ti.Name, column);

                // Ignore BLOB colums
                //if (column.OriginalType.ToUpper().Contains("BLOB")) return;
                if (column.Type == DbColumnTypes.DbBinary || column.Type == DbColumnTypes.DbUnknown) return DistinctColumnResultEnum.Success;
                
                OnStartColumn(column.Name);

                // open existing column info
                if (ti.Columns.ContainsKey(column.Name)) {
                    ci = ti.Columns[column.Name];
                    if (ci.Size != 0) {
                        ci.Size = 0;
                        ci.Save(DbDst[threadId]);
                    }
                } else {
                    // create new column info
                    ci = new DistinctColumnInfo(ti, column.Name);
                    ti.Columns.Add(ci.Name, ci);
                    ci.Save(DbDst[threadId]);
                }

                DistinctIndexTable idxTable = new DistinctIndexTable(DbDst[threadId].DbConfig.DbName, ci);
                idxTable.Create(DbDst[threadId], DbSrc[threadId].DbConfig.DbName);
                ci.Save(DbDst[threadId]); // update IndexTableHasIdColumn value

                // Insert values.
                PopulateIndexTable(threadId, ci, idxTable);
                return DistinctColumnResultEnum.Success;
            } catch (Exception ex) {
                MySqlException mySqlException = ex.InnerException as MySqlException;
                if (mySqlException != null) {
                    // handles unknown column exception, when no _row_no_ column found
                    if (mySqlException.Number == 1054 && mySqlException.Message.Contains(ROW_NO_COLUMN)) {
                        _log.Log(LogLevelEnum.Error, string.Format("Table [{1}] has no [{0}] column!", ROW_NO_COLUMN, ti.Name));
                        SetColumnInfoSizeToNull(threadId, ci);
                        return DistinctColumnResultEnum.HasNoRowNo;
                    }
                }
                _log.Log(LogLevelEnum.Error, "Fehler bei Spalte '" + ti.Name + "'.'" + column.Name + "', noch ein Versuch: " + !secondTry + Environment.NewLine + ex.Message);
                if (!secondTry) {
                    DbSrc[threadId] = ConnectionManager.CreateConnection(DbSrc[threadId].DbConfig);
                    DbSrc[threadId].Open();
                    MySqlCommand cmd = new MySqlCommand("set net_write_timeout=99999; set net_read_timeout=99999", (MySqlConnection)DbSrc[threadId].Connection); // Setting tiimeout on mysqlServer
                    cmd.ExecuteNonQuery();
                    return DistinctColumn(threadId, column, ti, true);
                } else {
                    SetColumnInfoSizeToNull(threadId, ci);
                    return DistinctColumnResultEnum.Failed;
                }
            }
        }

        /// <summary>
        /// Sets the size of distinct column ot zero
        /// </summary>
        /// <param name="threadId">The thread id</param>
        /// <param name="ci">The distinct column</param>
        private void SetColumnInfoSizeToNull(int threadId, DistinctColumnInfo ci) {
            if (ci != null) {
                if (ci.Size != 0) {
                    ci.Size = 0;
                    ci.Save(DbDst[threadId]);
                }
            }
        }

        /// <summary>
        /// Populates the index table.
        /// </summary>
        /// <param name="table">The table.</param>
        /// <param name="column">The column.</param>
        /// <param name="columnId">The column id.</param>
        /// <param name="tableId">The table id.</param>
        private void PopulateIndexTable(int threadId, DistinctColumnInfo ci, DistinctIndexTable idxTable, bool secondTry = false) {

            object lastValue = null;
            object value = null;
            long rowNumber = 0;
            int rowCounterPerStream = 0;
            List<UInt32> rowNumberIds = new List<uint>();

            try {
                string sql =
                    "SELECT " + DbDst[threadId].Enquote(ci.Name) + ", " + DbDst[threadId].Enquote(ROW_NO_COLUMN) + " " +
                    "FROM " + DbDst[threadId].Enquote(ci.TableInfo.Name) + " " +
                    "ORDER BY " + DbDst[threadId].Enquote(ci.Name);

                // close connection to dest-db, since the following operation needs several hours when processing huge tables
                //if (DbDst[threadId].IsOpen) DbDst[threadId].Close();

                ulong size = 0;

                using (IDataReader oReader = DbSrc[threadId].ExecuteReader(sql)) {

                    // reopen connection to dest-db
                    //if (!DbDst[threadId].IsOpen) DbDst[threadId].Open();

                    using (MemoryStream memStream = new MemoryStream()) {

                        while (oReader.Read()) {

                            // Ignore null values
                            if (oReader.IsDBNull(0)) continue;

                            // Read values
                            if (oReader.GetFieldType(0) == typeof(TimeSpan)) {
                                value = oReader.GetValue(0).ToString();
                            } else {
                                value = oReader.GetValue(0);
                            }
                            rowNumber = (uint)oReader.GetInt64(1);

                            if (lastValue == null) {
                                // Save row-number of new value (case occurs only for the first dataset).
                                memStream.Write(BitConverter.GetBytes(rowNumber), 0, 4);
                                lastValue = value;
                                continue;

                            } else if (value.Equals(lastValue)) {

                                // value already exists, save row number
                                if (rowCounterPerStream >= MAX_ROWS_PER_STREAM) {

                                    // Save rownumber array to db since the memory stream contains at least MAX_ROWS_PER_STREAM row´s.
                                    rowNumberIds.Add(idxTable.InsertRowNumber(DbDst[threadId], memStream));

                                    // Reset the row-counter.
                                    rowCounterPerStream = 0;
                                }

                                // Add row-number to memory stream.
                                memStream.Write(BitConverter.GetBytes(rowNumber), 0, 4);
                                rowCounterPerStream++;

                            } else {
                                // New value found, save old memstream to db.
                                InsertValue(threadId, idxTable, memStream, lastValue, rowNumberIds, ref size);

                                //Reset row-number list.
                                rowNumberIds.Clear();

                                // Save row-number of new value.
                                memStream.Write(BitConverter.GetBytes(rowNumber), 0, 4);
                                rowCounterPerStream = 1;
                            }

                            lastValue = value;
                        }

                        if (memStream.Length != 0) {
                            // Save unsaved row-numbers.
                            InsertValue(threadId, idxTable, memStream, lastValue, rowNumberIds, ref size);
                        }
                    }

                    // Close data reader.
                    try {
                        oReader.Close();
                    } catch (Exception exInner) {
                        OnError("Fehler beim Schließen des DataReader, Datenbankverbindungen werden neu aufgebaut." + Environment.NewLine + exInner.Message);

                        DbSrc[threadId].Close();
                        DbDst[threadId].Close();

                        DbSrc[threadId].Open();
                        DbDst[threadId].Open();
                    }

                    ci.Size = (long) size;
                    ci.Save(DbDst[threadId]);
                }

            }  finally {
                if (!DbSrc[threadId].IsOpen) DbSrc[threadId].Open();
                if (!DbDst[threadId].IsOpen) DbDst[threadId].Open();
            }
        }

        /// <summary>
        /// Inserts the value.
        /// </summary>
        /// <param name="idxTable">The idx table.</param>
        /// <param name="memStream">The mem stream.</param>
        /// <param name="value">The value.</param>
        /// <param name="rowNumberIds">The row number ids.</param>
        /// <param name="size">The size.</param>
        private void InsertValue(
            int threadId,
            DistinctIndexTable idxTable, 
            MemoryStream memStream, 
            object value, 
            List<UInt32> rowNumberIds, 
            ref ulong size) {

            try {
                switch (value.GetType().Name) {
                    case "String":
                        size += (ulong)((String)value).Length;
                        break;

                    case "Byte":
                    case "SByte":
                        size += 1;
                        break;

                    case "UInt16":
                    case "Int16":
                        size += 2;
                        break;

                    case "Int32":
                    case "UInt32":
                    case "Single":
                        size += 4;
                        break;

                    case "Double":
                    case "Int64":
                    case "UInt64":
                    case "TimeSpan":
                        size += 8;
                        break;

                    case "Decimal":
                    case "DateTime":
                    case "MySqlDateTime":
                        size += 16;
                        break;

                    default:
                        // Type unknown, raise warning message and add avarage size.
                        OnError("Warnung, unbekannter Typ bei Größenberechnung gefunden: " + value.GetType().Name);
                        size += 8;
                        break;
                }

                idxTable.InsertValue(DbDst[threadId], memStream, value, rowNumberIds);
            } catch (Exception ex) {
                throw ex;
            }
        }

        #endregion methods
    }
}
