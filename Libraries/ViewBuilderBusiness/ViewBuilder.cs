using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.Common;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using SystemDb;
using SystemDb.Internal;
using AV.Log;
using DbAccess;
using DbAccess.Structures;
using ProjectDb.Tables;
using Utils;
using ViewBuilderBusiness.EventArgs;
using ViewBuilderBusiness.Exceptions;
using ViewBuilderBusiness.Resources;
using ViewBuilderBusiness.Structures;
using ViewBuilderBusiness.Structures.Config;
using ViewBuilderCommon;
using log4net;
using Index = ViewBuilderCommon.Index;
using View = ProjectDb.Tables.View;

namespace ViewBuilderBusiness
{
    public class ViewCreateOptions
    {
        public bool GenerateDistinctDataOnly { get; set; }
    }

    /// <summary>
    ///   This class implements the viewbuilder business logic.
    /// </summary>
    public class ViewBuilder : INotifyPropertyChanged
    {
        internal static ILog _log = LogHelper.GetLogger();

        #region events

        /// <summary>
        ///   Occurs when a property changed
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        ///   Occurs when an error occured.
        /// </summary>
        public event EventHandler Error;

        /// <summary>
        ///   Occurs when the process finished.
        /// </summary>
        public event EventHandler Finished1;

        /// <summary>
        ///   Occurs when [worker started].
        /// </summary>
        public event EventHandler WorkerStarted;

        /// <summary>
        ///   Occurs when [worker finished].
        /// </summary>
        public event EventHandler WorkerFinished;

        #endregion events

        #region eventTrigger

        /// <summary>
        ///   Called when an error occured.
        /// </summary>
        /// <param name="message"> The message. </param>
        private void OnError(string message)
        {
            if (Error != null) Error(this, new ErrorEventArgs(message));
        }

        /// <summary>
        ///   Called when the process finished.
        /// </summary>
        private void OnFinished()
        {
            if (Finished1 != null) Finished1(this, null);
        }

        /// <summary>
        ///   Called when a property changed.
        /// </summary>
        /// <param name="property"> The property. </param>
        private void OnPropertyChanged(string property)
        {
            if (PropertyChanged != null) PropertyChanged(this, new PropertyChangedEventArgs(property));
        }

        /// <summary>
        ///   Called when [worker started].
        /// </summary>
        /// <param name="worker"> The worker. </param>
        private void OnWorkerStarted(WorkerState worker)
        {
            if (WorkerStarted != null) WorkerStarted(worker, null);
        }

        /// <summary>
        ///   Called when [worker finished].
        /// </summary>
        /// <param name="worker"> The worker. </param>
        private void OnWorkerFinished(WorkerState worker)
        {
            if (WorkerFinished != null) WorkerFinished(worker, null);
        }

        #endregion eventTrigger

        #region members

        /// <summary>
        ///   Indicates, if this process has been cancelled.
        /// </summary>
        private bool _cancelled;

        /// <summary>
        ///   List of all unproceeded indizes.
        /// </summary>
        private Dictionary<string, List<Index>> _indizes;

        /// <summary>
        ///   See property IsRunning.
        /// </summary>
        private bool _isRunning;

        /// <summary>
        ///   List of all currently running indizes. (threadindex, tablename)
        /// </summary>
        private Dictionary<int, KeyValuePair<string, List<Index>>> _runningIndizes;

        /// <summary>
        ///   See property State.
        /// </summary>
        private ViewBuilderState _state;

        /// <summary>
        ///   List of all unproceeded views.
        /// </summary>
        private List<Viewscript> _views;

        /// <summary>
        ///   To save the transaction number
        /// </summary>
        private int tran_number;

        #endregion members

        #region properties

        /// <summary>
        ///   Gets the suffix of the temporary view database name.
        /// </summary>
        /// <value> The TMP view db name base. </value>
        private static string _tmpViewDbNameBase;

        /// <summary>
        ///   Gets a value indicating whether viewbox database loaded unsuccessfully.
        /// </summary>
        /// <value> <c>true</c> if viewbox database loaded unsuccessfully; otherwise, <c>false</c> . </value>
        private bool _isFailedToLoad;

        private bool _ready;
        private WorkerState[] _workerStates;

        public static string TmpViewDbNameBase
        {
            get
            {
                if (_tmpViewDbNameBase == null)
                {
                    string machineName = Environment.MachineName ?? "nomachineName";
                    machineName = Regex.Replace(machineName, @"[^a-zA-Z0-9_$]", "_");
                    _tmpViewDbNameBase = "_tmp_" + machineName + "_";
                }
                return _tmpViewDbNameBase;
            }
        }

        /// <summary>
        ///   Gets or sets the state.
        /// </summary>
        /// <value> The state. </value>
        public ViewBuilderState State
        {
            get { return _state; }
            set
            {
                if (_state != value)
                {
                    _state = value;
                    OnPropertyChanged("State");
                }
            }
        }

        /// <summary>
        ///   Gets or sets the profile.
        /// </summary>
        /// <value> The profile. </value>
        public ProfileConfig Profile { get; set; }

        /// <summary>
        ///   Gets or sets a value indicating whether this instance is running.
        /// </summary>
        /// <value> <c>true</c> if this instance is running; otherwise, <c>false</c> . </value>
        public bool IsRunning
        {
            get { return _isRunning; }
            private set
            {
                if (_isRunning != value)
                {
                    _isRunning = value;
                    OnPropertyChanged("IsRunning");
                    OnPropertyChanged("IsIdle");
                    OnPropertyChanged("IsReady");
                    OnPropertyChanged("IsCancelled");
                    OnPropertyChanged("NotCancelled");
                }
            }
        }

        /// <summary>
        ///   Gets a value indicating whether this instance is idle.
        /// </summary>
        /// <value> <c>true</c> if this instance is idle; otherwise, <c>false</c> . </value>
        public bool IsIdle
        {
            get { return !IsRunning && !IsFailedToLoad; }
        }

        public bool IsFailedToLoad
        {
            get { return _isFailedToLoad; }
            set
            {
                _isFailedToLoad = value;
                OnPropertyChanged("IsFailedToLoad");
                OnPropertyChanged("IsIdle");
            }
        }

        public bool IsReady
        {
            get { return IsIdle && _ready; }
            set
            {
                _ready = value;
                OnPropertyChanged("IsReady");
            }
        }

        /// <summary>
        ///   Gets a value indicating whether this instance is cancelled.
        /// </summary>
        /// <value> <c>true</c> if this instance is cancelled; otherwise, <c>false</c> . </value>
        public bool IsCancelled
        {
            get { return !IsRunning || _cancelled; }
        }

        /// <summary>
        ///   Gets a value indicating whether [not cancelled].
        /// </summary>
        /// <value> <c>true</c> if [not cancelled]; otherwise, <c>false</c> . </value>
        public bool NotCancelled
        {
            get { return IsRunning && !_cancelled; }
        }

        #endregion properties

        #region methods

        /// <summary>
        ///   Cancels all viewbuilder processes.
        /// </summary>
        public void Cancel()
        {
            _cancelled = true;
            //Profile.ConnectionManager.CancelAllJobs();
            if (_workerStates != null)
            {
                foreach (var workerState in _workerStates)
                    if (workerState != null && workerState.Connection != null)
                    {
                        workerState.Connection.CancelCommand();
                        workerState.Connection.Close();
                        workerState.Connection.Dispose();
                    }
            }
            try
            {
                IndexDb.IndexDb.GetInstance().IndexDbConnection.CancelAllJobs();
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }
            OnPropertyChanged("IsCancelled");
            OnPropertyChanged("NotCancelled");
        }

        private void DropIndizes(string database, List<string> tables)
        {
            using (var db = Profile.ViewboxDb.ConnectionManager.GetConnection())
            {
                foreach (var table in tables)
                    foreach (var index in db.GetIndexNames(database, table).Distinct())
                        if (index.ToLower() != "primary")
                            db.ExecuteNonQuery(String.Format("DROP INDEX {0} ON {1}", index, db.Enquote(database, table)));
            }
        }

        private Index ModifyIndex(Index index)
        {
            ITableObject tobj = Profile.ViewboxDb.Objects[Profile.DbConfig.DbName + "." + index.Table];
            if (tobj == null)
            {
                string msg = string.Format("Table not exists in metadata: [{0}]",
                                           Profile.DbConfig.DbName + "." + index.Table);
                _log.Error(msg);
                throw new ArgumentException(msg);
            }
            if (!tobj.Columns.All(c => index.Columns.Contains(c.Name.ToLower()) && !c.IsEmpty))
            {
                try
                {
                    var newColumns =
                        (from column in index.Columns where !tobj.Columns[column].IsEmpty select column).ToList();
                    index = new Index(index.Table, newColumns);
                }
                catch (KeyNotFoundException ex)
                {
                    throw new KeyNotFoundException(
                        string.Format("Column not found for creating index [{0}]", index.Name), ex);
                }
            }
            return index;
        }

        private void CreateDistinctValuesForParameters(IIssue issue, int threadIndex)
        {
            GetWorkerState(threadIndex).PreparationProgress = 0;
            if (!(Profile.IndexDb.IsInitialized.HasValue && Profile.IndexDb.IsInitialized.Value))
                throw new Exception("IndexDb not initialized!");
            int i = 1;
            foreach (IParameter parameter in issue.Parameters)
            {
                if (_cancelled) return;
                IndexDb.IndexDb.DoJob(Profile.ViewboxDb, new ProgressCalculator(), 4, 0, parameter.Id);
                GetWorkerState(threadIndex).PreparationProgress = i/(double) issue.Parameters.Count;
                i++;
            }
        }

        /// <summary>
        ///   DEVNOTE: double and float type of columns can be created without precision in MySql, int this case the limit is only the hardware
        /// </summary>
        /// <param name="threadIndex"> </param>
        /// <param name="tmpViewDbName"> </param>
        /// <param name="viewscript"> </param>
        private void CheckForDoubleColumns(int threadIndex, string tmpViewDbName, Viewscript viewscript)
        {
            if (Profile.ConnectionManager.DbConfig.DbType != "MySQL")
                return;
            using (IDatabase conn = Profile.ConnectionManager.GetConnection())
            {
                if (!conn.TableExists(viewscript.Name))
                    return;
                string warning = null;
                List<DbColumnInfo> columnInfos = conn.GetColumnInfos(viewscript.Name);
                if (columnInfos == null)
                {
                    viewscript.LastError += " Error on CheckForDoubleColumns (check log for details)";
                    return;
                }
                else
                {
                    foreach (var columnInfo in columnInfos)
                    {
                        if (_cancelled) break;
                        if (columnInfo.OriginalType.ToLower().Contains("double"))
                        {
                            if (columnInfo.MaxLength == 0 || columnInfo.NumericScale == 0)
                            {
                                warning = string.Empty;
                                if (columnInfo.MaxLength == 0)
                                    warning = string.Format("Column {0} type of {1} has no MaxLength!", columnInfo.Name,
                                                            "double");
                                if (columnInfo.NumericScale == 0)
                                {
                                    if (string.IsNullOrEmpty(warning)) warning += Environment.NewLine;
                                    warning += string.Format("Column {0} type of {1} has no NumericScale!",
                                                             columnInfo.Name, "double");
                                }
                                viewscript.AddWarning(warning);
                            }
                            else
                                viewscript.AddWarning(
                                    string.Format(
                                        "Die Spalte {0} ist vom Typ double - dies sollte vermieden werden und stattdessen decimal benutzt werden.",
                                        columnInfo.Name));
                        }
                        if (columnInfo.OriginalType.ToLower().Contains("float"))
                        {
                            if (columnInfo.MaxLength == 0 || columnInfo.NumericScale == 0)
                            {
                                warning = string.Empty;
                                if (columnInfo.MaxLength == 0)
                                    warning = string.Format("Column {0} type of {1} has no MaxLength!", columnInfo.Name,
                                                            "float");
                                if (columnInfo.NumericScale == 0)
                                {
                                    if (string.IsNullOrEmpty(warning)) warning += Environment.NewLine;
                                    warning += string.Format("Column {0} type of {1} has no NumericScale!",
                                                             columnInfo.Name, "float");
                                }
                                viewscript.AddWarning(warning);
                            }
                            else
                                viewscript.AddWarning(
                                    string.Format(
                                        "Die Spalte {0} ist vom Typ float - dies sollte vermieden werden und stattdessen decimal benutzt werden.",
                                        columnInfo.Name));
                        }
                    }
                }
            }
        }

        private void CheckParameterReferencedColumn(string databaseName, ReportParameter parameter,
                                                    Viewscript viewscript)
        {
            CheckParameterReferencedColumn(databaseName, parameter.TableName, parameter.ColumnName,
                                           parameter.ParameterName, viewscript);
        }

        private void CheckParameterReferencedColumn(string databaseName, ProcedureParameter parameter,
                                                    Viewscript viewscript)
        {
            CheckParameterReferencedColumn(databaseName, parameter.TableName, parameter.ColumnName, parameter.Name,
                                           viewscript);
        }

        private void CheckParameterReferencedColumn(string databaseName, string tableName, string columnName,
                                                    string paramName, Viewscript viewscript)
        {
            if (!_cancelled)
            {
                string sql = null;
                if (Profile.ConnectionManager.DbConfig.DbType != "MySQL")
                    sql = "SELECT TOP 1 {2} FROM {0}.{1};";
                else
                    sql = "SELECT {2} FROM {0}.{1} LIMIT 1;";
                sql = string.Format(sql, databaseName, tableName, columnName);
                try
                {
                    using (IDatabase conn = Profile.ConnectionManager.GetConnection())
                    {
                        // if fails then throw error otherwise successful
                        using (IDataReader rdr = conn.ExecuteReader(sql))
                        {
                        }
                    }
                }
                catch (Exception ex)
                {
                    viewscript.AddWarning(string.Format(Resource.ReferencedColumnNotFound, paramName,
                                                        databaseName, tableName, columnName));
                    throw new InvalidScriptException(string.Format(Resource.WrongReportFilterCriteria,
                                                                   ex.Message));
                }
            }
        }

        private void CheckWhereCondition(int threadIndex, string tmpViewDbName, Viewscript viewscript)
        {
            string sql = null;
            if (Profile.ConnectionManager.DbConfig.DbType != "MySQL")
                sql = "SELECT TOP 1 * FROM {0}.{1} WHERE {2};";
            else
                sql = "SELECT * FROM {0}.{1} WHERE {2} LIMIT 1;";
            string whereClause = string.Empty;
            foreach (string reportFilterLine in viewscript.ViewInfo.ReportFilterLines)
            {
                if (!string.IsNullOrEmpty(whereClause)) whereClause += Environment.NewLine;
                whereClause += reportFilterLine;
            }
            whereClause = whereClause.ToLower();
            foreach (ReportParameter reportParameter in viewscript.ViewInfo.ReportParameterDictionary.Values)
            {
                whereClause = whereClause.Replace(reportParameter.ParameterName.ToLower(), "NULL");
            }
            sql = string.Format(sql, tmpViewDbName, viewscript.ViewInfo.Name, whereClause);
            try
            {
                using (IDatabase conn = Profile.ConnectionManager.GetConnection())
                {
                    // if fails then throw error otherwise successful
                    using (IDataReader rdr = conn.ExecuteReader(sql))
                    {
                    }
                }
            }
            catch (Exception ex)
            {
                throw new InvalidScriptException(string.Format(Resource.WrongViewFilterCriteria, ex.Message));
            }
        }

        private void CheckForParameterColumnsExists(int threadIndex, string tmpViewDbName, Viewscript viewscript)
        {
            string error = string.Empty;
            bool notInTable = false;
            using (IDatabase conn = Profile.ConnectionManager.GetConnection())
            {
                //DEVNOTE: check first in viewscript.ViewInfo.ColumnDictionary whether it is a column in the viewtable itself
                List<DbColumnInfo> columnInfos = new List<DbColumnInfo>();
                if (conn.TableExists(viewscript.Name))
                {
                    columnInfos = conn.GetColumnInfos(viewscript.Name);
                }
                foreach (ReportParameter param in viewscript.ViewInfo.ReportParameterDictionary.Values)
                {
                    if (!_cancelled)
                    {
                        if (
                            !viewscript.ViewInfo.ColumnDictionary.Any(
                                ci =>
                                string.Compare(ci.Key, param.ColumnName, StringComparison.InvariantCultureIgnoreCase) ==
                                0))
                        {
                            if (
                                !columnInfos.Any(
                                    ci =>
                                    string.Compare(ci.Name, param.ColumnName,
                                                   StringComparison.InvariantCultureIgnoreCase) == 0))
                            {
                                if (!string.IsNullOrEmpty(error)) error += Environment.NewLine;
                                error += string.Format(Resource.ParameterColumnDoesNotExists, param.ColumnName);
                            }
                        }
                    }
                }
                //if (!conn.TableExists(viewscript.Name)) {
                //    return;
                //}
                //foreach (ReportParameter param in viewscript.ViewInfo.ReportParameterDictionary.Values) {
                //    if (!_cancelled) {
                //        if (!columnInfos.Any(ci => string.Compare(ci.Name, param.ColumnName, StringComparison.InvariantCultureIgnoreCase) == 0)) {
                //            if (!string.IsNullOrEmpty(error)) error += Environment.NewLine;
                //            error += string.Format(Resources.Resource.ParameterColumnDoesNotExists, param.ColumnName);
                //        }
                //    }
                //}
            }
            if (!string.IsNullOrEmpty(error))
                throw new InvalidScriptException(error);
        }

        private void viewscript_ViewIsRunningLong(object sender, System.EventArgs e)
        {
            Viewscript viewscript = (Viewscript) sender;
            viewscript.SendEmail(Profile.DbConfig);
        }

        private WorkerState GetWorkerState(int threadIndex)
        {
            if (_workerStates != null && _workerStates.Any(ws => ws.ThreadIndex == threadIndex))
                return _workerStates[threadIndex];
            return new WorkerState(threadIndex);
        }

        #region AddProcedureToSysDb

        private void AddProcedureToSysDb(IDatabase conn, Viewscript viewscript)
        {
            string dbName = conn.DbConfig.DbName + "_system";
            string createProceduresTableSql =
                string.Format(
                    @"CREATE TABLE IF NOT EXISTS {0} (
				id              INT UNSIGNED auto_increment PRIMARY KEY,
				table_id        INT UNSIGNED NOT NULL,
				name            varchar(45),
				description     varchar(255) NOT NULL,
				DoClientSplit   tinyint(1) NOT NULL,
				DoCompCodeSplit tinyint(1) NOT NULL,
				DoFYearSplit    tinyint(1) NOT NULL)",
                    conn.Enquote(dbName, "procedures"));
            string createProcedureParamsSql =
                string.Format(
                    @"CREATE TABLE IF NOT EXISTS {0} (
			id           INT UNSIGNED auto_increment PRIMARY KEY,
			procedure_id INT UNSIGNED NOT NULL,
			ordinal      INT UNSIGNED NOT NULL,
			name         varchar(45) NOT NULL,
			description  varchar(255) NOT NULL,
			`type`       varchar(45) NOT NULL)",
                    conn.Enquote(dbName, "procedure_params"));
            conn.ExecuteNonQuery(createProceduresTableSql);
            conn.ExecuteNonQuery(createProcedureParamsSql);
            object procedureIdObj =
                conn.ExecuteScalar("SELECT id FROM " + conn.Enquote(dbName, "procedures") + " WHERE LOWER(name)=" +
                                   conn.GetSqlString(viewscript.ViewInfo.ProcedureName.ToLower()));
            object tableIdObj =
                conn.ExecuteScalar("SELECT table_id FROM " + conn.Enquote(dbName, "table") + " WHERE LOWER(name)=" +
                                   conn.GetSqlString(viewscript.ViewInfo.Name.ToLower()));
            int tableId;
            if (tableIdObj == null || !Int32.TryParse(tableIdObj.ToString(), out tableId))
                //conn.ExecuteNonQuery(string.Format("INSERT INTO {0}", conn.Enquote(dbName, "table")));
                throw new Exception(
                    string.Format(
                        "Für die Prozedur {0} konnte die Basistabelle {1} nicht in der Tabelle {2} gefunden werden.",
                        viewscript.ViewInfo.ProcedureName, viewscript.ViewInfo.Name, conn.Enquote(dbName, "table")));
            int procedureId;
            if (procedureIdObj != null && Int32.TryParse(procedureIdObj.ToString(), out procedureId))
            {
                conn.ExecuteNonQuery("DELETE FROM " + conn.Enquote(dbName, "procedure_params") + " WHERE procedure_id=" +
                                     procedureId);
                conn.ExecuteNonQuery("DELETE FROM " + conn.Enquote(dbName, "procedures") + " WHERE id=" + procedureId);
            }
            conn.ExecuteNonQuery("INSERT INTO " + conn.Enquote(dbName, "procedures") +
                                 " (table_id,`name`,description,DoClientSplit, DoCompCodeSplit, DoFYearSplit) VALUES "
                                 + string.Format("({0},{1},{2},{3},{4},{5})", tableId,
                                                 conn.GetSqlString(viewscript.ViewInfo.ProcedureName),
                                                 conn.GetSqlString(viewscript.ViewInfo.Description),
                                                 viewscript.ViewInfo.OptimizeCriterias.DoClientSplit ? "1" : "0",
                                                 viewscript.ViewInfo.OptimizeCriterias.DoCompCodeSplit ? "1" : "0",
                                                 viewscript.ViewInfo.OptimizeCriterias.DoFYearSplit ? "1" : "0"));
            //As we inserted the procedure this is garanteed to work
            procedureIdObj =
                conn.ExecuteScalar("SELECT id FROM " + conn.Enquote(dbName, "procedures") + " WHERE LOWER(name)=" +
                                   conn.GetSqlString(viewscript.ViewInfo.ProcedureName.ToLower()));
            if (procedureIdObj == null || !Int32.TryParse(procedureIdObj.ToString(), out procedureId))
                throw new Exception(
                    string.Format("Die Prozedur {0} konnte nicht in die Systemdatenbank {1} eingetragen werden",
                                  viewscript.ViewInfo.ProcedureName, conn.Enquote(dbName, "table")));
            int ordinal = 0;
            foreach (var paramPair in viewscript.ViewInfo.ParameterDictionary)
            {
                conn.ExecuteNonQuery(
                    string.Format(
                        "INSERT INTO {0} (procedure_id, ordinal, name, description, type) VALUES ({1}, {2},{3},{4},{5})",
                        conn.Enquote(dbName, "procedure_params"), procedureId, ordinal++,
                        conn.GetSqlString(paramPair.Key),
                        conn.GetSqlString(paramPair.Value.GetDescription()),
                        conn.GetSqlString(paramPair.Value.GetSystemDbType())));
            }
        }

        #endregion AddProcedureToSysDb

        #region DeleteTableEntriesIfExist

        private void DeleteTableEntriesIfExist(IDatabase viewboxConn, string viewscriptName)
        {
            tran_number = -1;
            DbDataReader reader = null;
            try
            {
                string name = viewscriptName.ToLower();
                try
                {
                    viewboxConn.BeginTransaction();
                    ////Delete from system db
                    //if (viewboxConn.TableExists("order_area"))
                    //    viewboxConn.ExecuteNonQuery("DELETE FROM " + viewboxConn.Enquote("order_area") + " WHERE LOWER(" +
                    //                                viewboxConn.Enquote("table_name") + ") = '" + name + "'");
                    if (viewboxConn.TableExists("table_parts"))
                    {
                        viewboxConn.ExecuteNonQuery("DELETE FROM " + viewboxConn.Enquote("table_parts") +
                                                    " WHERE LOWER(" +
                                                    viewboxConn.Enquote("table_name") + ") = '" + name + "'");
                    }
                    if (viewboxConn.TableExists("table"))
                    {
                        viewboxConn.ExecuteNonQuery("DELETE FROM " + viewboxConn.Enquote("table") + " WHERE LOWER(" +
                                                    viewboxConn.Enquote("name") + ") = '" + name + "'");

                        reader = (DbDataReader) viewboxConn.ExecuteReader(
                            "SELECT " +
                            "table_id, " + // 0
                            "name " + // 1
                            "FROM " +
                            viewboxConn.Enquote("table") +
                            " WHERE LOWER(name) = '" + name + "'");

                        if (reader.HasRows)
                        {
                            reader.Read();

                            var tableId = reader[0];
                            //var name = reader[1];
                            reader.Close();

                            //DELETE IT
                            // delete entries from system database
                            viewboxConn.ExecuteNonQuery("DELETE FROM " + viewboxConn.Enquote("col") + " WHERE " +
                                                        viewboxConn.Enquote("table_id") + " = " +
                                                        (Int32) (UInt32) tableId);
                        }

                        reader.Close();
                    }
                    viewboxConn.CommitTransaction();
                }
                catch (Exception ex)
                {
                    if (viewboxConn.HasTransaction())
                        viewboxConn.RollbackTransaction();
                    _log.Error(ex.Message, ex);
                    throw;
                }

                //Delete from viewbox database
                using (var db = Profile.ViewboxDb.ConnectionManager.GetConnection())
                {
                    //var database = viewboxConn.DbConfig.DbName.Substring(0, viewboxConn.DbConfig.DbName.Length - 7);
                    var database = Profile.ConnectionManager.DbConfig.DbName;

                    reader = (DbDataReader) db.ExecuteReader(
                        "SELECT " +
                        "id, " + // 0
                        "transaction_nr " + // 1
                        "FROM " +
                        db.Enquote("tables") +
                        " WHERE LOWER(name) = '" + name + "' AND " + db.Enquote("database") + "= '" + database + "'");

                    if (reader.HasRows)
                    {
                        reader.Read();

                        var tableId = reader.GetInt32(0);
                        tran_number = reader.GetInt32(1);

                        reader.Close();
                        //DELETE IT
                        // delete entries from viewbox database
                        try
                        {
                            db.BeginTransaction();
                            DeleteFromViewboxDb(db, tableId, false, Profile);
                            db.CommitTransaction();
                        }
                        catch (Exception ex)
                        {
                            if (db.HasTransaction())
                                db.RollbackTransaction();
                            _log.Error(ex.Message, ex);
                            throw;
                        }
                        Profile.ViewboxDb.LoadTables(db);
                    }
                }
            }
            finally
            {
                if (reader != null)
                {
                    if (!reader.IsClosed)
                        reader.Close();
                }
            }
        }

        #endregion DeleteTableEntriesIfExist

        #region DeleteFromViewboxDb

        private static void DeleteFromViewboxDb(IDatabase conn, int tableId, bool onlyTexts, ProfileConfig profile)
        {
            DbDataReader reader = null;
            try
            {
                if (!onlyTexts)
                {
                    conn.ExecuteNonQuery("DELETE FROM " + conn.Enquote("tables") + " WHERE " + conn.Enquote("id") +
                                         " = " +
                                         tableId);
                    conn.ExecuteNonQuery("DELETE FROM " + conn.Enquote("order_areas") + " WHERE " +
                                         conn.Enquote("table_id") + " = " + tableId);
                    conn.ExecuteNonQuery("DELETE FROM " + conn.Enquote("issue_extensions") + " WHERE " +
                                         conn.Enquote("ref_id") + " = " + tableId);

                    //user specific changes
                    conn.ExecuteNonQuery("DELETE FROM " + conn.Enquote("user_table_settings") + " WHERE " +
                                         conn.Enquote("table_id") + " = " + tableId);
                    conn.ExecuteNonQuery("DELETE FROM " + conn.Enquote("user_column_order_settings") + " WHERE " +
                                         conn.Enquote("table_id") + " = " + tableId);
                    conn.ExecuteNonQuery(
                        string.Format(
                            "DELETE FROM {0} WHERE {1} LIKE '{2},%' OR {1} LIKE '%,{2}' OR {1} LIKE '%,{2},%'",
                            conn.Enquote("user_table_object_order_settings"),
                            conn.Enquote("table_object_order"), tableId));
                }
                conn.ExecuteNonQuery("DELETE FROM " + conn.Enquote("table_texts") + " WHERE " + conn.Enquote("ref_id") +
                                     " = " + tableId);

                if (!onlyTexts)
                {
                    // DELETE FROM extended_column_information
                    string selectColumns = "SELECT id FROM " + conn.Enquote("columns") + " WHERE " +
                                           conn.Enquote("table_id") + " = " + tableId;
                    conn.ExecuteNonQuery("DELETE FROM " + conn.Enquote("extended_column_information") + " WHERE " +
                                         conn.Enquote("parent") + " IN ( " + selectColumns + " ) OR " +
                                         conn.Enquote("child") + " IN ( " + selectColumns + " ) OR " +
                                         conn.Enquote("information") + " IN ( " + selectColumns + " )");
                }

                reader = (DbDataReader) conn.ExecuteReader(
                    "SELECT " +
                    "id " + // 0
                    "FROM " +
                    conn.Enquote("columns") +
                    " WHERE table_id = " + tableId);

                if (reader.HasRows)
                {
                    var columnIds = new List<string>();
                    while (reader.Read())
                    {
                        columnIds.Add(reader[0].ToString());
                    }
                    reader.Close();

                    foreach (var id in columnIds)
                    {
                        if (!onlyTexts)
                        {
                            conn.ExecuteNonQuery("DELETE FROM " + conn.Enquote("columns") + " WHERE " +
                                                 conn.Enquote("id") + " = " + id);

                            conn.ExecuteNonQuery("DELETE FROM " + conn.Enquote("user_column_settings") + " WHERE " +
                                                 conn.Enquote("column_id") + " = " + id);

                            conn.ExecuteNonQuery("DELETE FROM " + conn.Enquote("relations") + " WHERE " +
                                                 conn.Enquote("parent") + " = " + id + " OR " + conn.Enquote("child") +
                                                 " = " + id);
                        }
                        conn.ExecuteNonQuery("DELETE FROM " + conn.Enquote("column_texts") + " WHERE " +
                                             conn.Enquote("ref_id") + " = " + id);

                        if (!onlyTexts)
                        {
                            try
                            {
                                //DEVNOTE: delete distinct values from IndexDb as well
                                profile.IndexDb.DeleteColumnData(Convert.ToInt32(id));
                            }
                            catch (Exception ex)
                            {
                                //DEVNOTE: we do not handle this exception here because the entire process is not fail safe
                                _log.Warn(
                                    "Operation on index db resulted in error, this is ignored intentionally because the entire process is not fail safe",
                                    ex);
                            }
                        }
                    }
                }
                reader.Close();

                reader = (DbDataReader) conn.ExecuteReader(
                    "SELECT " +
                    "id " + // 0
                    "FROM " +
                    conn.Enquote("parameter") +
                    " WHERE issue_id = " + tableId);

                if (reader.HasRows)
                {
                    var parameterIds = new List<string>();
                    while (reader.Read())
                    {
                        parameterIds.Add(reader[0].ToString());
                    }
                    reader.Close();

                    foreach (var id in parameterIds)
                    {
                        if (!onlyTexts)
                        {
                            conn.ExecuteNonQuery(string.Format("DELETE FROM {0} WHERE {1} = {2}",
                                                               conn.Enquote("user_issue_storedprocedure_order_settings"),
                                                               conn.Enquote("parameter_id"), id));
                        }

                        string collectionIdSql =
                            string.Format(
                                "SELECT `collection_id` FROM `parameter_collections` WHERE parameter_id = {0} limit 1",
                                id);
                        int? collectionId = (int?) conn.ExecuteScalar(collectionIdSql);

                        conn.ExecuteNonQuery("DELETE FROM " + conn.Enquote("parameter_texts") + " WHERE " +
                                             conn.Enquote("ref_id") + " = " + id);

                        if (!onlyTexts)
                        {
                            conn.ExecuteNonQuery("DELETE FROM " + conn.Enquote("parameter") + " WHERE " +
                                                 conn.Enquote("id") + " = " + id);
                            conn.ExecuteNonQuery("DELETE FROM " + conn.Enquote("parameter_collections") + " WHERE " +
                                                 conn.Enquote("parameter_id") + " = " + id);
                        }

                        if (collectionId.HasValue)
                        {
                            string countSql =
                                string.Format(
                                    "SELECT COUNT(*) FROM `parameter_collections` WHERE `collection_id` = {0}",
                                    collectionId);
                            Int64 count = (Int64) conn.ExecuteScalar(countSql);
                            if (count == 0)
                            {
                                conn.ExecuteNonQuery("DELETE FROM `collection_texts` WHERE `ref_id` = " + collectionId);
                                if (!onlyTexts)
                                {
                                    conn.ExecuteNonQuery("DELETE FROM `collections` WHERE `collection_id` = " +
                                                         collectionId);
                                }
                            }
                        }
                    }
                }
                reader.Close();

                if (!onlyTexts)
                {
                    //There may be filtered issues which need to be deleted recursively
                    reader = (DbDataReader) conn.ExecuteReader(string.Format("SELECT ref_id FROM {0} WHERE {1}={2}",
                                                                             conn.Enquote("issue_extensions"),
                                                                             conn.Enquote("obj_id"),
                                                                             tableId));
                    List<int> tablesToDelete = new List<int>();
                    while (reader.Read())
                        tablesToDelete.Add(reader.GetInt32(0));
                    reader.Close();
                    foreach (var tableToDelete in tablesToDelete)
                    {
                        //Prevents infinite loop
                        conn.ExecuteNonQuery("DELETE FROM " + conn.Enquote("issue_extensions") + " WHERE " +
                                             conn.Enquote("obj_id") + " = " + tableId);
                        DeleteFromViewboxDb(conn, tableToDelete, onlyTexts, profile);
                    }
                    //Delete all user config
                    conn.ExecuteNonQuery(string.Format("DELETE FROM {0}",
                                                       conn.Enquote("user_table_object_order_settings")));
                }
            }
            finally
            {
                if (reader != null)
                    reader.Close();
            }
        }

        #endregion DeleteFromViewboxDb

        #region CopyView

        /// <summary>
        ///   Copies the view from temp database to original database
        /// </summary>
        /// <param name="conn"> The conn. </param>
        /// <param name="tmpViewDbName"> Name of the TMP view db. </param>
        /// <param name="viewscript"> The view info. </param>
        private void CopyView(ViewCreateOptions viewOptions, int threadIndex, string tmpViewDbName,
                              Viewscript viewscript)
        {
            using (var conn = Profile.ConnectionManager.GetConnection())
            {
                conn.SetHighTimeout();
                if (!viewOptions.GenerateDistinctDataOnly && !viewscript.ProcedureInfo.OnlyMetadata)
                {
                    conn.SetHighTimeout();
                    GetWorkerState(threadIndex).Task = WorkerTask.DropExistingMetadata;
                    GetWorkerState(threadIndex).PreparationProgress = 0;
                    GetWorkerState(threadIndex).Connection = conn;
                    OptimizeCriterias optimizeCriterias = viewscript.ViewInfo.OptimizeCriterias;
                    string sourceTableName = conn.Enquote(tmpViewDbName, viewscript.Name);
                    string destTableName = conn.Enquote(viewscript.Name);
                    // drop view table if it exists (this could happen, e.g. if a view has been manually deleted from the system database)
                    conn.DropTableIfExists(viewscript.Name);
                    GetWorkerState(threadIndex).PreparationProgress = 1/(double) 6;
                    if (_cancelled) return;
                    conn.ExecuteNonQuery(
                        "CREATE TABLE " + destTableName + " LIKE " + sourceTableName);
                    GetWorkerState(threadIndex).PreparationProgress = 2/(double) 6;
                    if (_cancelled) return;
                    conn.ExecuteNonQuery(
                        "ALTER TABLE " + destTableName + " Engine = MyISAM");
                    GetWorkerState(threadIndex).PreparationProgress = 3/(double) 6;
                    if (_cancelled) return;
                    conn.ExecuteNonQuery(
                        "ALTER TABLE " + destTableName + " ADD `_row_no_` INT UNSIGNED NOT NULL");
                    GetWorkerState(threadIndex).PreparationProgress = 4/(double) 6;
                    if (_cancelled) return;
                    conn.ExecuteNonQuery(
                        "ALTER TABLE " + destTableName + " ADD PRIMARY KEY (`_row_no_`)");
                    GetWorkerState(threadIndex).PreparationProgress = 5/(double) 6;
                    if (_cancelled) return;
                    conn.ExecuteNonQuery(
                        "ALTER TABLE " + destTableName +
                        " CHANGE `_row_no_` `_row_no_` INT UNSIGNED NOT NULL AUTO_INCREMENT");
                    GetWorkerState(threadIndex).PreparationProgress = 6/(double) 6;
                    string fields = string.Empty;
                    GetWorkerState(threadIndex).Task = WorkerTask.GetColumnInfo;
                    GetWorkerState(threadIndex).PreparationProgress = 0;
                    // get view columns
                    if (!viewscript.HasStoredProcedure)
                    {
                        IDataReader reader = null;
                        try
                        {
                            reader = conn.ExecuteReader(
                                "SELECT * FROM " + sourceTableName, CommandBehavior.SchemaOnly);

                            for (int i = 0; i < reader.FieldCount; i++)
                            {
                                string colName = conn.ReplaceSpecialChars(reader.GetName(i));
                                // add column to selection string
                                if (fields.Length > 0)
                                    fields += ",";
                                fields += conn.Enquote(colName);
                            }
                        }
                        finally
                        {
                            if ((reader != null) && (!reader.IsClosed))
                                reader.Close();
                        }

                        if (_cancelled) return;

                        string sql =
                            "INSERT INTO " + destTableName + " (" + fields + ") " +
                            "SELECT " + fields + " " +
                            "FROM " + sourceTableName;

                        var resort = false;

                        var orderCols = String.Empty;
                        //MANDT
                        if (optimizeCriterias.DoClientSplit)
                        {
                            orderCols += conn.Enquote(optimizeCriterias.ClientField);
                            resort = true;
                        }
                        //BUKRS
                        if (optimizeCriterias.DoCompCodeSplit)
                        {
                            if (orderCols.Length > 0)
                                orderCols += ", ";
                            orderCols += conn.Enquote(optimizeCriterias.CompCodeField);
                            resort = true;
                        }
                        //GJAHR
                        if (optimizeCriterias.DoFYearSplit)
                        {
                            if (orderCols.Length > 0)
                                orderCols += ", ";
                            orderCols += conn.Enquote(optimizeCriterias.GJahrField);
                            resort = true;
                        }

                        if (resort)
                        {
                            sql +=
                                " ORDER BY " + orderCols;
                        }

                        if (_cancelled) return;

                        // create view table
                        conn.ExecuteNonQuery(sql);
                        GetWorkerState(threadIndex).Connection = null;
                    }
                    GetWorkerState(threadIndex).PreparationProgress = 1;
                }
                DeleteAndAddEntries(viewOptions, threadIndex, viewscript, conn);
            }
        }

        #endregion CopyView

        #region DeleteAndAddEntries

        private void DeleteAndAddEntries(ViewCreateOptions viewOptions, int threadIndex, Viewscript viewscript,
                                         IDatabase conn)
        {
            lock (Profile.ViewboxDb)
            {
                if (!viewOptions.GenerateDistinctDataOnly && !viewscript.ProcedureInfo.OnlyMetadata)
                {
                    GetWorkerState(threadIndex).Task = WorkerTask.DeletingTableEntries;
                    GetWorkerState(threadIndex).PreparationProgress = 0;
                    if (_cancelled) return;
                    using (var viewboxConn = Profile.SysDb.ConnectionManager.GetConnection())
                    {
                        DeleteTableEntriesIfExist(viewboxConn, viewscript.Name);
                    }
                    GetWorkerState(threadIndex).PreparationProgress = 1;
                    conn.SetHighTimeout();
                }
                //AddToSysDb(conn, viewscript);
                if (_cancelled) return;
                try
                {
                    CreateViewboxDbEntry(viewOptions, threadIndex, conn, viewscript);
                }
                catch (Exception)
                {
                    // DEVNOTE: if only the distinct value generation failed, then it can be rerun seperately so not necessary to fail the whole script
                    // give warning message to the user instead
                    if (viewscript.State == ViewscriptStates.GeneratingDistinctValuesError)
                    {
                        viewscript.AddWarning(string.Format(Resource.WarningRunGenerateIndexData,
                                                            Resource.GenerateIndexData));
                    }
                    else throw;
                }
            }
        }

        #endregion DeleteAndAddEntries

        #region AddToSysDb

        private void AddToSysDb(IDatabase conn, Viewscript viewscript)
        {
            IDataReader oReader = null;
            try
            {
                conn.BeginTransaction();
                conn.ExecuteNonQuery(
                    "INSERT IGNORE INTO " + conn.Enquote(conn.DbConfig.DbName + "_system", "system") + " " +
                    "(`SYSTEM_ID`, `COMPANY_ID`, `NAME`) VALUES (2, 1, 'Views')");
                conn.ExecuteNonQuery(
                    "insert into " + conn.Enquote(conn.DbConfig.DbName + "_system", "table_parts") + " VALUES ('" +
                    viewscript.Name + "', 0, 1, '" + viewscript.Name + "')");
                string sql = "INSERT INTO " + conn.Enquote(conn.DbConfig.DbName + "_system", "table") + " " +
                             "(`SYSTEM_ID`, `NAME`, `COMMENT`, `MANDT_SPLIT`, `MANDT_COL`,`GJAHR_SPLIT`,`GJAHR_COL`,`BUKRS_SPLIT`,`BUKRS_COL`) " +
                             "VALUES (2, '" + viewscript.Name + "', '" + viewscript.Description + "', '" +
                             (viewscript.ViewInfo.OptimizeCriterias.DoClientSplit ? 1 : 0) + "', '" +
                             viewscript.ViewInfo.OptimizeCriterias.ClientField + "', '" +
                             (viewscript.ViewInfo.OptimizeCriterias.DoFYearSplit ? 1 : 0) + "', '" +
                             viewscript.ViewInfo.OptimizeCriterias.GJahrField + "', '" +
                             (viewscript.ViewInfo.OptimizeCriterias.DoCompCodeSplit ? 1 : 0) + "', '" +
                             viewscript.ViewInfo.OptimizeCriterias.CompCodeField +
                             "')";
                conn.ExecuteNonQuery(sql);
                int nLastId =
                    Convert.ToInt32(conn.ExecuteScalar(
                        "SELECT last_insert_id() FROM " +
                        conn.Enquote(conn.DbConfig.DbName + "_system", "table")));
                var insertSqls = new List<string>();
                foreach (var c in conn.GetColumnInfos(conn.DbConfig.DbName, viewscript.Name))
                {
                    string sColumn =
                        c.Name
                            .Replace(Convert.ToString((char) 195) + Convert.ToString((char) 132), "Ä")
                            .Replace(Convert.ToString((char) 195) + Convert.ToString((char) 164), "ä")
                            .Replace(Convert.ToString((char) 195) + Convert.ToString((char) 150), "Ö")
                            .Replace(Convert.ToString((char) 195) + Convert.ToString((char) 182), "ö")
                            .Replace(Convert.ToString((char) 195) + Convert.ToString((char) 156), "Ü")
                            .Replace(Convert.ToString((char) 195) + Convert.ToString((char) 188), "ü")
                            .Replace(Convert.ToString((char) 195) + Convert.ToString((char) 159), "ß");
                    // Spalte '_row_no_' ignorieren
                    if (sColumn.Equals("_row_no_"))
                        continue;
                    string sDescription = string.Empty;
                    if (viewscript.ViewInfo.ColumnDictionary.ContainsKey(sColumn))
                    {
                        Dictionary<string, string> langToDescr = viewscript.ViewInfo.ColumnDictionary[sColumn];
                        if (langToDescr.Count > 0)
                            sDescription = langToDescr[viewscript.ViewInfo.Languages[0]];
                    }
                    string sType;
                    switch (c.Type)
                    {
                        case DbColumnTypes.DbText:
                        case DbColumnTypes.DbLongText:
                            sType = "string";
                            break;
                        case DbColumnTypes.DbDate:
                            sType = "date";
                            break;
                        case DbColumnTypes.DbInt:
                        case DbColumnTypes.DbBigInt:
                        case DbColumnTypes.DbNumeric:
                            sType = "numeric";
                            break;
                        case DbColumnTypes.DbBinary:
                            OnError("VARBINARY-Typ in '" + viewscript.Name + "." + sColumn + "' gefunden");
                            sType = "string";
                            break;
                        case DbColumnTypes.DbTime:
                            sType = "time";
                            break;
                        case DbColumnTypes.DbDateTime:
                            sType = "datetime";
                            break;
                        case DbColumnTypes.DbBool:
                            sType = "bool";
                            break;
                        default:
                            OnError("Nicht zugeordneter Datentyp in " + viewscript.Name + "." + sColumn + "': " +
                                    c.OriginalType);
                            sType = "string";
                            break;
                    }
                    string sSql =
                        "INSERT INTO " +
                        conn.Enquote(conn.DbConfig.DbName + "_system", "col") +
                        "(table_id, name, comment, `type`) " +
                        "VALUES (" + nLastId + ",'" + sColumn + "'," + conn.GetSqlString(sDescription) + ",'" +
                        sType + "')";
                    insertSqls.Add(sSql);
                }
                /*using (oReader = conn.ExecuteReader(
					"SELECT * FROM " + conn.Enquote(conn.DbConfig.DbName, viewscript.Name), System.Data.CommandBehavior.SchemaOnly)) {
					for (int n = 0; n < oReader.FieldCount; n++) {
						string sColumn =
							oReader.GetName(n)
								.Replace(Convert.ToString((char) 195) + Convert.ToString((char) 132), "Ä")
								.Replace(Convert.ToString((char) 195) + Convert.ToString((char) 164), "ä")
								.Replace(Convert.ToString((char) 195) + Convert.ToString((char) 150), "Ö")
								.Replace(Convert.ToString((char) 195) + Convert.ToString((char) 182), "ö")
								.Replace(Convert.ToString((char) 195) + Convert.ToString((char) 156), "Ü")
								.Replace(Convert.ToString((char) 195) + Convert.ToString((char) 188), "ü")
								.Replace(Convert.ToString((char) 195) + Convert.ToString((char) 159), "ß");
						// Spalte '_row_no_' ignorieren
						if (sColumn.Equals("_row_no_"))
							continue;
						string sDescription = string.Empty;
						if (viewscript.ViewInfo.ColumnDictionary.ContainsKey(sColumn)) {
							Dictionary<string, string> langToDescr = viewscript.ViewInfo.ColumnDictionary[sColumn];
							if (langToDescr.Count > 0) sDescription = langToDescr[viewscript.ViewInfo.Languages[0]];
						}
						string sType;
						string sDataTypeName = oReader.GetDataTypeName(n);
						if (oReader.GetDataTypeName(n).Contains("CHAR") ||
							oReader.GetDataTypeName(n).Equals("TEXT")) {
							sType = "string";
						}
						else if
							(sDataTypeName.Contains("INT") ||
							 sDataTypeName.Equals("FLOAT") ||
							 sDataTypeName.Equals("DOUBLE") ||
							 sDataTypeName.Equals("REAL") ||
							 sDataTypeName.Equals("NUMERIC") ||
							 sDataTypeName.Equals("DECIMAL")) {
							sType = "numeric";
						}
						else if (sDataTypeName.StartsWith("DATE")) {
							sType = "date";
						}
						else if (sDataTypeName.Equals("TIME")) {
							sType = "time";
						}
						else if (sDataTypeName.Equals("TIMESTAMP")) {
							sType = "datetime";
						}
						else if (sDataTypeName.Equals("VARBINARY")) {
							OnError("VARBINARY-Typ in '" + viewscript.Name + "." + sColumn + "' gefunden");
							sType = "string";
						}
						else {
							OnError(
								"Nicht zugeordneter Datentyp in " + viewscript.Name + "." + sColumn + "': " +
								sDataTypeName);
							sType = "string";
						}
						string sSql =
							"INSERT INTO " +
							conn.Enquote(conn.DbConfig.DbName + "_system", "col") +
							"(table_id, name, comment, `type`) " +
							"VALUES (" + nLastId + ",'" + sColumn + "'," + conn.GetSqlString(sDescription) + ",'" +
							sType + "')";
						insertSqls.Add(sSql);
					}
					oReader.Close();
				}*/
                foreach (var insertSql in insertSqls)
                    conn.ExecuteNonQuery(insertSql);
                conn.CommitTransaction();
            }
            catch (Exception ex)
            {
                conn.RollbackTransaction();
                if (oReader != null && !oReader.IsClosed)
                    oReader.Close();
                OnError("Fehler beim Hinzufügen der System-DB-Einträge: " + ex.Message);
            }
        }

        #endregion AddToSysDb

        #region CreateViewboxDbEntry

        private void CreateViewboxDbEntry(ViewCreateOptions viewOptions, int threadIndex, IDatabase conn,
                                          Viewscript viewscript)
        {
            GetWorkerState(threadIndex).Task = WorkerTask.SavingIssue;
            GetWorkerState(threadIndex).PreparationProgress = 0;
            //table + columns + table_texts
            var optimizeCriterias = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            if (viewscript.ViewInfo.OptimizeCriterias.DoClientSplit)
                optimizeCriterias.Add("Index", viewscript.ViewInfo.OptimizeCriterias.ClientField);
            if (viewscript.ViewInfo.OptimizeCriterias.DoCompCodeSplit)
                optimizeCriterias.Add("Split", viewscript.ViewInfo.OptimizeCriterias.CompCodeField);
            if (viewscript.ViewInfo.OptimizeCriterias.DoFYearSplit)
                optimizeCriterias.Add("Sort", viewscript.ViewInfo.OptimizeCriterias.GJahrField);
            TableType type = viewscript.HasStoredProcedure ? TableType.Issue : TableType.View;
            string viewScriptName = viewscript.Name;
            if (type == TableType.View && viewscript.ViewInfo.ReportParameterDictionary.Count > 0)
            {
                viewScriptName = viewscript.Name + "_filter";
            }
            ITableObject tobj = null;
            ITableObject tobjIssue = null;
            if (_cancelled) return;
            if (!viewOptions.GenerateDistinctDataOnly)
            {
                lock (Profile.ViewboxDb)
                {
                    // Save ObjectType
                    if (viewscript.ViewInfo.ObjectType != null)
                    {
                        using (IDatabase viewboxConn = Profile.ViewboxDb.ConnectionManager.GetConnection())
                        {
                            Profile.ViewboxDb.SaveObjectType(viewboxConn, viewscript.ViewInfo.ObjectType.Name,
                                                             viewscript.ViewInfo.ObjectType == null
                                                                 ? null
                                                                 : viewscript.ViewInfo.ObjectType.LangToDescription);
                        }
                    }
                    else
                    {
                    }
                    viewScriptName = viewscript.Name;

                    Profile.ViewboxDb.TransactionNumber = tran_number;
                    //We can always create a new object as the old one has been deleted in a prior method
                    tobj = Profile.ViewboxDb.CreateAndSaveTableObject(type, conn.DbConfig.DbName, viewScriptName,
                                                                      conn.CountTable(viewscript.Name),
                                                                      conn.GetColumnInfos(viewscript.Name),
                                                                      optimizeCriterias,
                                                                      viewscript.ViewInfo.GetDescriptions(),
                                                                      viewscript.ViewInfo.ColumnDictionary,
                                                                      viewscript.ViewInfo.ProcedureName, "", null,
                                                                      viewscript.ViewInfo.ObjectType == null
                                                                          ? null
                                                                          : viewscript.ViewInfo.ObjectType.Name,
                                                                      viewscript.ViewInfo.OptimizeCriterias.YearRequired);

                    // Create issue when View is a parameterized view
                    if (type == TableType.View && viewscript.ViewInfo.ReportParameterDictionary.Count > 0)
                    {
                        // DEVNOTE: this is necessary here again
                        viewScriptName = viewscript.Name + "_filter";
                        string command = string.Empty;
                        foreach (string reportFilterLine in viewscript.ViewInfo.ReportFilterLines)
                        {
                            if (!string.IsNullOrEmpty(command)) command += Environment.NewLine;
                            command += reportFilterLine;
                        }
                        string rownocommand = string.Empty;
                        foreach (string reportFilterLine in viewscript.ViewInfo.ReportFilterSpecialLines)
                        {
                            if (!string.IsNullOrEmpty(reportFilterLine))
                                rownocommand += reportFilterLine;
                        }
                        tobjIssue = Profile.ViewboxDb.CreateAndSaveTableObject(TableType.Issue, conn.DbConfig.DbName,
                                                                               viewScriptName,
                                                                               //conn.CountTable(viewscript.Name),
                                                                               -1,
                                                                               // DEVNOTE: this issue is like user defined which should not have row count
                                                                               //conn.GetColumnInfos(viewscript.Name),
                                                                               new List<DbColumnInfo>(),
                                                                               // DEVNOTE: this issue is like user defined which should not have columns
                                                                               optimizeCriterias,
                                                                               viewscript.ViewInfo.GetDescriptions(),
                                                                               viewscript.ViewInfo.ColumnDictionary,
                                                                               command, rownocommand, tobj,
                                                                               viewscript.ViewInfo.ObjectType == null
                                                                                   ? null
                                                                                   : viewscript.ViewInfo.ObjectType.Name,
                                                                               viewscript.ViewInfo.OptimizeCriterias.
                                                                                   YearRequired);
                        IIssue issue = tobjIssue as IIssue;
                        if (issue != null)
                            SaveIssueData(TableType.Issue, viewscript, issue, tobj, new List<IParameter>(),
                                          issue.RowNoFilter);
                    }
                    else
                    {
                        tobjIssue = tobj as IIssue;
                        SaveIssueData(type, viewscript, tobj, null, new List<IParameter>(), "");
                    }
                    // Load newly added stuff
                    using (IDatabase viewboxConn = Profile.ViewboxDb.ConnectionManager.GetConnection())
                    {
                        Profile.SysDb.LoadTables(viewboxConn, true);
                    }
                }
                GetWorkerState(threadIndex).PreparationProgress = 1;

                GetWorkerState(threadIndex).Task = WorkerTask.CalculatingOrderArea;
                GetWorkerState(threadIndex).PreparationProgress = 0;
                //order_area
                if ((viewscript.ViewInfo.OptimizeCriterias.DoClientSplit ||
                     viewscript.ViewInfo.OptimizeCriterias.DoCompCodeSplit ||
                     viewscript.ViewInfo.OptimizeCriterias.DoFYearSplit) && tobj.RowCount > 0 && !(tobj is IIssue))
                {
                    string orderCols = String.Empty;
                    //MANDT
                    if (viewscript.ViewInfo.OptimizeCriterias.DoClientSplit)
                    {
                        orderCols += conn.Enquote(tobj.IndexTableColumn.Name);
                    }
                    //BUKRS
                    if (viewscript.ViewInfo.OptimizeCriterias.DoCompCodeSplit)
                    {
                        if (orderCols.Length > 0)
                            orderCols += ", ";
                        orderCols += conn.Enquote(tobj.SplitTableColumn.Name);
                    }
                    //GJAHR
                    if (viewscript.ViewInfo.OptimizeCriterias.DoFYearSplit)
                    {
                        if (orderCols.Length > 0)
                            orderCols += ", ";
                        orderCols += conn.Enquote(tobj.SortColumn.Name);
                    }
                    if (_cancelled) return;
                    if (!viewscript.ProcedureInfo.OnlyMetadata)
                        using (var db = Profile.ViewboxDb.ConnectionManager.GetConnection())
                        {
                            Profile.ViewboxDb.CalculateOrderArea(tobj, orderCols, db);
                        }
                }
                GetWorkerState(threadIndex).PreparationProgress = 1;
            }
            if (_cancelled) return;
            //// BEGIN ************ Order area for referenced tables ************
            //if ((viewscript.ViewInfo.OptimizeCriterias.DoClientSplit ||
            //     viewscript.ViewInfo.OptimizeCriterias.DoCompCodeSplit ||
            //     viewscript.ViewInfo.OptimizeCriterias.DoFYearSplit)) {
            //    using (IDatabase viewboxdb = Profile.ViewboxDb.ConnectionManager.GetConnection()) {
            //        bool change = false;
            //        foreach (KeyValuePair<string, ReportParameter> reportParam in viewscript.ViewInfo.ReportParameterDictionary) {
            //            if (CreateOrderAreaForReference(threadIndex, viewscript, viewboxdb, reportParam.Value.DatabaseName, reportParam.Value.TableName, tobjIssue)) {
            //                change = true;
            //            }
            //        }
            //        foreach (KeyValuePair<string, ProcedureParameter> procedureParam in viewscript.ViewInfo.ParameterDictionary) {
            //            if (CreateOrderAreaForReference(threadIndex, viewscript, viewboxdb, procedureParam.Value.DatabaseName, procedureParam.Value.TableName, tobjIssue)) {
            //                change = true;
            //            }
            //        }
            //        if (change) {
            //            Profile.SysDb.LoadTables(viewboxdb, true);
            //        }
            //    }
            //}
            //// END ************ Order area for referenced tables ************
            GetWorkerState(threadIndex).Task = WorkerTask.GeneratingDistinctValues;
            try
            {
                if (_cancelled) return;
                if ((!viewOptions.GenerateDistinctDataOnly && Profile.AutoGenerateIndex)
                    ||
                    (viewOptions.GenerateDistinctDataOnly &&
                     ViewExists(viewScriptName, out tobjIssue, threadIndex, viewscript)))
                {
                    GetWorkerState(threadIndex).Task = WorkerTask.GeneratingDistinctValues;
                    //DEVNOTE: generate distinct values for parameters after order areas are created
                    _log.InfoFormatWithCheck("Generating distinct values for parameters started...");
                    if (type == TableType.View && viewscript.ViewInfo.ReportParameterDictionary.Any())
                    {
                        CreateDistinctValuesForParameters(tobjIssue as IIssue, threadIndex);
                    }
                    else if (type == TableType.Issue && viewscript.ViewInfo.ParameterDictionary.Any())
                    {
                        CreateDistinctValuesForParameters(tobjIssue as IIssue, threadIndex);
                    }
                    _log.InfoFormatWithCheck("Generating distinct values for parameters finished.");
                }
            }
            catch (Exception)
            {
                viewscript.State = ViewscriptStates.GeneratingDistinctValuesError;
                throw;
            }
        }

        private bool CreateOrderAreaForReference(int threadIndex, Viewscript viewscript, IDatabase viewboxDb,
                                                 string databaseName, string tableName, ITableObject issue)
        {
            const string client = "MANDT";
            const string company = "BUKRS";
            const string financialYear = "GJAHR";
            bool change = false;
            ITableObject refTbl =
                Profile.SysDb.Tables.Where(
                    t =>
                    string.Compare(t.Database, databaseName, StringComparison.InvariantCultureIgnoreCase) == 0 &&
                    string.Compare(t.TableName, tableName, StringComparison.InvariantCultureIgnoreCase) == 0).
                    FirstOrDefault();
            // if not a table try to find fake table
            if (refTbl == null)
                refTbl =
                    Profile.SysDb.GetFakeTables(viewboxDb).Where(
                        t =>
                        string.Compare(t.Database, databaseName, StringComparison.InvariantCultureIgnoreCase) == 0 &&
                        string.Compare(t.TableName, tableName, StringComparison.InvariantCultureIgnoreCase) == 0).
                        FirstOrDefault();
            if (refTbl != null)
            {
                // Check order area exists
                if (!OrderAreaExists(refTbl, threadIndex, viewscript))
                {
                    string orderCols = String.Empty;
                    if (refTbl is FakeTable)
                    {
                        //MANDT
                        if (viewscript.ViewInfo.OptimizeCriterias.DoClientSplit)
                        {
                            IColumn col =
                                issue.Columns.FirstOrDefault(
                                    c =>
                                    string.Compare(c.Name, client, StringComparison.InvariantCultureIgnoreCase) == 0);
                            refTbl.Columns.Add(col);
                            (refTbl as TableObject).IndexTableColumn = col;
                            orderCols += viewboxDb.Enquote(client);
                        }
                        //BUKRS
                        if (viewscript.ViewInfo.OptimizeCriterias.DoCompCodeSplit)
                        {
                            IColumn col =
                                issue.Columns.FirstOrDefault(
                                    c =>
                                    string.Compare(c.Name, company, StringComparison.InvariantCultureIgnoreCase) == 0);
                            refTbl.Columns.Add(col);
                            (refTbl as TableObject).IndexTableColumn = col;
                            if (orderCols.Length > 0) orderCols += ", ";
                            orderCols += viewboxDb.Enquote(company);
                        }
                        //GJAHR
                        if (viewscript.ViewInfo.OptimizeCriterias.DoFYearSplit)
                        {
                            IColumn col =
                                issue.Columns.FirstOrDefault(
                                    c =>
                                    string.Compare(c.Name, financialYear, StringComparison.InvariantCultureIgnoreCase) ==
                                    0);
                            refTbl.Columns.Add(col);
                            (refTbl as TableObject).IndexTableColumn = col;
                            if (orderCols.Length > 0) orderCols += ", ";
                            orderCols += viewboxDb.Enquote(financialYear);
                        }
                    }
                    else
                    {
                        //MANDT
                        if (refTbl.IndexTableColumn != null && !refTbl.IndexTableColumn.IsEmpty)
                        {
                            orderCols += viewboxDb.Enquote(refTbl.IndexTableColumn.Name);
                        }
                        //BUKRS
                        if (refTbl.SplitTableColumn != null && !refTbl.SplitTableColumn.IsEmpty)
                        {
                            if (orderCols.Length > 0) orderCols += ", ";
                            orderCols += viewboxDb.Enquote(refTbl.SplitTableColumn.Name);
                        }
                        //GJAHR
                        if (refTbl.SortColumn != null && !refTbl.SortColumn.IsEmpty)
                        {
                            if (orderCols.Length > 0) orderCols += ", ";
                            orderCols += viewboxDb.Enquote(refTbl.SortColumn.Name);
                        }
                    }
                    try
                    {
                        Profile.ViewboxDb.CalculateOrderArea(refTbl, orderCols, viewboxDb);
                    }
                    catch (Exception ex)
                    {
                        _log.Warn(string.Format("Calculate order areas for FakeTable [{0}] failed", refTbl.Name), ex);
                        // if creating order areas for fake table fails because of no _row_no_ field, then ignore the error
                    }
                    change = true;
                }
            }
            return change;
        }

        private bool OrderAreaExists(ITableObject table, int threadIndex, Viewscript viewscript)
        {
            bool retVal = true;
            if (table.OrderAreas == null || table.OrderAreas.Count == 0)
                return false;

            List<OrderArea> orderAreas = table.OrderAreas.Select(oa => (OrderArea) oa).ToList();
            if (orderAreas.Count > 0) Profile.SysDb.ExtendTheOrderAreas(orderAreas);
            if (viewscript.ViewInfo.OptimizeCriterias.DoClientSplit && table.IndexTableColumn != null &&
                !table.IndexTableColumn.IsEmpty)
            {
                if (viewscript.ViewInfo.OptimizeCriterias.DoCompCodeSplit && table.SplitTableColumn != null &&
                    !table.SplitTableColumn.IsEmpty)
                {
                    if (viewscript.ViewInfo.OptimizeCriterias.DoFYearSplit && table.SortColumn != null &&
                        !table.SortColumn.IsEmpty)
                    {
                        orderAreas = table.OrderAreas.Where(oa =>
                                                            !string.IsNullOrWhiteSpace(oa.IndexValue) &&
                                                            !string.IsNullOrWhiteSpace(oa.SplitValue) &&
                                                            !string.IsNullOrWhiteSpace(oa.SortValue)).Select(
                                                                oa => (OrderArea) oa).ToList();
                    }
                    else
                    {
                        orderAreas = table.OrderAreas.Where(oa =>
                                                            !string.IsNullOrWhiteSpace(oa.IndexValue) &&
                                                            !string.IsNullOrWhiteSpace(oa.SplitValue)).Select(
                                                                oa => (OrderArea) oa).ToList();
                    }
                }
                else
                {
                    orderAreas =
                        table.OrderAreas.Where(oa => !string.IsNullOrWhiteSpace(oa.IndexValue)).Select(
                            oa => (OrderArea) oa).ToList();
                }
            }
            else
            {
                orderAreas = table.OrderAreas.Select(oa => (OrderArea) oa).ToList();
            }
            if (orderAreas.Count > 0) retVal = true;
            return retVal;
        }

        private bool ViewExists(string viewName, out ITableObject issue, int threadIndex, Viewscript viewscript)
        {
            bool retVal = false;
            using (IDatabase viewboxConn = Profile.ViewboxDb.ConnectionManager.GetConnection())
            {
                GetWorkerState(threadIndex).Task = WorkerTask.Preparing;
                GetWorkerState(threadIndex).PreparationProgress = 0;
                try
                {
                    //issue = viewboxConn.DbMapping.Load<SystemDb.Internal.Issue>("name = '" + viewName + "'").FirstOrDefault();
                    if (Profile.SysDb.Issues == null || Profile.SysDb.Issues.Count == 0)
                        Profile.SysDb.LoadTables(viewboxConn, true);
                    issue =
                        Profile.SysDb.Issues.Where(
                            i => string.Compare(i.TableName, viewName, StringComparison.InvariantCultureIgnoreCase) == 0)
                            .FirstOrDefault();
                    retVal = issue != null;
                }
                catch (Exception)
                {
                    viewscript.State = ViewscriptStates.CheckingReportParametersError;
                    throw new ArgumentException(string.Format("View [{0}] does not exists.", viewName));
                    //issue = null;
                    //retVal = false;
                }
                GetWorkerState(threadIndex).PreparationProgress = 1;
                return retVal;
            }
        }

        private void SaveIssueData(TableType type, Viewscript viewscript, ITableObject tobj, ITableObject parentObject,
                                   List<IParameter> parameters, string rowNoFilter)
        {
            if (type == TableType.Issue)
            {
                using (IDatabase viewboxConn = Profile.ViewboxDb.ConnectionManager.GetConnection())
                {
                    if (parentObject == null)
                    {
                        Profile.ViewboxDb.SaveIssueExtension(viewboxConn, tobj as IIssue, parameters, null, rowNoFilter);
                        Profile.ViewboxDb.ModifyIssue(viewboxConn, tobj as IIssue, null);
                    }
                    else
                    {
                        Profile.ViewboxDb.SaveIssueExtension(viewboxConn, tobj as IIssue, parameters, parentObject,
                                                             rowNoFilter);
                        Profile.ViewboxDb.ModifyIssue(viewboxConn, tobj as IIssue, null);
                    }
                }
                SaveIssueParameters(viewscript, tobj as IIssue);
            }
        }

        #endregion CreateViewboxDbEntry

        #region SaveIssueParameters

        private void SaveIssueParameters(Viewscript viewscript, IIssue tobj)
        {
            List<IParameter> parameters = new List<IParameter>();
            Dictionary<string, List<string>> parameterTexts = new Dictionary<string, List<string>>();
            foreach (ILanguage lang in Profile.ViewboxDb.Languages)
            {
                parameterTexts[lang.CountryCode] = new List<string> {"unused"};
            }
            if (viewscript.ViewInfo.ParameterDictionary.Values.Count > 0)
            {
                foreach (ProcedureParameter parameter in viewscript.ViewInfo.ParameterDictionary.Values)
                {
                    Parameter param =
                        tobj.Parameters.FirstOrDefault(
                            p =>
                            string.Compare(p.Name, parameter.Name, StringComparison.InvariantCultureIgnoreCase) == 0) as
                        Parameter;
                    if (param == null) param = new Parameter();
                    param.ColumnName = parameter.ColumnName;
                    param.DataType = parameter.Type;
                    param.TypeModifier = parameter.TypeModifier;
                    //param.DatabaseName = tobj.Database;
                    //param.DatabaseName = string.IsNullOrEmpty(param.DatabaseName) ? tobj.Database : param.DatabaseName;
                    if (String.IsNullOrEmpty(param.DatabaseName))
                    {
                        if (!String.IsNullOrEmpty(parameter.DatabaseName))
                        {
                            param.DatabaseName = parameter.DatabaseName;
                        }
                        else
                        {
                            param.DatabaseName = tobj.Database;
                        }
                    }
                    param.Default = parameter.DefaultValue;
                    param.TableName = parameter.TableName;
                    param.Name = parameter.Name;
                    param.Issue = tobj;
                    param.UserDefined = false;
                    parameters.Add(param);
                    foreach (ILanguage lang in Profile.ViewboxDb.Languages)
                    {
                        parameterTexts[lang.CountryCode].Add(!parameter.LangToDescription.ContainsKey(lang.CountryCode)
                                                                 ? parameter.Name
                                                                 : parameter.LangToDescription[lang.CountryCode]);
                    }
                }
            }
            else if (viewscript.ViewInfo.ReportParameterDictionary.Count > 0)
            {
                foreach (ReportParameter parameter in viewscript.ViewInfo.ReportParameterDictionary.Values)
                {
                    Parameter param =
                        tobj.Parameters.FirstOrDefault(
                            p =>
                            string.Compare(p.Name, parameter.ParameterName, StringComparison.InvariantCultureIgnoreCase) ==
                            0) as Parameter;
                    if (param == null) param = new Parameter();
                    param.ColumnName = parameter.ColumnName;
                    param.DataType = parameter.Type;
                    param.TypeModifier = parameter.TypeModifier;
                    param.DatabaseName = tobj.Database;
                    param.Default = parameter.DefaultValue;
                    param.TableName = parameter.TableName;
                    param.Name = parameter.ParameterName;
                    param.Issue = tobj;
                    param.UserDefined = false;
                    parameters.Add(param);
                    foreach (ILanguage lang in Profile.ViewboxDb.Languages)
                    {
                        parameterTexts[lang.CountryCode].Add(
                            !parameter.ParameterDescription.ContainsKey(lang.CountryCode)
                                ? parameter.ParameterName
                                : parameter.ParameterDescription[lang.CountryCode]);
                    }
                }
            }
            using (IDatabase viewboxConn = Profile.ViewboxDb.ConnectionManager.GetConnection())
            {
                Profile.ViewboxDb.SaveIssueParameters(viewboxConn,
                                                      Profile.ViewboxDb.Languages,
                                                      parameterTexts,
                                                      tobj, parameters);
            }
        }

        #endregion SaveIssueParameters

        #region CreateTmpViewDb

        /// <summary>
        ///   Creates the temporary view database for the specified worker.
        /// </summary>
        /// <param name="threadIndex"> </param>
        /// <param name="conn"> </param>
        private void CreateTmpViewDb(int threadIndex, WorkerState state)
        {
            using (IDatabase conn = Profile.ConnectionManager.GetConnection())
            {
                string tmpViewDbName = conn.DbConfig.DbName + TmpViewDbNameBase + threadIndex;

                // create new temporary view database (any existing one will be dropped before)
                conn.DropDatabaseIfExists(tmpViewDbName);
                conn.CreateDatabaseIfNotExists(tmpViewDbName);

                //var tableNames = Profile.SysDb.GetTableNamesAndDescriptionsFromSystemDb();
                var tableNames = new List<string>();
                lock (_views)
                {
                    var query = from v in _views select v.GetTables();
                    foreach (var tables in query)
                    {
                        foreach (var t in tables)
                            if (!tableNames.Contains(t.Key))
                                tableNames.Add(t.Key);
                    }
                }

                CreateTempViewTable(state, conn, tmpViewDbName, tableNames, new ProgressCalculator());
            }
        }

        public void CreateTempViewTable(WorkerState state, IDatabase conn, string tmpViewDbName, List<string> tableNames,
                                        ProgressCalculator progress)
        {
            int curIndex = -1;
            int maxIndex = tableNames.Count - 1;
            foreach (var tableName in tableNames)
            {
                if (_cancelled || progress.CancellationPending)
                    break;
                progress.Description = "erstelle View für " + tableName + " ...";
                curIndex++;
                try
                {
                    if (maxIndex > 0)
                    {
                        state.PreparationProgress = curIndex/(double) maxIndex;
                    }
                    else
                    {
                        state.PreparationProgress = 0;
                    }
                    string sql =
                        "CREATE OR REPLACE VIEW " +
                        conn.Enquote(tmpViewDbName, (tableName.First() == '_' ? "" : "_") + tableName) + " " +
                        "AS SELECT " +
                        GetColumnNames(tableName, conn) +
                        " " +
                        "FROM " + conn.Enquote(tableName);
                    conn.ExecuteNonQuery(sql);
                }
                catch (Exception ex)
                {
                    OnError("Fehler bei Erstellung der temporären Viewdatenbank: " + ex.Message);
                }
                progress.StepDone();
            }
        }

        private string GetColumnNames(string tableName, IDatabase conn)
        {
            List<string> columnNames = conn.GetColumnNames(tableName);
            ITableObject table =
                Profile.ViewboxDb.Objects.FirstOrDefault(
                    obj =>
                    obj.TableName.ToLower() == tableName.ToLower() && obj.Database.ToLower() == Profile.DbConfig.DbName);
            StringBuilder additionalColumns = new StringBuilder();
            if (table != null)
            {
                //Add all empty columns
                var emptyColumns = Profile.ViewboxDb.Columns.Where(c => c.Table == table && c.IsEmpty);
                foreach (var emptyColumn in emptyColumns)
                {
                    if (!columnNames.Contains(emptyColumn.Name))
                    {
                        string constValue;
                        int number;
                        if (emptyColumn.ConstValue == null)
                            constValue = "CAST(NULL AS CHAR(1))";
                        else if (emptyColumn.ConstValue == string.Empty)
                            constValue = "CAST('' AS CHAR(1))";
                        else if (Int32.TryParse(emptyColumn.ConstValue, out number))
                            constValue = number.ToString(CultureInfo.InvariantCulture);
                        else
                            constValue = conn.GetSqlString(emptyColumn.ConstValue);
                        additionalColumns.Append(string.Format(",{0} AS {1}", constValue,
                                                               conn.Enquote(emptyColumn.Name)));
                    }
                }
            }

            return String.Join(",", from cName in columnNames select conn.Enquote(cName)) +
                   additionalColumns;
        }

        #endregion CreateTmpViewDb

        #region DeleteView

        public void DeleteView(View view)
        {
            //Delete from system and viewbox db
            using (var viewboxConn = Profile.SysDb.ConnectionManager.GetConnection())
                DeleteTableEntriesIfExist(viewboxConn, view.Name);
            using (IDatabase conn = Profile.ConnectionManager.GetConnection())
            {
                //Drop table in database
                conn.DropTableIfExists(view.Name);
                //Drop procedure if this is a dynamic view
                Viewscript viewscript = Profile.GetViewscript(view.Name);
                if (viewscript != null && viewscript.HasStoredProcedure)
                {
                    conn.DropProcedureIfExists(viewscript.ViewInfo.ProcedureName);
                }
            }
            //Delete in data structures
            view.DeleteAllWithName();
            Profile.RemoveAllViewsWithName(view.Name);
        }

        #endregion DeleteView

        #region UpdateMetadata

        public void UpdateMetadata(View view)
        {
            Viewscript viewscript = Profile.GetViewscript(view.Name);
            if (viewscript == null)
                throw new Exception(
                    //string.Format(
                    //    "Kein Viewskript namens {0} vorhanden, von welchem das Data Dictionary zum Ersetzen genommen werden kann.",
                    //    view.Name));
                    string.Format(
                        "Viewskript [{0}] not found, cannot be actualized.",
                        view.Name));
            //Check if columns did not change
            ITableObject table = Profile.ViewboxDb.Objects[Profile.DbConfig.DbName + "." + viewscript.Name];
            if (table == null)
                throw new Exception("In der Viewbox Datenbank konnte die eingespielte View nicht gefunden werden");
            //var columnsOld = table.Columns;
            var columnsOld =
                Profile.ViewboxDb.Columns.Where(c => c.Table.Id == table.Id).OrderBy(c => c.Name.ToLower()).ToList();
            var columnsNew = viewscript.ViewInfo.ColumnDictionary.Keys.OrderBy(name => name.ToLower()).ToList();
            for (int i = 0; i < columnsOld.Count(); ++i)
            {
                if (i >= columnsNew.Count)
                    throw new Exception(
                        string.Format(
                            "Die Struktur des Views hat sich geändert, die Spalte {0} wurde gelöscht, bitte View neu einspielen",
                            columnsOld[i].Name));
                if (columnsOld[i].Name.ToLower() != columnsNew[i].ToLower())
                    throw new Exception(
                        string.Format(
                            "Die Struktur des Views hat sich geändert, die Spalten {0}/{1} im alten/neuen View stimmen nicht überein, eine der Spalten wurden hinzugefügt/gelöscht, bitte View neu einspielen",
                            columnsOld[i].Name, columnsNew[i]));
            }
            if (columnsOld.Count < columnsNew.Count)
                throw new Exception(
                    string.Format("Die Struktur des Views hat sich geändert, es sind {0} neue Spalten hinzugekommen.",
                                  columnsNew.Count - columnsOld.Count));
            //Check if optimize section did not change
            if (viewscript.HasStoredProcedure)
            {
                if ((table as IIssue).UseIndexValue != viewscript.ViewInfo.OptimizeCriterias.DoClientSplit
                    || (table as IIssue).UseSplitValue != viewscript.ViewInfo.OptimizeCriterias.DoCompCodeSplit
                    || (table as IIssue).UseSortValue != viewscript.ViewInfo.OptimizeCriterias.DoFYearSplit
                    )
                    throw new Exception("Optimierungskriterien weichen ab, bitte View neu einspielen");
            }
            else
            {
                if (table.IndexTableColumn != null ^ viewscript.ViewInfo.OptimizeCriterias.DoClientSplit
                    || table.SplitTableColumn != null ^ viewscript.ViewInfo.OptimizeCriterias.DoCompCodeSplit
                    || table.SortColumn != null ^ viewscript.ViewInfo.OptimizeCriterias.DoFYearSplit)
                    throw new Exception("Optimierungskriterien weichen ab, bitte View neu einspielen");
                if (table.IndexTableColumn != null &&
                    table.IndexTableColumn.Name.ToLower() != viewscript.ViewInfo.OptimizeCriterias.ClientField.ToLower()
                    ||
                    table.SplitTableColumn != null &&
                    table.SplitTableColumn.Name.ToLower() !=
                    viewscript.ViewInfo.OptimizeCriterias.CompCodeField.ToLower()
                    ||
                    table.SortColumn != null &&
                    table.SortColumn.Name.ToLower() != viewscript.ViewInfo.OptimizeCriterias.GJahrField.ToLower())
                    throw new Exception(
                        "Die Spalten bei den Optimierungskriterien weichen ab, bitte View neu einspielen");
            }
            //Update Metadata: deleting table texts, column texts, parameter texts and re-adding them
            using (var conn = Profile.ViewboxDb.ConnectionManager.GetConnection())
            {
                //Delete all texts
                //DeleteFromViewboxDb(conn, table.Id, true, Profile);
                try
                {
                    conn.BeginTransaction();
                    DeleteFromViewboxDb(conn, table.Id, true, Profile);
                    conn.CommitTransaction();
                }
                catch (Exception ex)
                {
                    if (conn.HasTransaction())
                        conn.RollbackTransaction();
                    _log.Error(ex.Message, ex);
                    throw;
                }
                //Reload all data
                Profile.ViewboxDb.LoadTables(conn);
                table = Profile.ViewboxDb.Objects[Profile.DbConfig.DbName + "." + viewscript.Name];
                if (table == null)
                    throw new Exception("In der Viewbox Datenbank konnte die eingespielte View nicht gefunden werden");
                if (viewscript.HasStoredProcedure)
                {
                    //Add parameters + parameter texts
                    IIssue issue = table as IIssue;
                    foreach (var param in viewscript.ViewInfo.ParameterDictionary.Values)
                    {
                        var parameterTexts = new Dictionary<string, List<string>>();
                        //Parameter was not found - no problem, then no update on the texts is done
                        if (!issue.Parameters.Contains(param.Name))
                            continue;
                        IParameter issueParam = issue.Parameters[param.Name];
                        foreach (var lang in Profile.ViewboxDb.Languages)
                        {
                            parameterTexts[lang.CountryCode] = new List<string>
                                                                   {
                                                                       !param.LangToDescription.ContainsKey(
                                                                           lang.CountryCode)
                                                                           ? param.Name
                                                                           : param.LangToDescription[lang.CountryCode]
                                                                   };
                        }
                        using (IDatabase viewboxConn = Profile.ViewboxDb.ConnectionManager.GetConnection())
                        {
                            SystemDb.SystemDb.SaveParameterTexts(viewboxConn, Profile.ViewboxDb.Languages,
                                                                 parameterTexts, -1, issueParam);
                        }
                    }
                }
                //Add table texts
                Profile.ViewboxDb.AddTableTexts(viewscript.ViewInfo.GetDescriptions(), table, conn);
                //Add column texts
                foreach (var column in table.Columns)
                    Profile.ViewboxDb.AddColumnTexts(viewscript.ViewInfo.ColumnDictionary, conn, column.Name, column.Id);
            }
            //Delete old meta data and add new
            //lock (Profile.ViewboxDb) {
            //    using (var viewboxConn = Profile.SysDb.ConnectionManager.GetConnection()) {
            //        DeleteTableEntriesIfExist(viewboxConn, viewscript.Name);
            //    }
            //    using (IDatabase conn = Profile.ConnectionManager.GetConnection()) {
            //        AddToSysDb(conn, viewscript);
            //        CreateViewboxDbEntry(conn, viewscript);
            //    }
            //}
        }

        #endregion UpdateMetadata

        #region UpdateAllMetadata

        public void UpdateAllMetadata(IEnumerable<View> views, ProgressCalculator progress)
        {
            progress.SetWorkSteps(views.Count(), false);
            string errors = "";
            foreach (var view in views)
            {
                if (progress.CancellationPending)
                    throw new TaskCanceledException("Metadaten Aktualisierung abgebrochen.");
                try
                {
                    progress.Description = "View " + view.Name;
                    UpdateMetadata(view);
                    Console.WriteLine("Successful: " + view.Name);
                }
                catch (Exception ex)
                {
                    errors += Environment.NewLine + view.Name + ": " + ex.Message;
                    Console.WriteLine("error in view " + view.Name + Environment.NewLine + ex.Message);
                }
                progress.StepDone();
            }
            if (!string.IsNullOrEmpty(errors))
                throw new Exception(errors);
        }

        #endregion UpdateAllMetadata

        #region DeleteAndAddEntries

        public void DeleteAndAddEntries(IEnumerable<Viewscript> viewscripts)
        {
            //Viewscript viewscript = Profile.GetViewscript(view.Name);
            //if(viewscript == null) throw new Exception(string.Format("Kein Viewskript namens {0} vorhanden, bei dem die .", view.Name));
            using (var conn = Profile.ConnectionManager.GetConnection())
            {
                foreach (var viewscript in viewscripts)
                {
                    if (!viewscript.IsChecked)
                        continue;
                    DeleteAndAddEntries(new ViewCreateOptions {GenerateDistinctDataOnly = false}, -1, viewscript, conn);
                }
            }
        }

        #endregion DeleteAndAddEntries

        #region GetNextViewscript

        /// <summary>
        ///   Gets the next view.
        /// </summary>
        /// <param name="threadIndex"> Index of the thread. </param>
        /// <returns> </returns>
        private Viewscript GetNextViewscript()
        {
            Viewscript result = null;
            lock (_views)
            {
                if (_views.Count > 0)
                {
                    int i = 0;
                    var tables = from t in _views[i].GetTables() select t.Key.ToLower();
                    var indexTables = from t in _runningIndizes select t.Value.Key.ToLower();
                    while (tables.Any(t => indexTables.Contains(t)))
                    {
                        if (i + 1 < _views.Count)
                        {
                            i++;
                        }
                        else
                        {
                            i = 0;
                            Thread.Sleep(1000*10);
                        }
                        tables = from t in _views[i].GetTables() select t.Key.ToLower();
                        indexTables = from t in _runningIndizes select t.Value.Key.ToLower();
                    }
                    result = _views[i];
                    _views.RemoveAt(i);
                }
            }
            return result;
        }

        #endregion GetNextViewscript

        #region GetNextIndex

        private KeyValuePair<string, List<Index>> GetNextIndex(int threadIndex)
        {
            KeyValuePair<string, List<Index>> result = default(KeyValuePair<string, List<Index>>);
            lock (_indizes)
            {
                if (_indizes.Count > 0)
                {
                    result = _indizes.ElementAt(0);
                    lock (_runningIndizes)
                        _runningIndizes[threadIndex] = result;
                    _indizes.Remove(result.Key);
                }
            }
            return result;
        }

        #endregion GetNextIndex

        #region WorkerThread

        /// <summary>
        ///   Main method for the worker threads.
        /// </summary>
        /// <param name="param"> The param. </param>
        private void WorkerThread(object param)
        {
            ViewCreateOptions viewOptions = null;
            try
            {
                viewOptions = ((object[]) param)[0] as ViewCreateOptions;
            }
            catch (Exception)
            {
            }
            if (viewOptions == null)
                throw new ArgumentException("The type of first parameter in object[] should be ViewCreateOptions!",
                                            "param");

            var threadIndex = (int) ((object[]) param)[1];
            var readyEvent = (AutoResetEvent) ((object[]) param)[2];
            //var workerState = new WorkerState(threadIndex);
            var workerState = _workerStates[threadIndex];
            OnWorkerStarted(workerState);
            try
            {
                // update worker state
                workerState.Task = WorkerTask.Preparing;
                if (!viewOptions.GenerateDistinctDataOnly)
                {
                    // create temporary view database
                    if (!_cancelled)
                        CreateTmpViewDb(threadIndex, workerState);
                    // update worker state
                    workerState.Task = WorkerTask.CreatingIndex;
                    //CreateIndizes
                    var indexPair = GetNextIndex(threadIndex);
                    while (!_cancelled && !indexPair.Equals(default(KeyValuePair<string, List<Index>>)))
                    {
                        var job = new CreateIndexJob {Name = indexPair.Key};
                        job.StartStopwatch();
                        try
                        {
                            workerState.JobInfo = job;
                            CreateIndex(indexPair);
                            indexPair = GetNextIndex(threadIndex);
                            State.ProcessedIndices++;
                        }
                        finally
                        {
                            job.StopStopwatch();
                        }
                    }
                    if (_runningIndizes.ContainsKey(threadIndex))
                        _runningIndizes.Remove(threadIndex);
                }
                //CreateViews
                workerState.Task = WorkerTask.WaitingForNextViewscript;
                Viewscript viewscript = GetNextViewscript();
                while (!_cancelled && viewscript != null)
                {
                    workerState.Task = WorkerTask.Working;
                    workerState.JobInfo = viewscript;
                    CreateView(viewOptions, threadIndex, viewscript);
                    workerState.Task = WorkerTask.WaitingForNextViewscript;
                    workerState.JobInfo = null;
                    viewscript = GetNextViewscript();
                    State.ProcessedViews++;
                }
                // update worker state
                workerState.Task = WorkerTask.Cleanup;
                // drop temporary view database
                //Do not drop as this may be helpful for debugging
                //using (IDatabase conn = Profile.ConnectionManager.GetConnection()) {
                //    conn.DropDatabaseIfExists(conn.DbConfig.DbName + TmpViewDbNameBase + threadIndex);
                //}
            }
            catch (Exception ex)
            {
                OnError(ex.Message + ", stacktrace: " + ex.StackTrace);
            }
            OnWorkerFinished(workerState);
            readyEvent.Set();
        }

        #endregion WorkerThread

        #region CreateViews

        /// <summary>
        ///   MainFunction, starts the worker threads.
        /// </summary>
        public void CreateViews(object options)
        {
            try
            {
                ViewCreateOptions viewOptions = options as ViewCreateOptions;
                if (viewOptions == null)
                    throw new ArgumentException("The type of parameter should be ViewCreateOptions!", "options");
                IsRunning = true;
                _cancelled = false;
                InitProcessList();
                int workers = Math.Min(Profile.MaxWorkerThreads.Value, _views.Count);
                if (workers > 0)
                {
                    var readyEvent = new AutoResetEvent[workers];
                    _workerStates = new WorkerState[workers];
                    for (int i = 0; i < workers; i++)
                    {
                        readyEvent[i] = new AutoResetEvent(false);
                        _workerStates[i] = new WorkerState(i);
                        var thread = new Thread(WorkerThread) {Name = "ViewBuilderWorker" + i};
                        thread.Start(new object[] {viewOptions, i, readyEvent[i]});
                    }
                    WaitHandle.WaitAll(readyEvent);
                }
                _workerStates = null;
            }
            catch (Exception ex)
            {
                OnError(ex.Message);
            }
            finally
            {
                OnFinished();
                IsRunning = false;
                if (State != null)
                    State.Dispose();
            }
        }

        #endregion CreateViews

        #region CreateIndex

        private void CreateIndex(KeyValuePair<string, List<Index>> indexPair)
        {
            using (IDatabase db = Profile.ConnectionManager.GetConnection())
            {
                var sql = String.Format("ALTER TABLE {0} {1}", db.Enquote(indexPair.Key),
                                        String.Join(", ", from index in indexPair.Value
                                                          select
                                                              "ADD INDEX " +
                                                              (index.Name.Length > 63
                                                                   ? index.Name.Substring(index.Name.Length - 63)
                                                                   : index.Name) + "(" +
                                                              //String.Join(",", index.Columns) + ")"));
                                                              String.Join(",", index.Columns.Select(c => db.Enquote(c))) +
                                                              ")"));
                try
                {
                    db.ExecuteNonQuery(sql);
                }
                catch (Exception ex)
                {
                    throw new Exception(
                        "Fehler bei der Indexerstellung, wahrscheinlich existiert eine Spalte nicht. Abgesetztes SQL Statement: " +
                        Environment.NewLine + sql + Environment.NewLine + Environment.NewLine + ex.Message);
                }
            }
        }

        #endregion CreateIndex

        #region CreateView

        /// <summary>
        ///   Creates the specified view.
        /// </summary>
        /// <param name="viewOptions"> Options for generating views </param>
        /// <param name="threadIndex"> Index of the thread. </param>
        /// <param name="viewscript"> </param>
        private void CreateView(ViewCreateOptions viewOptions, int threadIndex, Viewscript viewscript)
        {
            if (_cancelled)
                return;
            using (NDC.Push(LogHelper.GetNamespaceContext()))
            {
                try
                {
                    var tmpViewDbName = String.Empty;
                    using (IDatabase conn = Profile.ConnectionManager.GetConnection())
                    {
                        conn.SetHighTimeout();
                        viewscript.Sql = string.Empty;
                        viewscript.LastError = string.Empty;
                        viewscript.Warnings = string.Empty;
                        if (!viewOptions.GenerateDistinctDataOnly)
                            viewscript.State = ViewscriptStates.CreatingTable;
                        viewscript.SaveOrUpdate();
                        viewscript.ViewIsRunningLong -= viewscript_ViewIsRunningLong;
                        viewscript.ViewIsRunningLong += viewscript_ViewIsRunningLong;
                        viewscript.StartStopwatch();
                        tmpViewDbName = conn.DbConfig.DbName + TmpViewDbNameBase + threadIndex;
                    }
                    // create view
                    try
                    {
                        if (!viewOptions.GenerateDistinctDataOnly && !viewscript.ProcedureInfo.OnlyMetadata)
                        {
                            var tmpDbConfig = Profile.DbConfig.Clone() as DbConfig;
                            tmpDbConfig.DbName = tmpViewDbName;
                            using (var conn = ConnectionManager.CreateConnection(tmpDbConfig))
                            {
                                conn.Open();
                                GetWorkerState(threadIndex).Connection = conn;
                                //Do temporary stuff
                                if (!string.IsNullOrEmpty(viewscript.ViewInfo.TempStatementsComplete))
                                    foreach (var statement in viewscript.ViewInfo.TempStatements)
                                        conn.ExecuteNonQuery(statement);
                                viewscript.SaveOrUpdate();
                                foreach (string sql in viewscript.ViewInfo.Statements)
                                {
                                    if (_cancelled)
                                        break;
                                    viewscript.Sql = sql;
                                    //viewscript.SaveOrUpdate();
                                    if (Index.CheckSql(sql))
                                    {
                                        // found create index command
                                        var index = new Index(sql);
                                        if (!conn.IsIndexExisting(index.Table, index.Columns))
                                        {
                                            //Do not let the user abort indices
                                            GetWorkerState(threadIndex).Connection = null;
                                            conn.ExecuteNonQuery(sql);
                                            GetWorkerState(threadIndex).Connection = conn;
                                        }
                                    }
                                    else
                                    {
                                        conn.ExecuteNonQuery(sql);
                                    }
                                }
                                viewscript.Sql = string.Empty;
                                GetWorkerState(threadIndex).Connection = null;
                            }
                        }
                    }
                    catch (Exception)
                    {
                        viewscript.State = ViewscriptStates.CreateTableError;
                        throw;
                    }
                    if (!viewOptions.GenerateDistinctDataOnly)
                        viewscript.State = ViewscriptStates.CopyingTable;
                    viewscript.SaveOrUpdate();
                    // copy view
                    if (!_cancelled)
                    {
                        try
                        {
                            if (!viewOptions.GenerateDistinctDataOnly)
                            {
                                //create stored procedures
                                if (viewscript.HasStoredProcedure)
                                {
                                    using (var conn = Profile.ConnectionManager.GetConnection())
                                    {
                                        GetWorkerState(threadIndex).Connection = conn;
                                        if (viewscript.ProcedureInfo.ExecuteCreateTempTables)
                                        {
                                            foreach (
                                                var statement in viewscript.ViewInfo.ProcedureCreateTempTablesStatements
                                                )
                                                conn.ExecuteNonQuery(statement);
                                        }
                                        if (viewscript.ProcedureInfo.ExecuteStoredProcedure)
                                        {
                                            foreach (var statement in viewscript.ViewInfo.ProcedureStatements)
                                                conn.ExecuteNonQuery(statement);
                                        }
                                        GetWorkerState(threadIndex).Connection = null;
                                    }
                                }
                            }
                            if (!_cancelled)
                            {
                                GetWorkerState(threadIndex).Task = WorkerTask.CheckingReportParameters;
                                if (!viewOptions.GenerateDistinctDataOnly && !viewscript.ProcedureInfo.OnlyMetadata)
                                    viewscript.State = ViewscriptStates.CheckingReportParameters;
                                _log.InfoFormatWithCheck("Checking for double columns started...");
                                if (viewOptions.GenerateDistinctDataOnly || viewscript.ProcedureInfo.OnlyMetadata)
                                    CheckForDoubleColumns(threadIndex, Profile.DbConfig.DbName, viewscript);
                                else
                                    CheckForDoubleColumns(threadIndex, tmpViewDbName, viewscript);
                                _log.InfoFormatWithCheck("Checking for double columns finished.");
                            }
                            if (!_cancelled)
                            {
                                _log.InfoFormatWithCheck("Checking for existence of report parameter columns started...");
                                if (viewOptions.GenerateDistinctDataOnly || viewscript.ProcedureInfo.OnlyMetadata)
                                    CheckForParameterColumnsExists(threadIndex, Profile.DbConfig.DbName, viewscript);
                                else
                                    CheckForParameterColumnsExists(threadIndex, tmpViewDbName, viewscript);
                                _log.InfoFormatWithCheck("Checking for existence of report parameter columns finished.");
                            }
                            GetWorkerState(threadIndex).Task = WorkerTask.CheckingWhereCondition;
                            GetWorkerState(threadIndex).PreparationProgress = 0;
                            if (!viewOptions.GenerateDistinctDataOnly && !viewscript.ProcedureInfo.OnlyMetadata)
                                viewscript.State = ViewscriptStates.CheckingWhereCondition;
                            //DEVNOTE: if ReportParameterDictionary.Count > 0 then it should be validated 
                            if (viewscript.ViewInfo.ReportParameterDictionary.Count > 0)
                            {
                                if (viewscript.ViewInfo.ReportFilterLines.Count == 0)
                                    throw new InvalidScriptException(
                                        string.Format(Resource.NoFilterSpecifiedForParameterizedView,
                                                      viewscript.ViewInfo.Name));
                                //DEVNOTE: Not needed, it is already checked on a diferent way
                                //if (viewscript.ViewInfo.ReportParameterDictionary.Count > 0) {
                                //    foreach (ReportParameter parameter in viewscript.ViewInfo.ReportParameterDictionary.Values) {
                                //        string databaseName = Profile.ProjectDb.DataDbName;
                                //        CheckParameterReferencedColumn(databaseName, parameter, viewscript);
                                //    }
                                //}
                                if (!_cancelled)
                                {
                                    //runs a select with wrere clause and throw exception when fail
                                    _log.InfoFormatWithCheck("Checking where condition started...");
                                    if (viewOptions.GenerateDistinctDataOnly || viewscript.ProcedureInfo.OnlyMetadata)
                                        CheckWhereCondition(threadIndex, Profile.DbConfig.DbName, viewscript);
                                    else
                                        CheckWhereCondition(threadIndex, tmpViewDbName, viewscript);
                                    _log.InfoFormatWithCheck("Checking where condition finished.");
                                }
                            }
                            GetWorkerState(threadIndex).PreparationProgress = 1;
                            GetWorkerState(threadIndex).Task = WorkerTask.CheckingProcedureParameters;
                            GetWorkerState(threadIndex).PreparationProgress = 0;
                            if (!viewOptions.GenerateDistinctDataOnly && !viewscript.ProcedureInfo.OnlyMetadata)
                                viewscript.State = ViewscriptStates.CheckingProcedureParameters;
                            //DEVNOTE: if ParameterDictionary.Count > 0 then it should be validated 
                            if (viewscript.ViewInfo.ParameterDictionary.Count > 0)
                            {
                                _log.InfoFormatWithCheck(
                                    "Checking for existence of procedure parameter columns started...");
                                int i = 1;
                                foreach (ProcedureParameter parameter in viewscript.ViewInfo.ParameterDictionary.Values)
                                {
                                    if (!_cancelled)
                                    {
                                        GetWorkerState(threadIndex).PreparationProgress = i/
                                                                                          (double)
                                                                                          viewscript.ViewInfo.
                                                                                              ParameterDictionary.Count;
                                        // DEVNOTE: skip if both table name and column name missing, then parameter reference is not set
                                        if (string.IsNullOrWhiteSpace(parameter.TableName) &&
                                            string.IsNullOrWhiteSpace(parameter.ColumnName)) continue;
                                        string databaseName = string.IsNullOrWhiteSpace(parameter.DatabaseName)
                                                                  ? Profile.ProjectDb.DataDbName
                                                                  : parameter.DatabaseName;
                                        CheckParameterReferencedColumn(databaseName, parameter, viewscript);
                                    }
                                    i++;
                                }
                                _log.InfoFormatWithCheck(
                                    "Checking for existence of procedure parameter columns finished.");
                            }
                            GetWorkerState(threadIndex).Task = WorkerTask.CopyTable;
                            if (!viewOptions.GenerateDistinctDataOnly && !viewscript.ProcedureInfo.OnlyMetadata)
                                viewscript.State = ViewscriptStates.CopyingTable;
                            if (!_cancelled)
                            {
                                CopyView(viewOptions, threadIndex, tmpViewDbName, viewscript);
                            }
                            if (viewscript.HasStoredProcedure)
                            {
                                using (IDatabase conn = Profile.ConnectionManager.GetConnection())
                                {
                                    GetWorkerState(threadIndex).Connection = conn;
                                    //AddProcedureToSysDb(conn, viewscript);
                                    GetWorkerState(threadIndex).Connection = null;
                                }
                            }
                            //CheckForDoubleColumns(threadIndex, tmpViewDbName, viewscript);
                            // save object type
                            if (!_cancelled)
                            {
                                if (!viewOptions.GenerateDistinctDataOnly)
                                {
                                    using (IDatabase conn = Profile.ViewboxDb.ConnectionManager.GetConnection())
                                    {
                                        CreateObjectType(viewOptions, viewscript, conn, Profile);
                                    }
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            _log.Error(ex.Message, ex);
                            if (!viewOptions.GenerateDistinctDataOnly)
                            {
                                switch (viewscript.State)
                                {
                                    case ViewscriptStates.CopyingTable:
                                        viewscript.State = ViewscriptStates.CopyError;
                                        break;
                                    case ViewscriptStates.CreatingIndex:
                                        viewscript.State = ViewscriptStates.CreateIndexError;
                                        break;
                                    case ViewscriptStates.CreatingTable:
                                        viewscript.State = ViewscriptStates.CreateTableError;
                                        break;
                                    case ViewscriptStates.CheckingProcedureParameters:
                                        viewscript.State = ViewscriptStates.CheckingProcedureParametersError;
                                        break;
                                    case ViewscriptStates.CheckingReportParameters:
                                        viewscript.State = ViewscriptStates.CheckingReportParametersError;
                                        break;
                                    case ViewscriptStates.CheckingWhereCondition:
                                        viewscript.State = ViewscriptStates.CheckingWhereConditionError;
                                        break;
                                    default:
                                        viewscript.State = ViewscriptStates.CopyError;
                                        break;
                                }
                            }
                            throw;
                        }
                    }
                    // creates indexes caching for the table created by the viewscript 
                    // (the table name is expected to be the same as the name of the view)
                    if (!_cancelled)
                    {
                        if (!viewOptions.GenerateDistinctDataOnly)
                        {
                            GetWorkerState(threadIndex).Task = WorkerTask.GetIndexInfo;
                            try
                            {
                                _log.InfoFormatWithCheck("Getting index information for tables started...");
                                if (!viewOptions.GenerateDistinctDataOnly)
                                    viewscript.State = ViewscriptStates.GettingIndexInfo;
                                using (IDatabase conn = Profile.ViewboxDb.ConnectionManager.GetConnection())
                                {
                                    conn.SetHighTimeout();
                                    Profile.ViewboxDb.CreateIndexesForTable(conn, viewscript.Name);
                                }
                                _log.InfoFormatWithCheck("Getting index information for tables finished.");
                            }
                            catch (Exception)
                            {
                                viewscript.State = ViewscriptStates.GettingIndexInfoError;
                                throw;
                            }
                        }
                    }
                    viewscript.StopStopwatch();
                    if (!viewOptions.GenerateDistinctDataOnly)
                        viewscript.State = ViewscriptStates.Completed;
                    viewscript.IsChecked = false;
                    viewscript.SaveOrUpdate();
                    // DEVNOTE: when generating index data then do not update the viescripts, leave as is
                    // if the view was already inserted, then nothing to do
                    // if the view was not inserted, then no distinct table will be generated and no need to add viewscript
                    if (!viewOptions.GenerateDistinctDataOnly)
                    {
                        //Delete all views which may have been created in former times and add information about this view
                        var view = new View(viewscript, Profile.AssignedUser.Name);
                        //Delete old entries from database
                        //view.DeleteAllWithName();
                        //Delete all with the same name from the view collection
                        Profile.RemoveAllViewsWithName(view.Name);
                        //Save new
                        view.SaveOrUpdate();
                        Profile.AddView(view);
                        Profile.ViewsHistory.Add(view);
                        // For loading the newly inserted stuff
                        using (IDatabase viewboxConn = Profile.ViewboxDb.ConnectionManager.GetConnection())
                        {
                            Profile.SysDb.LoadTables(viewboxConn, true);
                        }
                    }
                }
                catch (Exception ex)
                {
                    _log.ErrorFormat(ex.Message, ex);
                    viewscript.StopStopwatch();
                    viewscript.LastError = ex.Message;
                    viewscript.SaveOrUpdate();
                    var view = new View(viewscript, Profile.AssignedUser.Name);
                    view.SaveOrUpdate();
                    Profile.ViewsHistory.Add(view);
                }
            }
        }

        internal static void CreateObjectType(ViewCreateOptions viewOptions, Viewscript viewscript, IDatabase conn,
                                              ProfileConfig profile)
        {
            Dictionary<string, string> langDescr = new Dictionary<string, string>();
            foreach (KeyValuePair<string, CultureInfo> language in Language.Languages)
            {
                string text =
                    viewscript.ViewInfo.ObjectType.LangToDescription.Where(
                        d => d.Key.ToLower() == language.Value.TwoLetterISOLanguageName.ToLower()).Select(d => d.Value).
                        FirstOrDefault();
                if (string.IsNullOrWhiteSpace(text))
                    text = Resource.ResourceManager.GetString(viewscript.ViewInfo.ObjectType.Name.ToLower(),
                                                              language.Value);
                if (string.IsNullOrWhiteSpace(text))
                    text = viewscript.ViewInfo.ObjectType.Name.ToLower();
                langDescr.Add(language.Key, text);
            }
            viewscript.ViewInfo.ObjectType.LangToDescription = langDescr;
            if (conn != null)
                profile.ViewboxDb.SaveObjectType(conn, viewscript.ViewInfo.ObjectType.Name.ToLower(), langDescr, true);
        }

        #endregion CreateView

        #region InitProcessList

        /// <summary>
        ///   Inits the process list.
        /// </summary>
        private void InitProcessList()
        {
            //DropIndizes("blackrock_jun2005", (from viewscript in Profile.Viewscripts from table in viewscript.GetTables() where viewscript.Name == "verdichteter_ertragsausgleich" select table.Key.ToLower()).Distinct().ToList());
            _views = new List<Viewscript>();
            _runningIndizes = new Dictionary<int, KeyValuePair<string, List<Index>>>();
            var indizes = new List<Index>();
            using (var db = Profile.ConnectionManager.GetConnection())
            {
                foreach (var viewscript in Profile.Viewscripts)
                    if (viewscript.IsChecked && !viewscript.ProcedureInfo.OnlyMetadata)
                        foreach (var index in viewscript.GetIndizes())
                        {
                            if (db.TableExists(index.Table))
                            {
                                var newIndex = ModifyIndex(index);
                                if (newIndex.Columns.Count > 0 && !db.IsIndexExisting(newIndex.Table, newIndex.Columns))
                                    indizes.Add(newIndex);
                            }
                        }
                indizes = indizes.Distinct().ToList();
                //indizes = (from viewscript in Profile.Viewscripts
                //           from index in viewscript.GetIndizes()
                //           where
                //               viewscript.IsChecked && db.TableExists(index.Table) &&
                //               !db.IsIndexExisting(index.Table, index.Columns) && NoIndexColumnIsEmpty(db.DbConfig.DbName, index.Table, index.Columns)
                //           select index).Distinct().ToList();
                foreach (var index in indizes)
                {
                    ITableObject tobj = Profile.ViewboxDb.Objects[db.DbConfig.DbName + "." + index.Table];
                    if (tobj != null)
                        index.Complexity = tobj.RowCount*tobj.ColumnCount;
                }
            }
            _indizes = (from index in indizes
                        orderby index.Complexity descending
                        group index by index.Table
                        into groupedIndex select groupedIndex).ToDictionary(g => g.Key, g => g.ToList());
            _views.AddRange(from viewscript in Profile.Viewscripts
                            orderby viewscript.GetTables().Sum(pair => pair.Value) descending
                            where viewscript.IsChecked
                            select viewscript);
            State = new ViewBuilderState(0, _views.Count, _indizes.Count);
        }

        #endregion InitProcessList

        #endregion methods

        /*****************************************************************************************************/

        /*****************************************************************************************************/

        /*****************************************************************************************************/

        /*****************************************************************************************************/

        /*****************************************************************************************************/
    }
}