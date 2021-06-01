using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.Common;
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
using DbAccess.Enums;
using DbAccess.Structures;
using IndexDb;
using log4net;
using Utils;
using ViewboxBusiness.Common;
using ViewboxBusiness.Exceptions;
using ViewboxBusiness.ProfileDb;
using ViewboxBusiness.ProfileDb.Tables;
using ViewboxBusiness.Resources;
using ViewboxBusiness.Structures;
using ViewboxBusiness.Structures.Config;

namespace ViewboxBusiness.ViewBuilder
{
	public class ViewBuilder : INotifyPropertyChanged
	{
		internal static ILog _log = LogHelper.GetLogger();

		private bool _cancelled;

		private Dictionary<string, List<Index>> _indizes;

		private bool _isRunning;

		private Dictionary<int, KeyValuePair<string, List<Index>>> _runningIndizes;

		private ViewBuilderState _state;

		private List<Viewscript> _views;

		private int tran_number;

		private static string _tmpViewDbNameBase;

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
					machineName = Regex.Replace(machineName, "[^a-zA-Z0-9_$]", "_");
					_tmpViewDbNameBase = "_tmp_" + machineName + "_";
				}
				return _tmpViewDbNameBase;
			}
		}

		public ViewBuilderState State
		{
			get
			{
				return _state;
			}
			set
			{
				if (_state != value)
				{
					_state = value;
					OnPropertyChanged("State");
				}
			}
		}

		public ProfileConfig Profile { get; set; }

		public bool IsRunning
		{
			get
			{
				return _isRunning;
			}
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

		public bool IsIdle
		{
			get
			{
				if (!IsRunning)
				{
					return !IsFailedToLoad;
				}
				return false;
			}
		}

		public bool IsFailedToLoad
		{
			get
			{
				return _isFailedToLoad;
			}
			set
			{
				_isFailedToLoad = value;
				OnPropertyChanged("IsFailedToLoad");
				OnPropertyChanged("IsIdle");
			}
		}

		public bool IsReady
		{
			get
			{
				if (IsIdle)
				{
					return _ready;
				}
				return false;
			}
			set
			{
				_ready = value;
				OnPropertyChanged("IsReady");
			}
		}

		public bool IsCancelled
		{
			get
			{
				if (IsRunning)
				{
					return _cancelled;
				}
				return true;
			}
		}

		public bool NotCancelled
		{
			get
			{
				if (IsRunning)
				{
					return !_cancelled;
				}
				return false;
			}
		}

		public event PropertyChangedEventHandler PropertyChanged;

		public event EventHandler Error;

		public event EventHandler Finished1;

		public event EventHandler WorkerStarted;

		public event EventHandler WorkerFinished;

		private void OnError(string message)
		{
			if (this.Error != null)
			{
				this.Error(this, new ErrorEventArgs(message));
			}
		}

		private void OnFinished()
		{
			if (this.Finished1 != null)
			{
				this.Finished1(this, null);
			}
		}

		private void OnPropertyChanged(string property)
		{
			if (this.PropertyChanged != null)
			{
				this.PropertyChanged(this, new PropertyChangedEventArgs(property));
			}
		}

		private void OnWorkerStarted(WorkerState worker)
		{
			if (this.WorkerStarted != null)
			{
				this.WorkerStarted(worker, null);
			}
		}

		private void OnWorkerFinished(WorkerState worker)
		{
			if (this.WorkerFinished != null)
			{
				this.WorkerFinished(worker, null);
			}
		}

		public void Cancel()
		{
			_cancelled = true;
			if (_workerStates != null)
			{
				WorkerState[] workerStates = _workerStates;
				foreach (WorkerState workerState in workerStates)
				{
					if (workerState != null && workerState.Connection != null)
					{
						workerState.Connection.CancelCommand();
						workerState.Connection.Close();
						workerState.Connection.Dispose();
					}
				}
			}
			OnPropertyChanged("IsCancelled");
			OnPropertyChanged("NotCancelled");
		}

		private void DropIndizes(string database, List<string> tables)
		{
			using DatabaseBase db = Profile.ViewboxDb.ConnectionManager.GetConnection();
			foreach (string table in tables)
			{
				foreach (string index in db.GetIndexNames(database, table).Distinct())
				{
					if (index.ToLower() != "primary")
					{
						db.ExecuteNonQuery($"DROP INDEX {index} ON {db.Enquote(database, table)}");
					}
				}
			}
		}

		private Index ModifyIndex(Index index)
		{
			ITableObject tobj = Profile.ViewboxDb.Objects[Profile.DbConfig.DbName + "." + index.Table];
			if (tobj == null)
			{
				string msg = string.Format("Table not exists in metadata: [{0}]", Profile.DbConfig.DbName + "." + index.Table);
				_log.Error(msg);
				throw new ArgumentException(msg);
			}
			if (!tobj.Columns.All((IColumn c) => index.Columns.Contains(c.Name.ToLower()) && !c.IsEmpty))
			{
				try
				{
					List<string> newColumns = index.Columns.Where((string column) => !tobj.Columns[column].IsEmpty).ToList();
					index = new Index(index.Table, newColumns);
				}
				catch (KeyNotFoundException ex)
				{
					throw new KeyNotFoundException($"Column not found for creating index [{index.Name}]", ex);
				}
			}
			return index;
		}

		private void CreateDistinctValuesForParameters(IIssue issue, int threadIndex)
		{
			GetWorkerState(threadIndex).PreparationProgress = 0.0;
			if (!Profile.IndexDb.IsInitialized.HasValue || !Profile.IndexDb.IsInitialized.Value)
			{
				throw new Exception("IndexDb not initialized!");
			}
			int i = 1;
			foreach (IParameter parameter in issue.Parameters)
			{
				if (_cancelled)
				{
					break;
				}
				global::IndexDb.IndexDb.DoJob(Profile.ViewboxDb, new ProgressCalculator(), 4, 0, parameter.Id);
				GetWorkerState(threadIndex).PreparationProgress = (double)i / (double)issue.Parameters.Count;
				i++;
			}
		}

		private void CheckForDoubleColumns(int threadIndex, string tmpViewDbName, Viewscript viewscript)
		{
			using DatabaseBase conn = Profile.ConnectionManager.GetConnection();
			if (!conn.TableExists(viewscript.Name))
			{
				return;
			}
			string warning = null;
			List<DbColumnInfo> columnInfos = conn.GetColumnInfos(viewscript.Name);
			if (columnInfos == null)
			{
				viewscript.LastError += " Error on CheckForDoubleColumns (check log for details)";
				return;
			}
			foreach (DbColumnInfo columnInfo in columnInfos)
			{
				if (_cancelled)
				{
					break;
				}
				if (columnInfo.OriginalType.ToLower().Contains("double"))
				{
					if (columnInfo.MaxLength == 0 || columnInfo.NumericScale == 0)
					{
						warning = string.Empty;
						if (columnInfo.MaxLength == 0)
						{
							warning = string.Format("Column {0} type of {1} has no MaxLength!", columnInfo.Name, "double");
						}
						if (columnInfo.NumericScale == 0)
						{
							if (string.IsNullOrEmpty(warning))
							{
								warning += Environment.NewLine;
							}
							warning += string.Format("Column {0} type of {1} has no NumericScale!", columnInfo.Name, "double");
						}
						viewscript.AddWarning(warning);
					}
					else
					{
						viewscript.AddWarning($"Die Spalte {columnInfo.Name} ist vom Typ double - dies sollte vermieden werden und stattdessen decimal benutzt werden.");
					}
				}
				if (!columnInfo.OriginalType.ToLower().Contains("float"))
				{
					continue;
				}
				if (columnInfo.MaxLength == 0 || columnInfo.NumericScale == 0)
				{
					warning = string.Empty;
					if (columnInfo.MaxLength == 0)
					{
						warning = string.Format("Column {0} type of {1} has no MaxLength!", columnInfo.Name, "float");
					}
					if (columnInfo.NumericScale == 0)
					{
						if (string.IsNullOrEmpty(warning))
						{
							warning += Environment.NewLine;
						}
						warning += string.Format("Column {0} type of {1} has no NumericScale!", columnInfo.Name, "float");
					}
					viewscript.AddWarning(warning);
				}
				else
				{
					viewscript.AddWarning($"Die Spalte {columnInfo.Name} ist vom Typ float - dies sollte vermieden werden und stattdessen decimal benutzt werden.");
				}
			}
		}

		private void CheckParameterReferencedColumn(string databaseName, ReportParameter parameter, Viewscript viewscript)
		{
			CheckParameterReferencedColumn(databaseName, parameter.TableName, parameter.ColumnName, parameter.ParameterName, viewscript);
		}

		private void CheckParameterReferencedColumn(string databaseName, ProcedureParameter parameter, Viewscript viewscript)
		{
			CheckParameterReferencedColumn(databaseName, parameter.TableName, parameter.ColumnName, parameter.Name, viewscript);
		}

		private void CheckParameterReferencedColumn(string databaseName, string tableName, string columnName, string paramName, Viewscript viewscript)
		{
			if (_cancelled)
			{
				return;
			}
			string sql = "SELECT {2} FROM {0}.{1} LIMIT 1;";
			sql = string.Format(sql, databaseName, tableName, columnName);
			try
			{
				using DatabaseBase conn = Profile.ConnectionManager.GetConnection();
				using (conn.ExecuteReader(sql))
				{
				}
			}
			catch (Exception ex)
			{
				viewscript.AddWarning(string.Format(Resource.ReferencedColumnNotFound, paramName, databaseName, tableName, columnName));
				throw new InvalidScriptException(string.Format(Resource.WrongReportFilterCriteria, ex.Message));
			}
		}

		private void CheckWhereCondition(int threadIndex, string tmpViewDbName, Viewscript viewscript)
		{
			string sql = "SELECT * FROM {0}.{1} WHERE {2} LIMIT 1;";
			string whereClause = string.Empty;
			foreach (string reportFilterLine in viewscript.ViewInfo.ReportFilterLines)
			{
				if (!string.IsNullOrEmpty(whereClause))
				{
					whereClause += Environment.NewLine;
				}
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
				using DatabaseBase conn = Profile.ConnectionManager.GetConnection();
				using (conn.ExecuteReader(sql))
				{
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
			using (DatabaseBase conn = Profile.ConnectionManager.GetConnection())
			{
				List<DbColumnInfo> columnInfos = new List<DbColumnInfo>();
				if (conn.TableExists(viewscript.Name))
				{
					columnInfos = conn.GetColumnInfos(viewscript.Name);
				}
				foreach (ReportParameter param in viewscript.ViewInfo.ReportParameterDictionary.Values)
				{
					if (!_cancelled && !viewscript.ViewInfo.ColumnDictionary.Any((KeyValuePair<string, Dictionary<string, string>> ci) => string.Compare(ci.Key, param.ColumnName, StringComparison.InvariantCultureIgnoreCase) == 0) && !columnInfos.Any((DbColumnInfo ci) => string.Compare(ci.Name, param.ColumnName, StringComparison.InvariantCultureIgnoreCase) == 0))
					{
						if (!string.IsNullOrEmpty(error))
						{
							error += Environment.NewLine;
						}
						error += string.Format(Resource.ParameterColumnDoesNotExists, param.ColumnName);
					}
				}
			}
			if (!string.IsNullOrEmpty(error))
			{
				throw new InvalidScriptException(error);
			}
		}

		private void viewscript_ViewIsRunningLong(object sender, EventArgs e)
		{
			((Viewscript)sender).SendEmail(Profile.DbConfig);
		}

		private WorkerState GetWorkerState(int threadIndex)
		{
			if (_workerStates != null && _workerStates.Any((WorkerState ws) => ws.ThreadIndex == threadIndex))
			{
				return _workerStates[threadIndex];
			}
			return new WorkerState(threadIndex);
		}

		private void AddProcedureToSysDb(DatabaseBase conn, Viewscript viewscript)
		{
			string dbName = conn.DbConfig.DbName + "_system";
			string createProceduresTableSql = string.Format("CREATE TABLE IF NOT EXISTS {0} (\r\n\t\t\t\tid              INT UNSIGNED auto_increment PRIMARY KEY,\r\n\t\t\t\ttable_id        INT UNSIGNED NOT NULL,\r\n\t\t\t\tname            varchar(45),\r\n\t\t\t\tdescription     varchar(255) NOT NULL,\r\n\t\t\t\tDoClientSplit   tinyint(1) NOT NULL,\r\n\t\t\t\tDoCompCodeSplit tinyint(1) NOT NULL,\r\n\t\t\t\tDoFYearSplit    tinyint(1) NOT NULL)", conn.Enquote(dbName, "procedures"));
			string createProcedureParamsSql = string.Format("CREATE TABLE IF NOT EXISTS {0} (\r\n\t\t\tid           INT UNSIGNED auto_increment PRIMARY KEY,\r\n\t\t\tprocedure_id INT UNSIGNED NOT NULL,\r\n\t\t\tordinal      INT UNSIGNED NOT NULL,\r\n\t\t\tname         varchar(45) NOT NULL,\r\n\t\t\tdescription  varchar(255) NOT NULL,\r\n\t\t\t`type`       varchar(45) NOT NULL)", conn.Enquote(dbName, "procedure_params"));
			conn.ExecuteNonQuery(createProceduresTableSql);
			conn.ExecuteNonQuery(createProcedureParamsSql);
			object procedureIdObj = conn.ExecuteScalar("SELECT id FROM " + conn.Enquote(dbName, "procedures") + " WHERE LOWER(name)=" + conn.GetSqlString(viewscript.ViewInfo.ProcedureName.ToLower()));
			object tableIdObj = conn.ExecuteScalar("SELECT table_id FROM " + conn.Enquote(dbName, "table") + " WHERE LOWER(name)=" + conn.GetSqlString(viewscript.ViewInfo.Name.ToLower()));
			if (tableIdObj == null || !int.TryParse(tableIdObj.ToString(), out var tableId))
			{
				throw new Exception(string.Format("Für die Prozedur {0} konnte die Basistabelle {1} nicht in der Tabelle {2} gefunden werden.", viewscript.ViewInfo.ProcedureName, viewscript.ViewInfo.Name, conn.Enquote(dbName, "table")));
			}
			if (procedureIdObj != null && int.TryParse(procedureIdObj.ToString(), out var procedureId))
			{
				conn.ExecuteNonQuery("DELETE FROM " + conn.Enquote(dbName, "procedure_params") + " WHERE procedure_id=" + procedureId);
				conn.ExecuteNonQuery("DELETE FROM " + conn.Enquote(dbName, "procedures") + " WHERE id=" + procedureId);
			}
			conn.ExecuteNonQuery("INSERT INTO " + conn.Enquote(dbName, "procedures") + " (table_id,`name`,description,DoClientSplit, DoCompCodeSplit, DoFYearSplit) VALUES " + string.Format("({0},{1},{2},{3},{4},{5})", tableId, conn.GetSqlString(viewscript.ViewInfo.ProcedureName), conn.GetSqlString(viewscript.ViewInfo.Description), viewscript.ViewInfo.OptimizeCriterias.DoClientSplit ? "1" : "0", viewscript.ViewInfo.OptimizeCriterias.DoCompCodeSplit ? "1" : "0", viewscript.ViewInfo.OptimizeCriterias.DoFYearSplit ? "1" : "0"));
			procedureIdObj = conn.ExecuteScalar("SELECT id FROM " + conn.Enquote(dbName, "procedures") + " WHERE LOWER(name)=" + conn.GetSqlString(viewscript.ViewInfo.ProcedureName.ToLower()));
			if (procedureIdObj == null || !int.TryParse(procedureIdObj.ToString(), out procedureId))
			{
				throw new Exception(string.Format("Die Prozedur {0} konnte nicht in die Systemdatenbank {1} eingetragen werden", viewscript.ViewInfo.ProcedureName, conn.Enquote(dbName, "table")));
			}
			int ordinal = 0;
			foreach (KeyValuePair<string, ProcedureParameter> paramPair in viewscript.ViewInfo.ParameterDictionary)
			{
				conn.ExecuteNonQuery(string.Format("INSERT INTO {0} (procedure_id, ordinal, name, description, type) VALUES ({1}, {2},{3},{4},{5})", conn.Enquote(dbName, "procedure_params"), procedureId, ordinal++, conn.GetSqlString(paramPair.Key), conn.GetSqlString(paramPair.Value.GetDescription()), conn.GetSqlString(paramPair.Value.GetSystemDbType())));
			}
		}

		private void DeleteTableEntriesIfExist(DatabaseBase viewboxConn, string viewscriptName)
		{
			tran_number = -1;
			try
			{
				string name = viewscriptName.ToLower();
				try
				{
					viewboxConn.BeginTransaction();
					if (viewboxConn.TableExists("table_parts"))
					{
						viewboxConn.ExecuteNonQuery("DELETE FROM " + viewboxConn.Enquote("table_parts") + " WHERE LOWER(" + viewboxConn.Enquote("table_name") + ") = '" + name + "'");
					}
					if (viewboxConn.TableExists("table"))
					{
						viewboxConn.ExecuteNonQuery("DELETE FROM " + viewboxConn.Enquote("table") + " WHERE LOWER(" + viewboxConn.Enquote("name") + ") = '" + name + "'");
						using DbDataReader dbDataReader = (DbDataReader)viewboxConn.ExecuteReader("SELECT table_id, name FROM " + viewboxConn.Enquote("table") + " WHERE LOWER(name) = '" + name + "'");
						if (dbDataReader.HasRows)
						{
							dbDataReader.Read();
							object tableId2 = dbDataReader[0];
							viewboxConn.ExecuteNonQuery("DELETE FROM " + viewboxConn.Enquote("col") + " WHERE " + viewboxConn.Enquote("table_id") + " = " + (int)(uint)tableId2);
						}
					}
					viewboxConn.CommitTransaction();
				}
				catch (Exception ex2)
				{
					if (viewboxConn.HasTransaction())
					{
						viewboxConn.RollbackTransaction();
					}
					_log.Error(ex2.Message, ex2);
					throw;
				}
				using DatabaseBase db = Profile.ViewboxDb.ConnectionManager.GetConnection();
				string database = Profile.ConnectionManager.DbConfig.DbName;
				using DbDataReader reader = (DbDataReader)db.ExecuteReader("SELECT id, transaction_nr FROM " + db.Enquote("tables") + " WHERE LOWER(name) = '" + name + "' AND " + db.Enquote("database") + "= '" + database + "'");
				if (!reader.HasRows)
				{
					return;
				}
				reader.Read();
				int tableId = reader.GetInt32(0);
				tran_number = reader.GetInt32(1);
				try
				{
					db.BeginTransaction();
					DeleteFromViewboxDb(db, tableId, onlyTexts: false, Profile);
					db.CommitTransaction();
				}
				catch (Exception ex)
				{
					if (db.HasTransaction())
					{
						db.RollbackTransaction();
					}
					_log.Error(ex.Message, ex);
					throw;
				}
				Profile.ViewboxDb.LoadTables(db);
			}
			catch
			{
			}
		}

		private static void DeleteFromViewboxDb(DatabaseBase conn, int tableId, bool onlyTexts, ProfileConfig profile)
		{
			try
			{
				if (!onlyTexts)
				{
					conn.ExecuteNonQuery("DELETE FROM " + conn.Enquote("tables") + " WHERE " + conn.Enquote("id") + " = " + tableId);
					conn.ExecuteNonQuery("DELETE FROM " + conn.Enquote("order_areas") + " WHERE " + conn.Enquote("table_id") + " = " + tableId);
					conn.ExecuteNonQuery("DELETE FROM " + conn.Enquote("issue_extensions") + " WHERE " + conn.Enquote("ref_id") + " = " + tableId);
					conn.ExecuteNonQuery("DELETE FROM " + conn.Enquote("user_table_settings") + " WHERE " + conn.Enquote("table_id") + " = " + tableId);
					conn.ExecuteNonQuery("DELETE FROM " + conn.Enquote("user_column_order_settings") + " WHERE " + conn.Enquote("table_id") + " = " + tableId);
					conn.ExecuteNonQuery(string.Format("DELETE FROM {0} WHERE {1} LIKE '{2},%' OR {1} LIKE '%,{2}' OR {1} LIKE '%,{2},%'", conn.Enquote("user_table_object_order_settings"), conn.Enquote("table_object_order"), tableId));
				}
				conn.ExecuteNonQuery("DELETE FROM " + conn.Enquote("table_texts") + " WHERE " + conn.Enquote("ref_id") + " = " + tableId);
				if (!onlyTexts)
				{
					string selectColumns = "SELECT id FROM " + conn.Enquote("columns") + " WHERE " + conn.Enquote("table_id") + " = " + tableId;
					conn.ExecuteNonQuery("DELETE FROM " + conn.Enquote("extended_column_information") + " WHERE " + conn.Enquote("parent") + " IN ( " + selectColumns + " ) OR " + conn.Enquote("child") + " IN ( " + selectColumns + " ) OR " + conn.Enquote("information") + " IN ( " + selectColumns + " )");
				}
				using (DbDataReader dbDataReader = (DbDataReader)conn.ExecuteReader("SELECT id FROM " + conn.Enquote("columns") + " WHERE table_id = " + tableId))
				{
					if (dbDataReader.HasRows)
					{
						List<string> columnIds = new List<string>();
						while (dbDataReader.Read())
						{
							columnIds.Add(dbDataReader[0].ToString());
						}
						foreach (string id2 in columnIds)
						{
							if (!onlyTexts)
							{
								conn.ExecuteNonQuery("DELETE FROM " + conn.Enquote("columns") + " WHERE " + conn.Enquote("id") + " = " + id2);
								conn.ExecuteNonQuery("DELETE FROM " + conn.Enquote("user_column_settings") + " WHERE " + conn.Enquote("column_id") + " = " + id2);
								conn.ExecuteNonQuery("DELETE FROM " + conn.Enquote("relations") + " WHERE " + conn.Enquote("parent") + " = " + id2 + " OR " + conn.Enquote("child") + " = " + id2);
							}
							conn.ExecuteNonQuery("DELETE FROM " + conn.Enquote("column_texts") + " WHERE " + conn.Enquote("ref_id") + " = " + id2);
							if (!onlyTexts)
							{
								try
								{
									profile.IndexDb.DeleteColumnData(Convert.ToInt32(id2));
								}
								catch (Exception ex)
								{
									_log.Warn("Operation on index db resulted in error, this is ignored intentionally because the entire process is not fail safe", ex);
								}
							}
						}
					}
				}
				using (DbDataReader dbDataReader2 = (DbDataReader)conn.ExecuteReader("SELECT id FROM " + conn.Enquote("parameter") + " WHERE issue_id = " + tableId))
				{
					if (dbDataReader2.HasRows)
					{
						List<string> parameterIds = new List<string>();
						while (dbDataReader2.Read())
						{
							parameterIds.Add(dbDataReader2[0].ToString());
						}
						foreach (string id in parameterIds)
						{
							if (!onlyTexts)
							{
								conn.ExecuteNonQuery(string.Format("DELETE FROM {0} WHERE {1} = {2}", conn.Enquote("user_issue_storedprocedure_order_settings"), conn.Enquote("parameter_id"), id));
							}
							string collectionIdSql = $"SELECT `collection_id` FROM `parameter_collections` WHERE parameter_id = {id} limit 1";
							int? collectionId = (int?)conn.ExecuteScalar(collectionIdSql);
							conn.ExecuteNonQuery("DELETE FROM " + conn.Enquote("parameter_texts") + " WHERE " + conn.Enquote("ref_id") + " = " + id);
							if (!onlyTexts)
							{
								conn.ExecuteNonQuery("DELETE FROM " + conn.Enquote("parameter") + " WHERE " + conn.Enquote("id") + " = " + id);
								conn.ExecuteNonQuery("DELETE FROM " + conn.Enquote("parameter_collections") + " WHERE " + conn.Enquote("parameter_id") + " = " + id);
							}
							if (!collectionId.HasValue)
							{
								continue;
							}
							string countSql = $"SELECT COUNT(*) FROM `parameter_collections` WHERE `collection_id` = {collectionId}";
							if ((long)conn.ExecuteScalar(countSql) == 0L)
							{
								conn.ExecuteNonQuery("DELETE FROM `collection_texts` WHERE `ref_id` = " + collectionId);
								if (!onlyTexts)
								{
									conn.ExecuteNonQuery("DELETE FROM `collections` WHERE `collection_id` = " + collectionId);
								}
							}
						}
					}
				}
				if (onlyTexts)
				{
					return;
				}
				using DbDataReader reader = (DbDataReader)conn.ExecuteReader(string.Format("SELECT ref_id FROM {0} WHERE {1}={2}", conn.Enquote("issue_extensions"), conn.Enquote("obj_id"), tableId));
				List<int> tablesToDelete = new List<int>();
				while (reader.Read())
				{
					tablesToDelete.Add(reader.GetInt32(0));
				}
				foreach (int tableToDelete in tablesToDelete)
				{
					conn.ExecuteNonQuery("DELETE FROM " + conn.Enquote("issue_extensions") + " WHERE " + conn.Enquote("obj_id") + " = " + tableId);
					DeleteFromViewboxDb(conn, tableToDelete, onlyTexts, profile);
				}
				conn.ExecuteNonQuery(string.Format("DELETE FROM {0}", conn.Enquote("user_table_object_order_settings")));
			}
			catch
			{
			}
		}

		private void CopyView(ViewCreateOptions viewOptions, int threadIndex, string tmpViewDbName, Viewscript viewscript)
		{
			using DatabaseBase conn = Profile.ConnectionManager.GetConnection();
			conn.SetHighTimeout();
			if (viewOptions.GenerateDistinctDataOnly || viewscript.ProcedureInfo.OnlyMetadata)
			{
				goto IL_03ab;
			}
			conn.SetHighTimeout();
			GetWorkerState(threadIndex).Task = WorkerTask.DropExistingMetadata;
			GetWorkerState(threadIndex).PreparationProgress = 0.0;
			GetWorkerState(threadIndex).Connection = conn;
			OptimizeCriterias optimizeCriterias = viewscript.ViewInfo.OptimizeCriterias;
			string sourceTableName = conn.Enquote(tmpViewDbName, viewscript.Name);
			string destTableName = conn.Enquote(viewscript.Name);
			conn.DropTableIfExists(viewscript.Name);
			GetWorkerState(threadIndex).PreparationProgress = 0.16666666666666666;
			if (_cancelled)
			{
				return;
			}
			conn.ExecuteNonQuery("CREATE TABLE " + destTableName + " LIKE " + sourceTableName);
			GetWorkerState(threadIndex).PreparationProgress = 0.33333333333333331;
			if (_cancelled)
			{
				return;
			}
			conn.ExecuteNonQuery("ALTER TABLE " + destTableName + " Engine = MyISAM");
			GetWorkerState(threadIndex).PreparationProgress = 0.5;
			if (_cancelled)
			{
				return;
			}
			conn.ExecuteNonQuery("ALTER TABLE " + destTableName + " ADD `_row_no_` INT UNSIGNED NOT NULL");
			GetWorkerState(threadIndex).PreparationProgress = 2.0 / 3.0;
			if (_cancelled)
			{
				return;
			}
			conn.ExecuteNonQuery("ALTER TABLE " + destTableName + " ADD PRIMARY KEY (`_row_no_`)");
			GetWorkerState(threadIndex).PreparationProgress = 5.0 / 6.0;
			if (_cancelled)
			{
				return;
			}
			conn.ExecuteNonQuery("ALTER TABLE " + destTableName + " CHANGE `_row_no_` `_row_no_` INT UNSIGNED NOT NULL AUTO_INCREMENT");
			GetWorkerState(threadIndex).PreparationProgress = 1.0;
			string fields = string.Empty;
			GetWorkerState(threadIndex).Task = WorkerTask.GetColumnInfo;
			GetWorkerState(threadIndex).PreparationProgress = 0.0;
			if (viewscript.HasStoredProcedure)
			{
				goto IL_0396;
			}
			using (IDataReader reader = conn.ExecuteReader("SELECT * FROM " + sourceTableName, CommandBehavior.SchemaOnly))
			{
				for (int i = 0; i < reader.FieldCount; i++)
				{
					string colName = conn.ReplaceSpecialChars(reader.GetName(i));
					if (fields.Length > 0)
					{
						fields += ",";
					}
					fields += conn.Enquote(colName);
				}
			}
			if (_cancelled)
			{
				return;
			}
			string sql = "INSERT INTO " + destTableName + " (" + fields + ") SELECT " + fields + " FROM " + sourceTableName;
			bool resort = false;
			string orderCols = string.Empty;
			if (optimizeCriterias.DoClientSplit)
			{
				orderCols += conn.Enquote(optimizeCriterias.ClientField);
				resort = true;
			}
			if (optimizeCriterias.DoCompCodeSplit)
			{
				if (orderCols.Length > 0)
				{
					orderCols += ", ";
				}
				orderCols += conn.Enquote(optimizeCriterias.CompCodeField);
				resort = true;
			}
			if (optimizeCriterias.DoFYearSplit)
			{
				if (orderCols.Length > 0)
				{
					orderCols += ", ";
				}
				orderCols += conn.Enquote(optimizeCriterias.GJahrField);
				resort = true;
			}
			if (resort)
			{
				sql = sql + " ORDER BY " + orderCols;
			}
			if (_cancelled)
			{
				return;
			}
			conn.ExecuteNonQuery(sql);
			GetWorkerState(threadIndex).Connection = null;
			goto IL_0396;
			IL_03ab:
			DeleteAndAddEntries(viewOptions, threadIndex, viewscript, conn);
			goto end_IL_0011;
			IL_0396:
			GetWorkerState(threadIndex).PreparationProgress = 1.0;
			goto IL_03ab;
			end_IL_0011:;
		}

		private void DeleteAndAddEntries(ViewCreateOptions viewOptions, int threadIndex, Viewscript viewscript, DatabaseBase conn)
		{
			lock (Profile.ViewboxDb)
			{
				if (viewOptions.GenerateDistinctDataOnly || viewscript.ProcedureInfo.OnlyMetadata)
				{
					goto IL_00a9;
				}
				GetWorkerState(threadIndex).Task = WorkerTask.DeletingTableEntries;
				GetWorkerState(threadIndex).PreparationProgress = 0.0;
				if (_cancelled)
				{
					return;
				}
				using (DatabaseBase viewboxConn = Profile.SysDb.ConnectionManager.GetConnection())
				{
					DeleteTableEntriesIfExist(viewboxConn, viewscript.Name);
				}
				GetWorkerState(threadIndex).PreparationProgress = 1.0;
				conn.SetHighTimeout();
				goto IL_00a9;
				IL_00a9:
				if (_cancelled)
				{
					return;
				}
				try
				{
					CreateViewboxDbEntry(viewOptions, threadIndex, conn, viewscript);
				}
				catch (Exception)
				{
					if (viewscript.State == ViewscriptStates.GeneratingDistinctValuesError)
					{
						viewscript.AddWarning(string.Format(Resource.WarningRunGenerateIndexData, Resource.GenerateIndexData));
						return;
					}
					throw;
				}
			}
		}

		private void AddToSysDb(DatabaseBase conn, Viewscript viewscript)
		{
			try
			{
				conn.BeginTransaction();
				conn.ExecuteNonQuery("INSERT IGNORE INTO " + conn.Enquote(conn.DbConfig.DbName + "_system", "system") + " (`SYSTEM_ID`, `COMPANY_ID`, `NAME`) VALUES (2, 1, 'Views')");
				conn.ExecuteNonQuery("insert into " + conn.Enquote(conn.DbConfig.DbName + "_system", "table_parts") + " VALUES ('" + viewscript.Name + "', 0, 1, '" + viewscript.Name + "')");
				string sql = "INSERT INTO " + conn.Enquote(conn.DbConfig.DbName + "_system", "table") + " (`SYSTEM_ID`, `NAME`, `COMMENT`, `MANDT_SPLIT`, `MANDT_COL`,`GJAHR_SPLIT`,`GJAHR_COL`,`BUKRS_SPLIT`,`BUKRS_COL`) VALUES (2, '" + viewscript.Name + "', '" + viewscript.Description + "', '" + (viewscript.ViewInfo.OptimizeCriterias.DoClientSplit ? 1 : 0) + "', '" + viewscript.ViewInfo.OptimizeCriterias.ClientField + "', '" + (viewscript.ViewInfo.OptimizeCriterias.DoFYearSplit ? 1 : 0) + "', '" + viewscript.ViewInfo.OptimizeCriterias.GJahrField + "', '" + (viewscript.ViewInfo.OptimizeCriterias.DoCompCodeSplit ? 1 : 0) + "', '" + viewscript.ViewInfo.OptimizeCriterias.CompCodeField + "')";
				conn.ExecuteNonQuery(sql);
				int nLastId = Convert.ToInt32(conn.ExecuteScalar("SELECT last_insert_id() FROM " + conn.Enquote(conn.DbConfig.DbName + "_system", "table")));
				List<string> insertSqls = new List<string>();
				foreach (DbColumnInfo c in conn.GetColumnInfos(conn.DbConfig.DbName, viewscript.Name))
				{
					string sColumn = c.Name.Replace(Convert.ToString('Ã') + Convert.ToString('\u0084'), "Ä").Replace(Convert.ToString('Ã') + Convert.ToString('¤'), "ä").Replace(Convert.ToString('Ã') + Convert.ToString('\u0096'), "Ö")
						.Replace(Convert.ToString('Ã') + Convert.ToString('¶'), "ö")
						.Replace(Convert.ToString('Ã') + Convert.ToString('\u009c'), "Ü")
						.Replace(Convert.ToString('Ã') + Convert.ToString('¼'), "ü")
						.Replace(Convert.ToString('Ã') + Convert.ToString('\u009f'), "ß");
					if (sColumn.Equals("_row_no_"))
					{
						continue;
					}
					string sDescription = string.Empty;
					if (viewscript.ViewInfo.ColumnDictionary.ContainsKey(sColumn))
					{
						Dictionary<string, string> langToDescr = viewscript.ViewInfo.ColumnDictionary[sColumn];
						if (langToDescr.Count > 0)
						{
							sDescription = langToDescr[viewscript.ViewInfo.Languages[0]];
						}
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
					case DbColumnTypes.DbNumeric:
					case DbColumnTypes.DbInt:
					case DbColumnTypes.DbBigInt:
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
						OnError("Nicht zugeordneter Datentyp in " + viewscript.Name + "." + sColumn + "': " + c.OriginalType);
						sType = "string";
						break;
					}
					string sSql = "INSERT INTO " + conn.Enquote(conn.DbConfig.DbName + "_system", "col") + "(table_id, name, comment, `type`) VALUES (" + nLastId + ",'" + sColumn + "'," + conn.GetSqlString(sDescription) + ",'" + sType + "')";
					insertSqls.Add(sSql);
				}
				foreach (string insertSql in insertSqls)
				{
					conn.ExecuteNonQuery(insertSql);
				}
				conn.CommitTransaction();
			}
			catch (Exception ex)
			{
				conn.RollbackTransaction();
				OnError("Fehler beim Hinzufügen der System-DB-Einträge: " + ex.Message);
			}
		}

		private void CreateViewboxDbEntry(ViewCreateOptions viewOptions, int threadIndex, DatabaseBase conn, Viewscript viewscript)
		{
			GetWorkerState(threadIndex).Task = WorkerTask.SavingIssue;
			GetWorkerState(threadIndex).PreparationProgress = 0.0;
			Dictionary<string, string> optimizeCriterias = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
			if (viewscript.ViewInfo.OptimizeCriterias.DoClientSplit)
			{
				optimizeCriterias.Add("Index", viewscript.ViewInfo.OptimizeCriterias.ClientField);
			}
			if (viewscript.ViewInfo.OptimizeCriterias.DoCompCodeSplit)
			{
				optimizeCriterias.Add("Split", viewscript.ViewInfo.OptimizeCriterias.CompCodeField);
			}
			if (viewscript.ViewInfo.OptimizeCriterias.DoFYearSplit)
			{
				optimizeCriterias.Add("Sort", viewscript.ViewInfo.OptimizeCriterias.GJahrField);
			}
			TableType type = (viewscript.HasStoredProcedure ? TableType.Issue : TableType.View);
			string viewScriptName = viewscript.Name;
			if (type == TableType.View && viewscript.ViewInfo.ReportParameterDictionary.Count > 0)
			{
				viewScriptName = viewscript.Name + "_filter";
			}
			ITableObject tobjIssue = null;
			if (_cancelled)
			{
				return;
			}
			if (!viewOptions.GenerateDistinctDataOnly)
			{
				ITableObject tobj;
				lock (Profile.ViewboxDb)
				{
					if (viewscript.ViewInfo.ObjectType != null)
					{
						using DatabaseBase conn2 = Profile.ViewboxDb.ConnectionManager.GetConnection();
						Profile.ViewboxDb.SaveObjectType(conn2, viewscript.ViewInfo.ObjectType.Name, (viewscript.ViewInfo.ObjectType == null) ? null : viewscript.ViewInfo.ObjectType.LangToDescription);
					}
					viewScriptName = viewscript.Name;
					Profile.ViewboxDb.TransactionNumber = tran_number;
					tobj = Profile.ViewboxDb.CreateAndSaveTableObject(type, conn.DbConfig.DbName, viewScriptName, conn.CountTable(viewscript.Name), conn.GetColumnInfos(viewscript.Name), optimizeCriterias, viewscript.ViewInfo.GetDescriptions(), viewscript.ViewInfo.ColumnDictionary, viewscript.ViewInfo.ProcedureName, "", null, (viewscript.ViewInfo.ObjectType == null) ? null : viewscript.ViewInfo.ObjectType.Name, viewscript.ViewInfo.OptimizeCriterias.YearRequired);
					if (type == TableType.View && viewscript.ViewInfo.ReportParameterDictionary.Count > 0)
					{
						viewScriptName = viewscript.Name + "_filter";
						string command = string.Empty;
						foreach (string reportFilterLine2 in viewscript.ViewInfo.ReportFilterLines)
						{
							if (!string.IsNullOrEmpty(command))
							{
								command += Environment.NewLine;
							}
							command += reportFilterLine2;
						}
						string rownocommand = string.Empty;
						foreach (string reportFilterLine in viewscript.ViewInfo.ReportFilterSpecialLines)
						{
							if (!string.IsNullOrEmpty(reportFilterLine))
							{
								rownocommand += reportFilterLine;
							}
						}
						tobjIssue = Profile.ViewboxDb.CreateAndSaveTableObject(TableType.Issue, conn.DbConfig.DbName, viewScriptName, -1L, new List<DbColumnInfo>(), optimizeCriterias, viewscript.ViewInfo.GetDescriptions(), viewscript.ViewInfo.ColumnDictionary, command, rownocommand, tobj, (viewscript.ViewInfo.ObjectType == null) ? null : viewscript.ViewInfo.ObjectType.Name, viewscript.ViewInfo.OptimizeCriterias.YearRequired);
						IIssue issue = tobjIssue as IIssue;
						if (issue != null)
						{
							SaveIssueData(TableType.Issue, viewscript, issue, tobj, new List<IParameter>(), issue.RowNoFilter);
						}
					}
					else
					{
						tobjIssue = tobj as IIssue;
						SaveIssueData(type, viewscript, tobj, null, new List<IParameter>(), "");
					}
					using DatabaseBase viewboxConn = Profile.ViewboxDb.ConnectionManager.GetConnection();
					Profile.SysDb.LoadTables(viewboxConn, useNewIssueMethod: true);
				}
				GetWorkerState(threadIndex).PreparationProgress = 1.0;
				GetWorkerState(threadIndex).Task = WorkerTask.CalculatingOrderArea;
				GetWorkerState(threadIndex).PreparationProgress = 0.0;
				if ((viewscript.ViewInfo.OptimizeCriterias.DoClientSplit || viewscript.ViewInfo.OptimizeCriterias.DoCompCodeSplit || viewscript.ViewInfo.OptimizeCriterias.DoFYearSplit) && tobj.RowCount > 0 && !(tobj is IIssue))
				{
					string orderCols = string.Empty;
					if (viewscript.ViewInfo.OptimizeCriterias.DoClientSplit)
					{
						orderCols += conn.Enquote(tobj.IndexTableColumn.Name);
					}
					if (viewscript.ViewInfo.OptimizeCriterias.DoCompCodeSplit)
					{
						if (orderCols.Length > 0)
						{
							orderCols += ", ";
						}
						orderCols += conn.Enquote(tobj.SplitTableColumn.Name);
					}
					if (viewscript.ViewInfo.OptimizeCriterias.DoFYearSplit)
					{
						if (orderCols.Length > 0)
						{
							orderCols += ", ";
						}
						orderCols += conn.Enquote(tobj.SortColumn.Name);
					}
					if (_cancelled)
					{
						return;
					}
					if (!viewscript.ProcedureInfo.OnlyMetadata)
					{
						using DatabaseBase db = Profile.ViewboxDb.ConnectionManager.GetConnection();
						Profile.ViewboxDb.CalculateOrderArea(tobj, orderCols, db);
					}
				}
				GetWorkerState(threadIndex).PreparationProgress = 1.0;
			}
			if (_cancelled)
			{
				return;
			}
			GetWorkerState(threadIndex).Task = WorkerTask.GeneratingDistinctValues;
			try
			{
				if (!_cancelled && ((!viewOptions.GenerateDistinctDataOnly && Profile.AutoGenerateIndex) || (viewOptions.GenerateDistinctDataOnly && ViewExists(viewScriptName, out tobjIssue, threadIndex, viewscript))))
				{
					GetWorkerState(threadIndex).Task = WorkerTask.GeneratingDistinctValues;
				}
			}
			catch (Exception)
			{
				viewscript.State = ViewscriptStates.GeneratingDistinctValuesError;
				throw;
			}
		}

		private bool OrderAreaExists(ITableObject table, int threadIndex, Viewscript viewscript)
		{
			bool retVal = true;
			if (table.OrderAreas == null || table.OrderAreas.Count == 0)
			{
				return false;
			}
			List<OrderArea> orderAreas = table.OrderAreas.Select((IOrderArea oa) => (OrderArea)oa).ToList();
			orderAreas = ((!viewscript.ViewInfo.OptimizeCriterias.DoClientSplit || table.IndexTableColumn == null || table.IndexTableColumn.IsEmpty) ? table.OrderAreas.Select((IOrderArea oa) => (OrderArea)oa).ToList() : ((!viewscript.ViewInfo.OptimizeCriterias.DoCompCodeSplit || table.SplitTableColumn == null || table.SplitTableColumn.IsEmpty) ? (from oa in table.OrderAreas
				where !string.IsNullOrWhiteSpace(oa.IndexValue)
				select (OrderArea)oa).ToList() : ((!viewscript.ViewInfo.OptimizeCriterias.DoFYearSplit || table.SortColumn == null || table.SortColumn.IsEmpty) ? (from oa in table.OrderAreas
				where !string.IsNullOrWhiteSpace(oa.IndexValue) && !string.IsNullOrWhiteSpace(oa.SplitValue)
				select (OrderArea)oa).ToList() : (from oa in table.OrderAreas
				where !string.IsNullOrWhiteSpace(oa.IndexValue) && !string.IsNullOrWhiteSpace(oa.SplitValue) && !string.IsNullOrWhiteSpace(oa.SortValue)
				select (OrderArea)oa).ToList())));
			if (orderAreas.Count > 0)
			{
				retVal = true;
			}
			return retVal;
		}

		private bool ViewExists(string viewName, out ITableObject issue, int threadIndex, Viewscript viewscript)
		{
			bool retVal = false;
			using DatabaseBase viewboxConn = Profile.ViewboxDb.ConnectionManager.GetConnection();
			GetWorkerState(threadIndex).Task = WorkerTask.Preparing;
			GetWorkerState(threadIndex).PreparationProgress = 0.0;
			try
			{
				if (Profile.SysDb.Issues == null || Profile.SysDb.Issues.Count == 0)
				{
					Profile.SysDb.LoadTables(viewboxConn, useNewIssueMethod: true);
				}
				issue = Profile.SysDb.Issues.Where((IIssue i) => string.Compare(i.TableName, viewName, StringComparison.InvariantCultureIgnoreCase) == 0).FirstOrDefault();
				retVal = issue != null;
			}
			catch (Exception)
			{
				viewscript.State = ViewscriptStates.CheckingReportParametersError;
				throw new ArgumentException($"View [{viewName}] does not exists.");
			}
			GetWorkerState(threadIndex).PreparationProgress = 1.0;
			return retVal;
		}

		private void SaveIssueData(TableType type, Viewscript viewscript, ITableObject tobj, ITableObject parentObject, List<IParameter> parameters, string rowNoFilter)
		{
			if (type != TableType.Issue)
			{
				return;
			}
			using (DatabaseBase viewboxConn = Profile.ViewboxDb.ConnectionManager.GetConnection())
			{
				if (parentObject == null)
				{
					Profile.ViewboxDb.SaveIssueExtension(viewboxConn, tobj as IIssue, parameters, null, rowNoFilter);
					Profile.ViewboxDb.ModifyIssue(viewboxConn, tobj as IIssue, null);
				}
				else
				{
					Profile.ViewboxDb.SaveIssueExtension(viewboxConn, tobj as IIssue, parameters, parentObject, rowNoFilter);
					Profile.ViewboxDb.ModifyIssue(viewboxConn, tobj as IIssue, null);
				}
			}
			SaveIssueParameters(viewscript, tobj as IIssue);
		}

		private void SaveIssueParameters(Viewscript viewscript, IIssue tobj)
		{
			List<IParameter> parameters = new List<IParameter>();
			Dictionary<string, List<string>> parameterTexts = new Dictionary<string, List<string>>();
			foreach (ILanguage lang3 in Profile.ViewboxDb.Languages)
			{
				parameterTexts[lang3.CountryCode] = new List<string> { "unused" };
			}
			if (viewscript.ViewInfo.ParameterDictionary.Values.Count > 0)
			{
				foreach (ProcedureParameter parameter2 in viewscript.ViewInfo.ParameterDictionary.Values)
				{
					Parameter param2 = tobj.Parameters.FirstOrDefault((IParameter p) => string.Compare(p.Name, parameter2.Name, StringComparison.InvariantCultureIgnoreCase) == 0) as Parameter;
					if (param2 == null)
					{
						param2 = new Parameter();
					}
					param2.ColumnName = parameter2.ColumnName;
					param2.DataType = parameter2.Type;
					param2.TypeModifier = parameter2.TypeModifier;
					if (string.IsNullOrEmpty(param2.DatabaseName))
					{
						if (!string.IsNullOrEmpty(parameter2.DatabaseName))
						{
							param2.DatabaseName = parameter2.DatabaseName;
						}
						else
						{
							param2.DatabaseName = tobj.Database;
						}
					}
					param2.Default = parameter2.DefaultValue;
					param2.TableName = parameter2.TableName;
					param2.Name = parameter2.Name;
					param2.Issue = tobj;
					param2.UserDefined = false;
					parameters.Add(param2);
					foreach (ILanguage lang2 in Profile.ViewboxDb.Languages)
					{
						parameterTexts[lang2.CountryCode].Add((!parameter2.LangToDescription.ContainsKey(lang2.CountryCode)) ? parameter2.Name : parameter2.LangToDescription[lang2.CountryCode]);
					}
				}
			}
			else if (viewscript.ViewInfo.ReportParameterDictionary.Count > 0)
			{
				foreach (ReportParameter parameter in viewscript.ViewInfo.ReportParameterDictionary.Values)
				{
					Parameter param = tobj.Parameters.FirstOrDefault((IParameter p) => string.Compare(p.Name, parameter.ParameterName, StringComparison.InvariantCultureIgnoreCase) == 0) as Parameter;
					if (param == null)
					{
						param = new Parameter();
					}
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
						parameterTexts[lang.CountryCode].Add((!parameter.ParameterDescription.ContainsKey(lang.CountryCode)) ? parameter.ParameterName : parameter.ParameterDescription[lang.CountryCode]);
					}
				}
			}
			using DatabaseBase viewboxConn = Profile.ViewboxDb.ConnectionManager.GetConnection();
			Profile.ViewboxDb.SaveIssueParameters(viewboxConn, Profile.ViewboxDb.Languages, parameterTexts, tobj, parameters);
		}

		private void CreateTmpViewDb(int threadIndex, WorkerState state)
		{
			using DatabaseBase conn = Profile.ConnectionManager.GetConnection();
			string tmpViewDbName = conn.DbConfig.DbName + TmpViewDbNameBase + threadIndex;
			conn.DropDatabaseIfExists(tmpViewDbName);
			conn.CreateDatabaseIfNotExists(tmpViewDbName);
			List<string> tableNames = new List<string>();
			lock (_views)
			{
				foreach (Dictionary<string, long> item in _views.Select((Viewscript v) => v.GetTables()))
				{
					foreach (KeyValuePair<string, long> t in item)
					{
						if (!tableNames.Contains(t.Key))
						{
							tableNames.Add(t.Key);
						}
					}
				}
			}
			CreateTempViewTable(state, conn, tmpViewDbName, tableNames, new ProgressCalculator());
		}

		public void CreateTempViewTable(WorkerState state, DatabaseBase conn, string tmpViewDbName, List<string> tableNames, ProgressCalculator progress)
		{
			int curIndex = -1;
			int maxIndex = tableNames.Count - 1;
			foreach (string tableName in tableNames)
			{
				if (_cancelled || progress.CancellationPending)
				{
					break;
				}
				progress.Description = "erstelle View für " + tableName + " ...";
				curIndex++;
				try
				{
					if (maxIndex > 0)
					{
						state.PreparationProgress = (double)curIndex / (double)maxIndex;
					}
					else
					{
						state.PreparationProgress = 0.0;
					}
					string sql = "CREATE OR REPLACE VIEW " + conn.Enquote(tmpViewDbName, ((tableName.First() == '_') ? "" : "_") + tableName) + " AS SELECT " + GetColumnNames(tableName, conn) + " FROM " + conn.Enquote(tableName);
					conn.ExecuteNonQuery(sql);
				}
				catch (Exception ex)
				{
					OnError("Fehler bei Erstellung der temporären Viewdatenbank: " + ex.Message);
				}
				progress.StepDone();
			}
		}

		private string GetColumnNames(string tableName, DatabaseBase conn)
		{
			List<string> columnNames = conn.GetColumnNames(tableName);
			ITableObject table = Profile.ViewboxDb.Objects.FirstOrDefault((ITableObject obj) => obj.TableName.ToLower() == tableName.ToLower() && obj.Database.ToLower() == Profile.DbConfig.DbName);
			StringBuilder additionalColumns = new StringBuilder();
			if (table != null)
			{
				foreach (IColumn emptyColumn in Profile.ViewboxDb.Columns.Where((IColumn c) => c.Table == table && c.IsEmpty))
				{
					if (!columnNames.Contains(emptyColumn.Name))
					{
						int number;
						string constValue = ((emptyColumn.ConstValue == null) ? "CAST(NULL AS CHAR(1))" : ((emptyColumn.ConstValue == string.Empty) ? "CAST('' AS CHAR(1))" : ((!int.TryParse(emptyColumn.ConstValue, out number)) ? conn.GetSqlString(emptyColumn.ConstValue) : number.ToString(CultureInfo.InvariantCulture))));
						additionalColumns.Append($",{constValue} AS {conn.Enquote(emptyColumn.Name)}");
					}
				}
			}
			return string.Join(",", columnNames.Select((string cName) => conn.Enquote(cName))) + additionalColumns;
		}

		public void DeleteView(ViewboxBusiness.ProfileDb.Tables.View view)
		{
			using (DatabaseBase viewboxConn = Profile.SysDb.ConnectionManager.GetConnection())
			{
				DeleteTableEntriesIfExist(viewboxConn, view.Name);
			}
			using (DatabaseBase conn = Profile.ConnectionManager.GetConnection())
			{
				conn.DropTableIfExists(view.Name);
				Viewscript viewscript = Profile.GetViewscript(view.Name);
				if (viewscript != null && viewscript.HasStoredProcedure)
				{
					conn.DropProcedureIfExists(viewscript.ViewInfo.ProcedureName);
				}
			}
			view.DeleteAllWithName();
			Profile.RemoveAllViewsWithName(view.Name);
		}

		public void UpdateMetadata(ViewboxBusiness.ProfileDb.Tables.View view)
		{
			Viewscript viewscript = Profile.GetViewscript(view.Name);
			if (viewscript == null)
			{
				throw new Exception($"Viewskript [{view.Name}] not found, cannot be actualized.");
			}
			ITableObject table = Profile.ViewboxDb.Objects[Profile.DbConfig.DbName + "." + viewscript.Name];
			if (table == null)
			{
				throw new Exception("In der Viewbox Datenbank konnte die eingespielte View nicht gefunden werden");
			}
			List<IColumn> columnsOld = (from c in Profile.ViewboxDb.Columns
				where c.Table.Id == table.Id
				orderby c.Name.ToLower()
				select c).ToList();
			List<string> columnsNew = viewscript.ViewInfo.ColumnDictionary.Keys.OrderBy((string name) => name.ToLower()).ToList();
			for (int i = 0; i < columnsOld.Count(); i++)
			{
				if (i >= columnsNew.Count)
				{
					throw new Exception($"Die Struktur des Views hat sich geändert, die Spalte {columnsOld[i].Name} wurde gelöscht, bitte View neu einspielen");
				}
				if (columnsOld[i].Name.ToLower() != columnsNew[i].ToLower())
				{
					throw new Exception($"Die Struktur des Views hat sich geändert, die Spalten {columnsOld[i].Name}/{columnsNew[i]} im alten/neuen View stimmen nicht überein, eine der Spalten wurden hinzugefügt/gelöscht, bitte View neu einspielen");
				}
			}
			if (columnsOld.Count < columnsNew.Count)
			{
				throw new Exception($"Die Struktur des Views hat sich geändert, es sind {columnsNew.Count - columnsOld.Count} neue Spalten hinzugekommen.");
			}
			if (viewscript.HasStoredProcedure)
			{
				if ((table as IIssue).UseIndexValue != viewscript.ViewInfo.OptimizeCriterias.DoClientSplit || (table as IIssue).UseSplitValue != viewscript.ViewInfo.OptimizeCriterias.DoCompCodeSplit || (table as IIssue).UseSortValue != viewscript.ViewInfo.OptimizeCriterias.DoFYearSplit)
				{
					throw new Exception("Optimierungskriterien weichen ab, bitte View neu einspielen");
				}
			}
			else
			{
				if (((table.IndexTableColumn != null) ^ viewscript.ViewInfo.OptimizeCriterias.DoClientSplit) || ((table.SplitTableColumn != null) ^ viewscript.ViewInfo.OptimizeCriterias.DoCompCodeSplit) || ((table.SortColumn != null) ^ viewscript.ViewInfo.OptimizeCriterias.DoFYearSplit))
				{
					throw new Exception("Optimierungskriterien weichen ab, bitte View neu einspielen");
				}
				if ((table.IndexTableColumn != null && table.IndexTableColumn.Name.ToLower() != viewscript.ViewInfo.OptimizeCriterias.ClientField.ToLower()) || (table.SplitTableColumn != null && table.SplitTableColumn.Name.ToLower() != viewscript.ViewInfo.OptimizeCriterias.CompCodeField.ToLower()) || (table.SortColumn != null && table.SortColumn.Name.ToLower() != viewscript.ViewInfo.OptimizeCriterias.GJahrField.ToLower()))
				{
					throw new Exception("Die Spalten bei den Optimierungskriterien weichen ab, bitte View neu einspielen");
				}
			}
			using DatabaseBase conn = Profile.ViewboxDb.ConnectionManager.GetConnection();
			try
			{
				conn.BeginTransaction();
				DeleteFromViewboxDb(conn, table.Id, onlyTexts: true, Profile);
				conn.CommitTransaction();
			}
			catch (Exception ex)
			{
				if (conn.HasTransaction())
				{
					conn.RollbackTransaction();
				}
				_log.Error(ex.Message, ex);
				throw;
			}
			Profile.ViewboxDb.LoadTables(conn);
			table = Profile.ViewboxDb.Objects[Profile.DbConfig.DbName + "." + viewscript.Name];
			if (table == null)
			{
				throw new Exception("In der Viewbox Datenbank konnte die eingespielte View nicht gefunden werden");
			}
			if (viewscript.HasStoredProcedure)
			{
				IIssue issue = table as IIssue;
				foreach (ProcedureParameter param in viewscript.ViewInfo.ParameterDictionary.Values)
				{
					Dictionary<string, List<string>> parameterTexts = new Dictionary<string, List<string>>();
					if (!issue.Parameters.Contains(param.Name))
					{
						continue;
					}
					IParameter issueParam = issue.Parameters[param.Name];
					foreach (ILanguage lang in Profile.ViewboxDb.Languages)
					{
						parameterTexts[lang.CountryCode] = new List<string> { (!param.LangToDescription.ContainsKey(lang.CountryCode)) ? param.Name : param.LangToDescription[lang.CountryCode] };
					}
					using DatabaseBase viewboxConn = Profile.ViewboxDb.ConnectionManager.GetConnection();
					global::SystemDb.SystemDb.SaveParameterTexts(viewboxConn, Profile.ViewboxDb.Languages, parameterTexts, -1, issueParam);
				}
			}
			Profile.ViewboxDb.AddTableTexts(viewscript.ViewInfo.GetDescriptions(), table, conn);
			foreach (IColumn column in table.Columns)
			{
				Profile.ViewboxDb.AddColumnTexts(viewscript.ViewInfo.ColumnDictionary, conn, column.Name, column.Id);
			}
		}

		public void UpdateAllMetadata(IEnumerable<ViewboxBusiness.ProfileDb.Tables.View> views, ProgressCalculator progress)
		{
			progress.SetWorkSteps(views.Count(), hasChildren: false);
			string errors = "";
			foreach (ViewboxBusiness.ProfileDb.Tables.View view in views)
			{
				if (progress.CancellationPending)
				{
					throw new TaskCanceledException("Metadaten Aktualisierung abgebrochen.");
				}
				try
				{
					progress.Description = "View " + view.Name;
					UpdateMetadata(view);
					Console.WriteLine("Successful: " + view.Name);
				}
				catch (Exception ex)
				{
					errors = errors + Environment.NewLine + view.Name + ": " + ex.Message;
					Console.WriteLine("error in view " + view.Name + Environment.NewLine + ex.Message);
				}
				progress.StepDone();
			}
			if (!string.IsNullOrEmpty(errors))
			{
				throw new Exception(errors);
			}
		}

		public void DeleteAndAddEntries(IEnumerable<Viewscript> viewscripts)
		{
			using DatabaseBase conn = Profile.ConnectionManager.GetConnection();
			foreach (Viewscript viewscript in viewscripts)
			{
				if (viewscript.IsChecked)
				{
					DeleteAndAddEntries(new ViewCreateOptions
					{
						GenerateDistinctDataOnly = false
					}, -1, viewscript, conn);
				}
			}
		}

		private Viewscript GetNextViewscript()
		{
			Viewscript result = null;
			lock (_views)
			{
				if (_views.Count > 0)
				{
					int i = 0;
					IEnumerable<string> tables = _views[i].GetTables().Select(delegate(KeyValuePair<string, long> t)
					{
						KeyValuePair<string, long> keyValuePair4 = t;
						return keyValuePair4.Key.ToLower();
					});
					IEnumerable<string> indexTables = _runningIndizes.Select(delegate(KeyValuePair<int, KeyValuePair<string, List<Index>>> t)
					{
						KeyValuePair<int, KeyValuePair<string, List<Index>>> keyValuePair3 = t;
						return keyValuePair3.Value.Key.ToLower();
					});
					while (tables.Any((string t) => indexTables.Contains(t)))
					{
						if (i + 1 < _views.Count)
						{
							i++;
						}
						else
						{
							i = 0;
							Thread.Sleep(10000);
						}
						tables = _views[i].GetTables().Select(delegate(KeyValuePair<string, long> t)
						{
							KeyValuePair<string, long> keyValuePair2 = t;
							return keyValuePair2.Key.ToLower();
						});
						indexTables = _runningIndizes.Select(delegate(KeyValuePair<int, KeyValuePair<string, List<Index>>> t)
						{
							KeyValuePair<int, KeyValuePair<string, List<Index>>> keyValuePair = t;
							return keyValuePair.Value.Key.ToLower();
						});
					}
					result = _views[i];
					_views.RemoveAt(i);
					return result;
				}
				return result;
			}
		}

		private KeyValuePair<string, List<Index>> GetNextIndex(int threadIndex)
		{
			KeyValuePair<string, List<Index>> result = default(KeyValuePair<string, List<Index>>);
			lock (_indizes)
			{
				if (_indizes.Count > 0)
				{
					result = _indizes.ElementAt(0);
					lock (_runningIndizes)
					{
						_runningIndizes[threadIndex] = result;
					}
					_indizes.Remove(result.Key);
					return result;
				}
				return result;
			}
		}

		private void WorkerThread(object param)
		{
			ViewCreateOptions viewOptions = null;
			try
			{
				viewOptions = ((object[])param)[0] as ViewCreateOptions;
			}
			catch (Exception)
			{
			}
			if (viewOptions == null)
			{
				throw new ArgumentException("The type of first parameter in object[] should be ViewCreateOptions!", "param");
			}
			int threadIndex = (int)((object[])param)[1];
			AutoResetEvent readyEvent = (AutoResetEvent)((object[])param)[2];
			WorkerState workerState = _workerStates[threadIndex];
			OnWorkerStarted(workerState);
			try
			{
				workerState.Task = WorkerTask.Preparing;
				if (!viewOptions.GenerateDistinctDataOnly)
				{
					if (!_cancelled)
					{
						CreateTmpViewDb(threadIndex, workerState);
					}
					workerState.Task = WorkerTask.CreatingIndex;
					KeyValuePair<string, List<Index>> indexPair = GetNextIndex(threadIndex);
					while (!_cancelled && !indexPair.Equals(default(KeyValuePair<string, List<Index>>)))
					{
						CreateIndexJob job = new CreateIndexJob
						{
							Name = indexPair.Key
						};
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
					{
						_runningIndizes.Remove(threadIndex);
					}
				}
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
				workerState.Task = WorkerTask.Cleanup;
			}
			catch (Exception ex)
			{
				OnError(ex.Message + ", stacktrace: " + ex.StackTrace);
			}
			OnWorkerFinished(workerState);
			readyEvent.Set();
		}

		public void CreateViews(object options)
		{
			try
			{
				ViewCreateOptions viewOptions = options as ViewCreateOptions;
				if (viewOptions == null)
				{
					throw new ArgumentException("The type of parameter should be ViewCreateOptions!", "options");
				}
				IsRunning = true;
				_cancelled = false;
				InitProcessList();
				int workers = Math.Min(Profile.MaxWorkerThreads.Value, _views.Count);
				if (workers > 0)
				{
					AutoResetEvent[] readyEvent = new AutoResetEvent[workers];
					_workerStates = new WorkerState[workers];
					for (int i = 0; i < workers; i++)
					{
						readyEvent[i] = new AutoResetEvent(initialState: false);
						_workerStates[i] = new WorkerState(i);
						Thread thread = new Thread(WorkerThread);
						thread.Name = "ViewBuilderWorker" + i;
						thread.Start(new object[3]
						{
							viewOptions,
							i,
							readyEvent[i]
						});
					}
					WaitHandle[] waitHandles = readyEvent;
					WaitHandle.WaitAll(waitHandles);
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
				{
					State.Dispose();
				}
			}
		}

		private void CreateIndex(KeyValuePair<string, List<Index>> indexPair)
		{
			DatabaseBase db = Profile.ConnectionManager.GetConnection();
			try
			{
				string sql = string.Format("ALTER TABLE {0} {1}", db.Enquote(indexPair.Key), string.Join(", ", indexPair.Value.Select((Index index) => "ADD INDEX " + ((index.Name.Length > 63) ? index.Name.Substring(index.Name.Length - 63) : index.Name) + "(" + string.Join(",", index.Columns.Select((string c) => db.Enquote(c))) + ")")));
				try
				{
					db.ExecuteNonQuery(sql);
				}
				catch (Exception ex)
				{
					throw new Exception("Fehler bei der Indexerstellung, wahrscheinlich existiert eine Spalte nicht. Abgesetztes SQL Statement: " + Environment.NewLine + sql + Environment.NewLine + Environment.NewLine + ex.Message);
				}
			}
			finally
			{
				if (db != null)
				{
					((IDisposable)db).Dispose();
				}
			}
		}

		private void CreateView(ViewCreateOptions viewOptions, int threadIndex, Viewscript viewscript)
		{
			if (_cancelled)
			{
				return;
			}
			using (NDC.Push(LogHelper.GetNamespaceContext()))
			{
				try
				{
					string tmpViewDbName = string.Empty;
					using (DatabaseBase databaseBase = Profile.ConnectionManager.GetConnection())
					{
						databaseBase.SetHighTimeout();
						viewscript.Sql = string.Empty;
						viewscript.LastError = string.Empty;
						viewscript.Warnings = string.Empty;
						if (!viewOptions.GenerateDistinctDataOnly)
						{
							viewscript.State = ViewscriptStates.CreatingTable;
						}
						viewscript.SaveOrUpdate();
						viewscript.ViewIsRunningLong -= viewscript_ViewIsRunningLong;
						viewscript.ViewIsRunningLong += viewscript_ViewIsRunningLong;
						viewscript.StartStopwatch();
						tmpViewDbName = databaseBase.DbConfig.DbName + TmpViewDbNameBase + threadIndex;
					}
					try
					{
						if (!viewOptions.GenerateDistinctDataOnly && !viewscript.ProcedureInfo.OnlyMetadata)
						{
							DbConfig obj = Profile.DbConfig.Clone() as DbConfig;
							obj.DbName = tmpViewDbName;
							using DatabaseBase databaseBase2 = ConnectionManager.CreateConnection(obj);
							databaseBase2.Open();
							GetWorkerState(threadIndex).Connection = databaseBase2;
							if (!string.IsNullOrEmpty(viewscript.ViewInfo.TempStatementsComplete))
							{
								foreach (string statement3 in viewscript.ViewInfo.TempStatements)
								{
									databaseBase2.ExecuteNonQuery(statement3);
								}
							}
							viewscript.SaveOrUpdate();
							foreach (string sql in viewscript.ViewInfo.Statements)
							{
								if (_cancelled)
								{
									break;
								}
								viewscript.Sql = sql;
								if (Index.CheckSql(sql))
								{
									Index index = new Index(sql);
									if (!databaseBase2.IsIndexExisting(index.Table, index.Columns))
									{
										GetWorkerState(threadIndex).Connection = null;
										databaseBase2.ExecuteNonQuery(sql);
										GetWorkerState(threadIndex).Connection = databaseBase2;
									}
								}
								else
								{
									databaseBase2.ExecuteNonQuery(sql);
								}
							}
							viewscript.Sql = string.Empty;
							GetWorkerState(threadIndex).Connection = null;
						}
					}
					catch (Exception)
					{
						viewscript.State = ViewscriptStates.CreateTableError;
						throw;
					}
					if (!viewOptions.GenerateDistinctDataOnly)
					{
						viewscript.State = ViewscriptStates.CopyingTable;
					}
					viewscript.SaveOrUpdate();
					if (!_cancelled)
					{
						try
						{
							if (!viewOptions.GenerateDistinctDataOnly && viewscript.HasStoredProcedure)
							{
								using DatabaseBase databaseBase3 = Profile.ConnectionManager.GetConnection();
								GetWorkerState(threadIndex).Connection = databaseBase3;
								if (viewscript.ProcedureInfo.ExecuteCreateTempTables)
								{
									foreach (string statement2 in viewscript.ViewInfo.ProcedureCreateTempTablesStatements)
									{
										databaseBase3.ExecuteNonQuery(statement2);
									}
								}
								if (viewscript.ProcedureInfo.ExecuteStoredProcedure)
								{
									foreach (string statement in viewscript.ViewInfo.ProcedureStatements)
									{
										databaseBase3.ExecuteNonQuery(statement);
									}
								}
								GetWorkerState(threadIndex).Connection = null;
							}
							if (!_cancelled)
							{
								GetWorkerState(threadIndex).Task = WorkerTask.CheckingReportParameters;
								if (!viewOptions.GenerateDistinctDataOnly && !viewscript.ProcedureInfo.OnlyMetadata)
								{
									viewscript.State = ViewscriptStates.CheckingReportParameters;
								}
								_log.InfoFormatWithCheck("Checking for double columns started...");
								if (viewOptions.GenerateDistinctDataOnly || viewscript.ProcedureInfo.OnlyMetadata)
								{
									CheckForDoubleColumns(threadIndex, Profile.DbConfig.DbName, viewscript);
								}
								else
								{
									CheckForDoubleColumns(threadIndex, tmpViewDbName, viewscript);
								}
								_log.InfoFormatWithCheck("Checking for double columns finished.");
							}
							if (!_cancelled)
							{
								_log.InfoFormatWithCheck("Checking for existence of report parameter columns started...");
								if (viewOptions.GenerateDistinctDataOnly || viewscript.ProcedureInfo.OnlyMetadata)
								{
									CheckForParameterColumnsExists(threadIndex, Profile.DbConfig.DbName, viewscript);
								}
								else
								{
									CheckForParameterColumnsExists(threadIndex, tmpViewDbName, viewscript);
								}
								_log.InfoFormatWithCheck("Checking for existence of report parameter columns finished.");
							}
							GetWorkerState(threadIndex).Task = WorkerTask.CheckingWhereCondition;
							GetWorkerState(threadIndex).PreparationProgress = 0.0;
							if (!viewOptions.GenerateDistinctDataOnly && !viewscript.ProcedureInfo.OnlyMetadata)
							{
								viewscript.State = ViewscriptStates.CheckingWhereCondition;
							}
							if (viewscript.ViewInfo.ReportParameterDictionary.Count > 0)
							{
								if (viewscript.ViewInfo.ReportFilterLines.Count == 0)
								{
									throw new InvalidScriptException(string.Format(Resource.NoFilterSpecifiedForParameterizedView, viewscript.ViewInfo.Name));
								}
								if (!_cancelled)
								{
									_log.InfoFormatWithCheck("Checking where condition started...");
									if (viewOptions.GenerateDistinctDataOnly || viewscript.ProcedureInfo.OnlyMetadata)
									{
										CheckWhereCondition(threadIndex, Profile.DbConfig.DbName, viewscript);
									}
									else
									{
										CheckWhereCondition(threadIndex, tmpViewDbName, viewscript);
									}
									_log.InfoFormatWithCheck("Checking where condition finished.");
								}
							}
							GetWorkerState(threadIndex).PreparationProgress = 1.0;
							GetWorkerState(threadIndex).Task = WorkerTask.CheckingProcedureParameters;
							GetWorkerState(threadIndex).PreparationProgress = 0.0;
							if (!viewOptions.GenerateDistinctDataOnly && !viewscript.ProcedureInfo.OnlyMetadata)
							{
								viewscript.State = ViewscriptStates.CheckingProcedureParameters;
							}
							if (viewscript.ViewInfo.ParameterDictionary.Count > 0)
							{
								_log.InfoFormatWithCheck("Checking for existence of procedure parameter columns started...");
								int i = 1;
								foreach (ProcedureParameter parameter in viewscript.ViewInfo.ParameterDictionary.Values)
								{
									if (!_cancelled)
									{
										GetWorkerState(threadIndex).PreparationProgress = (double)i / (double)viewscript.ViewInfo.ParameterDictionary.Count;
										if (string.IsNullOrWhiteSpace(parameter.TableName) && string.IsNullOrWhiteSpace(parameter.ColumnName))
										{
											continue;
										}
										string databaseName = (string.IsNullOrWhiteSpace(parameter.DatabaseName) ? Profile.ProjectDb.DataDbName : parameter.DatabaseName);
										CheckParameterReferencedColumn(databaseName, parameter, viewscript);
									}
									i++;
								}
								_log.InfoFormatWithCheck("Checking for existence of procedure parameter columns finished.");
							}
							GetWorkerState(threadIndex).Task = WorkerTask.CopyTable;
							if (!viewOptions.GenerateDistinctDataOnly && !viewscript.ProcedureInfo.OnlyMetadata)
							{
								viewscript.State = ViewscriptStates.CopyingTable;
							}
							if (!_cancelled)
							{
								CopyView(viewOptions, threadIndex, tmpViewDbName, viewscript);
							}
							if (viewscript.HasStoredProcedure)
							{
								using DatabaseBase connection = Profile.ConnectionManager.GetConnection();
								GetWorkerState(threadIndex).Connection = connection;
								GetWorkerState(threadIndex).Connection = null;
							}
							if (!_cancelled && !viewOptions.GenerateDistinctDataOnly)
							{
								using DatabaseBase conn2 = Profile.ViewboxDb.ConnectionManager.GetConnection();
								CreateObjectType(viewOptions, viewscript, conn2, Profile);
							}
						}
						catch (Exception ex2)
						{
							_log.Error(ex2.Message, ex2);
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
					if (!_cancelled && !viewOptions.GenerateDistinctDataOnly)
					{
						GetWorkerState(threadIndex).Task = WorkerTask.GetIndexInfo;
						try
						{
							_log.InfoFormatWithCheck("Getting index information for tables started...");
							if (!viewOptions.GenerateDistinctDataOnly)
							{
								viewscript.State = ViewscriptStates.GettingIndexInfo;
							}
							using (DatabaseBase conn = Profile.ViewboxDb.ConnectionManager.GetConnection())
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
					viewscript.StopStopwatch();
					if (!viewOptions.GenerateDistinctDataOnly)
					{
						viewscript.State = ViewscriptStates.Completed;
					}
					viewscript.IsChecked = false;
					viewscript.SaveOrUpdate();
					if (!viewOptions.GenerateDistinctDataOnly)
					{
						ViewboxBusiness.ProfileDb.Tables.View view2 = new ViewboxBusiness.ProfileDb.Tables.View(viewscript, Profile.AssignedUser.Name);
						Profile.RemoveAllViewsWithName(view2.Name);
						view2.SaveOrUpdate();
						Profile.AddView(view2);
						Profile.ViewsHistory.Add(view2);
						using DatabaseBase viewboxConn = Profile.ViewboxDb.ConnectionManager.GetConnection();
						Profile.SysDb.LoadTables(viewboxConn, useNewIssueMethod: true);
					}
				}
				catch (Exception ex)
				{
					_log.ErrorFormat(ex.Message, ex);
					viewscript.StopStopwatch();
					viewscript.LastError = ex.Message;
					viewscript.SaveOrUpdate();
					ViewboxBusiness.ProfileDb.Tables.View view = new ViewboxBusiness.ProfileDb.Tables.View(viewscript, Profile.AssignedUser.Name);
					view.SaveOrUpdate();
					Profile.ViewsHistory.Add(view);
				}
			}
		}

		public static void CreateObjectType(ViewCreateOptions viewOptions, Viewscript viewscript, DatabaseBase conn, ProfileConfig profile)
		{
			Dictionary<string, string> langDescr = new Dictionary<string, string>();
			foreach (KeyValuePair<string, CultureInfo> language in Language.Languages)
			{
				string text = (from d in viewscript.ViewInfo.ObjectType.LangToDescription
					where d.Key.ToLower() == language.Value.TwoLetterISOLanguageName.ToLower()
					select d.Value).FirstOrDefault();
				if (string.IsNullOrWhiteSpace(text))
				{
					text = Resource.ResourceManager.GetString(viewscript.ViewInfo.ObjectType.Name.ToLower(), language.Value);
				}
				if (string.IsNullOrWhiteSpace(text))
				{
					text = viewscript.ViewInfo.ObjectType.Name.ToLower();
				}
				langDescr.Add(language.Key, text);
			}
			viewscript.ViewInfo.ObjectType.LangToDescription = langDescr;
			if (conn != null)
			{
				profile.ViewboxDb.SaveObjectType(conn, viewscript.ViewInfo.ObjectType.Name.ToLower(), langDescr, overwriteDescription: true);
			}
		}

		private void InitProcessList()
		{
			_views = new List<Viewscript>();
			_runningIndizes = new Dictionary<int, KeyValuePair<string, List<Index>>>();
			List<Index> indizes = new List<Index>();
			using (DatabaseBase db = Profile.ConnectionManager.GetConnection())
			{
				foreach (Viewscript viewscript2 in Profile.Viewscripts)
				{
					if (!viewscript2.IsChecked || viewscript2.ProcedureInfo.OnlyMetadata)
					{
						continue;
					}
					foreach (Index index3 in viewscript2.GetIndizes())
					{
						if (db.TableExists(index3.Table))
						{
							Index newIndex = ModifyIndex(index3);
							if (newIndex.Columns.Count > 0 && !db.IsIndexExisting(newIndex.Table, newIndex.Columns))
							{
								indizes.Add(newIndex);
							}
						}
					}
				}
				indizes = indizes.Distinct().ToList();
				foreach (Index index2 in indizes)
				{
					ITableObject tobj = Profile.ViewboxDb.Objects[db.DbConfig.DbName + "." + index2.Table];
					if (tobj != null)
					{
						index2.Complexity = tobj.RowCount * tobj.ColumnCount;
					}
				}
			}
			_indizes = (from index in indizes
				orderby index.Complexity descending
				group index by index.Table into groupedIndex
				select (groupedIndex)).ToDictionary((IGrouping<string, Index> g) => g.Key, (IGrouping<string, Index> g) => g.ToList());
			_views.AddRange(from viewscript in Profile.Viewscripts
				orderby viewscript.GetTables().Sum((KeyValuePair<string, long> pair) => pair.Value) descending
				where viewscript.IsChecked
				select viewscript);
			State = new ViewBuilderState(0, _views.Count, _indizes.Count);
		}
	}
}
