using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading;
using SystemDb;
using SystemDb.Internal;
using AV.Log;
using DbAccess;
using DbAccess.Enums;
using DbAccess.Structures;
using log4net;
using Utils;

namespace IndexDb.IndexJob
{
	public class JobQueue
	{
		private const string CallProcedure = "call GenerateColumnInfo(@1)";

		private readonly StringBuilder _builder = new StringBuilder();

		private readonly ILog _log = IndexDb.GetInstance().Log;

		private readonly ConcurrentQueue<Column> _queue = new ConcurrentQueue<Column>();

		private readonly List<Thread> _threads = new List<Thread>();

		private int _parameterId;

		private ProgressCalculator _progress;

		private void UpgradeParameters()
		{
			using DatabaseBase conn = IndexDb.GetInstance().ViewboxDb.ConnectionManager.GetConnection();
			StringBuilder builder = new StringBuilder();
			using (NDC.Push(LogHelper.GetNamespaceContext()))
			{
				_log.Info("UpgradeParameters started");
			}
			try
			{
				List<Parameter> parameters = conn.DbMapping.Load<Parameter>();
				List<IssueExtension> estensions = conn.DbMapping.Load<IssueExtension>();
				List<Column> columns = conn.DbMapping.Load<Column>();
				List<TableObject> tables = new List<TableObject>();
				tables.AddRange(conn.DbMapping.Load<Table>("type = " + 1));
				tables.AddRange(conn.DbMapping.Load<View>("type = " + 2));
				tables.AddRange(conn.DbMapping.Load<Issue>("type = " + 3));
				tables.AddRange(conn.DbMapping.Load<FakeTable>("type = " + 6));
				IssueExtension extension2;
				foreach (Parameter parameter3 in parameters.Where((Parameter w) => string.IsNullOrEmpty(w.DatabaseName)))
				{
					bool success4 = false;
					try
					{
						using (NDC.Push(LogHelper.GetNamespaceContext()))
						{
							builder.Clear();
							builder.Append("Fill missing database name for parameter. Id: ").Append(parameter3.Id);
							_log.Info(builder.ToString());
						}
						extension2 = estensions.FirstOrDefault((IssueExtension w) => w.RefId == parameter3.IssueId);
						if (extension2 != null)
						{
							TableObject table3 = tables.FirstOrDefault((TableObject w) => w.Id == extension2.TableObjectId);
							if (table3 != null)
							{
								parameter3.DatabaseName = table3.Database;
								conn.DbMapping.Save(parameter3);
								success4 = true;
							}
						}
					}
					catch (Exception e4)
					{
						using (NDC.Push(LogHelper.GetNamespaceContext()))
						{
							_log.Error(e4);
						}
					}
					using (NDC.Push(LogHelper.GetNamespaceContext()))
					{
						builder.Clear();
						builder.Append("Fill missing database name for parameter. Id: ").Append(parameter3.Id).Append(success4 ? " - success " : " - can't specify table name ");
						_log.Info(builder.ToString());
					}
				}
				IssueExtension extension;
				foreach (Parameter parameter2 in parameters.Where((Parameter w) => string.IsNullOrEmpty(w.TableName)))
				{
					bool success3 = false;
					try
					{
						using (NDC.Push(LogHelper.GetNamespaceContext()))
						{
							builder.Clear();
							builder.Append("Fill missing table name for parameter. Id: ").Append(parameter2.Id);
							_log.Info(builder.ToString());
						}
						extension = estensions.FirstOrDefault((IssueExtension w) => w.RefId == parameter2.IssueId);
						if (extension != null)
						{
							TableObject table2 = tables.FirstOrDefault((TableObject w) => w.Id == extension.TableObjectId);
							if (table2 != null)
							{
								parameter2.TableName = table2.TableName;
								conn.DbMapping.Save(parameter2);
								success3 = true;
							}
						}
					}
					catch (Exception e3)
					{
						using (NDC.Push(LogHelper.GetNamespaceContext()))
						{
							_log.Error(e3);
						}
					}
					using (NDC.Push(LogHelper.GetNamespaceContext()))
					{
						builder.Clear();
						builder.Append("Fill missing table name for parameter. Id: ").Append(parameter2.Id).Append(success3 ? " - success " : " - can't specify table name ");
						_log.Info(builder.ToString());
					}
				}
				int maxTableOrdinal = tables.Where((TableObject w) => w.Type == TableType.Table).Max((TableObject w) => w.Ordinal) + 1;
				Dictionary<SqlType, int> maxOrdinalsByType = Enum.GetValues(typeof(SqlType)).Cast<SqlType>().ToDictionary((SqlType type) => type, (SqlType type) => columns.Any((Column w) => w.DataType == type) ? columns.Where((Column w) => w.DataType == type).Max((Column w) => w.Ordinal) : 0);
				foreach (Parameter parameter in parameters.Where((Parameter w) => !string.IsNullOrEmpty(w.DatabaseName) && !string.IsNullOrEmpty(w.TableName) && !string.IsNullOrEmpty(w.ColumnName)))
				{
					TableObject table = tables.FirstOrDefault((TableObject w) => w.TableName.ToLower() == parameter.TableName.ToLower() && w.Database.ToLower() == parameter.DatabaseName.ToLower());
					if (table == null)
					{
						bool success2 = false;
						using (NDC.Push(LogHelper.GetNamespaceContext()))
						{
							builder.Clear();
							builder.Append("Create missing table object for parameter. Id: ").Append(parameter.Id);
							_log.Info(builder.ToString());
						}
						try
						{
							builder.Clear();
							builder.Append("SELECT COUNT(").Append(conn.Enquote(parameter.ColumnName)).Append(") FROM ")
								.Append(conn.Enquote(parameter.DatabaseName, parameter.TableName));
							object rowCount = conn.ExecuteScalar(builder.ToString());
							if (rowCount != null)
							{
								table = new Table
								{
									CategoryId = 0,
									Database = parameter.DatabaseName,
									TableName = parameter.TableName,
									Type = TableType.Table,
									DefaultSchemeId = 0,
									IsVisible = false,
									RowCount = (long)rowCount,
									TransactionNumber = "2001",
									UserDefined = false,
									Ordinal = maxTableOrdinal++,
									IsArchived = false
								};
								conn.DbMapping.Save(table);
								tables.Add(table);
								success2 = true;
							}
						}
						catch (Exception e2)
						{
							using (NDC.Push(LogHelper.GetNamespaceContext()))
							{
								_log.Error(e2);
							}
						}
						using (NDC.Push(LogHelper.GetNamespaceContext()))
						{
							builder.Clear();
							builder.Append("Create missing table object for parameter. Id: ").Append(parameter.Id).Append(success2 ? " - success " : " - can't create table object ");
							_log.Info(builder.ToString());
						}
					}
					if (table == null)
					{
						continue;
					}
					Column column = columns.FirstOrDefault((Column w) => w.TableId == table.Id && string.Compare(w.Name, parameter.ColumnName, StringComparison.InvariantCultureIgnoreCase) == 0);
					if (column != null)
					{
						continue;
					}
					bool success = false;
					using (NDC.Push(LogHelper.GetNamespaceContext()))
					{
						builder.Clear();
						builder.Append("Create missing column object for parameter. Id: ").Append(parameter.Id);
						_log.Info(builder.ToString());
					}
					try
					{
						SqlType currentType = SqlType.String;
						DbColumnInfo columnInfo = conn.GetColumnInfos(parameter.DatabaseName, parameter.TableName).FirstOrDefault((DbColumnInfo w) => string.Compare(w.Name, parameter.ColumnName, StringComparison.InvariantCultureIgnoreCase) == 0);
						if (columnInfo != null)
						{
							switch (columnInfo.Type)
							{
							case DbColumnTypes.DbNumeric:
								currentType = SqlType.Decimal;
								break;
							case DbColumnTypes.DbInt:
							case DbColumnTypes.DbBigInt:
								currentType = SqlType.Integer;
								break;
							case DbColumnTypes.DbBool:
								currentType = SqlType.Boolean;
								break;
							case DbColumnTypes.DbDate:
								currentType = SqlType.Date;
								break;
							case DbColumnTypes.DbDateTime:
								currentType = SqlType.DateTime;
								break;
							case DbColumnTypes.DbText:
							case DbColumnTypes.DbLongText:
							case DbColumnTypes.DbBinary:
							case DbColumnTypes.DbUnknown:
								currentType = SqlType.String;
								break;
							case DbColumnTypes.DbTime:
								currentType = SqlType.Time;
								break;
							}
							maxOrdinalsByType[currentType]++;
							column = new Column
							{
								TableId = table.Id,
								IsVisible = false,
								DataType = currentType,
								OptimizationType = OptimizationType.NotSet,
								IsEmpty = false,
								MaxLength = columnInfo.MaxLength,
								Name = parameter.ColumnName,
								UserDefined = false,
								Ordinal = maxOrdinalsByType[currentType]
							};
							conn.DbMapping.Save(column);
							columns.Add(column);
							success = true;
						}
					}
					catch (Exception e)
					{
						using (NDC.Push(LogHelper.GetNamespaceContext()))
						{
							_log.Error(e);
						}
					}
					using (NDC.Push(LogHelper.GetNamespaceContext()))
					{
						builder.Clear();
						builder.Append("Create missing table object for parameter. Id: ").Append(parameter.Id).Append(success ? " - success " : " - can't create table object ");
						_log.Info(builder.ToString());
					}
				}
			}
			finally
			{
				using (NDC.Push(LogHelper.GetNamespaceContext()))
				{
					_log.Info("UpgradeParameters ended");
				}
			}
		}

		private bool IndexDataExists(DatabaseBase conn, Column column)
		{
			try
			{
				StringBuilder stringBuilder = new StringBuilder();
				string indexTableName = stringBuilder.Append("index_").Append(column.Id).ToString();
				stringBuilder.Clear();
				string valueTableName = stringBuilder.Append("value_").Append(column.Id).ToString();
				return conn.TableExists(indexTableName) && conn.TableExists(valueTableName);
			}
			catch (Exception e)
			{
				_log.Error(e);
				return false;
			}
		}

		private IList<Column> GetColumns(DatabaseBase conn, bool onlyForParameters = false, int columnId = 0, int parameterId = 0, bool recreateIfExists = true, DatabaseBase indexConn = null)
		{
			UpgradeParameters();
			List<Column> avaibleColumns = conn.DbMapping.Load<Column>();
			if (columnId != 0)
			{
				return avaibleColumns.Where((Column w) => w.Id == columnId && (recreateIfExists || !IndexDataExists(indexConn, w))).ToList();
			}
			if (onlyForParameters)
			{
				List<TableObject> tables = new List<TableObject>();
				tables.AddRange(conn.DbMapping.Load<Table>("type = " + 1));
				tables.AddRange(conn.DbMapping.Load<View>("type = " + 2));
				tables.AddRange(conn.DbMapping.Load<Issue>("type = " + 3));
				tables.AddRange(conn.DbMapping.Load<FakeTable>("type = " + 6));
				List<Column> columns = new List<Column>();
				foreach (Parameter parameter in (parameterId == 0) ? conn.DbMapping.Load<Parameter>().ToList() : (from w in conn.DbMapping.Load<Parameter>()
					where w.Id == parameterId
					select w).ToList())
				{
					TableObject table = tables.FirstOrDefault((TableObject w) => string.Compare(w.TableName, parameter.TableName, StringComparison.InvariantCultureIgnoreCase) == 0 && string.Compare(w.Database, parameter.DatabaseName, StringComparison.InvariantCultureIgnoreCase) == 0);
					if (table != null)
					{
						Column column = avaibleColumns.FirstOrDefault((Column w) => w.TableId == table.Id && string.Compare(w.Name, parameter.ColumnName, StringComparison.InvariantCultureIgnoreCase) == 0);
						if (column != null && !columns.Contains(column) && (recreateIfExists || !IndexDataExists(indexConn, column)))
						{
							columns.Add(column);
						}
					}
				}
				return columns.ToList();
			}
			return avaibleColumns.Where((Column w) => (w.DataType != 0 || (w.MaxLength < 128 && w.MaxLength > 0)) && (recreateIfExists || !IndexDataExists(indexConn, w))).ToList();
		}

		public void QueueJobs(ProgressCalculator progress, int columnId = 0, int parameterId = 0, bool recreateIfExists = true)
		{
			using (NDC.Push(LogHelper.GetNamespaceContext()))
			{
				_log.Info("QueueJobs started");
			}
			_parameterId = parameterId;
			IndexDb instance = IndexDb.GetInstance();
			using (DatabaseBase conn = instance.ViewboxDb.ConnectionManager.GetConnection())
			{
				using DatabaseBase indexConn = instance.IndexDbConnection.GetConnection();
				IList<Column> columns = GetColumns(conn, onlyForParameters: true, columnId, parameterId, recreateIfExists, indexConn);
				Column ou;
				while (_queue.TryDequeue(out ou))
				{
				}
				progress.SetStep(0);
				progress.Description = "Queing jobs...";
				progress.SetWorkSteps(columns.Count, hasChildren: false);
				progress.StepDone();
				foreach (Column column in columns)
				{
					_queue.Enqueue(column);
					if (GlobalSettings.ColumnCount != 0 && _queue.Count > GlobalSettings.ColumnCount)
					{
						return;
					}
					progress.StepDone();
				}
			}
			using (NDC.Push(LogHelper.GetNamespaceContext()))
			{
				_log.Info("QueueJobs ended");
			}
		}

		public void StartJobs(ProgressCalculator progress, int maxWorkThreads)
		{
			Stopwatch watch = new Stopwatch();
			watch.Start();
			_progress = progress;
			progress.SetStep(0);
			progress.Description = "Starting jobs...";
			progress.SetWorkSteps(_queue.Count, hasChildren: false);
			progress.StepDone();
			using (NDC.Push(LogHelper.GetNamespaceContext()))
			{
				_log.Info("StartJobs started");
			}
			_threads.Clear();
			for (int i = 0; i < GlobalSettings.ThreadCount; i++)
			{
				Thread thread = new Thread(DoJob)
				{
					Name = "IndexJobThread_" + i,
					IsBackground = true
				};
				_threads.Add(thread);
				thread.Start();
				Thread.Sleep(100);
			}
			using (NDC.Push(LogHelper.GetNamespaceContext()))
			{
				_log.Info("Threads started");
			}
			foreach (Thread thread2 in _threads)
			{
				while (thread2.IsAlive)
				{
					Thread.Sleep(100);
				}
			}
			using (NDC.Push(LogHelper.GetNamespaceContext()))
			{
				_log.Info("StartJobs ended. Elapsed time: " + watch.ElapsedMilliseconds + " ms");
			}
		}

		private void DoJob()
		{
			using DatabaseBase connection = IndexDb.GetInstance().IndexDbConnection.GetConnection();
			Stopwatch watch = new Stopwatch();
			Column job;
			while (_queue.TryDequeue(out job))
			{
				_builder.Clear();
				string columnInfo = _builder.Append(job.Name).Append(" (").Append(job.Id)
					.Append(")")
					.ToString();
				try
				{
					if (_progress.CancellationPending)
					{
						return;
					}
					using (NDC.Push(LogHelper.GetNamespaceContext()))
					{
						_builder.Clear();
						_log.Info(_builder.Append("DoJob started: ").Append(columnInfo).ToString());
					}
					_builder.Clear();
					string beginText = _builder.Append("(").Append(_queue.Count).Append(" remaining) ")
						.Append("Generating data for column: ")
						.Append(columnInfo)
						.ToString();
					_progress.Description = beginText;
					watch.Reset();
					watch.Start();
					connection.BeginTransaction();
					connection.ExecuteNonQuery("call GenerateColumnInfo(@1)".Replace("@1", job.Id.ToString(CultureInfo.InvariantCulture)));
					connection.CommitTransaction();
					watch.Stop();
					_builder.Clear();
					string endText = _builder.Append("Column ready: ").Append(columnInfo).Append(" - elapsed time: ")
						.Append(watch.ElapsedMilliseconds)
						.Append("ms")
						.Append(" (")
						.Append(_queue.Count)
						.Append(" remaining)")
						.ToString();
					_progress.Description = endText;
					_progress.StepDone();
					using (NDC.Push(LogHelper.GetNamespaceContext()))
					{
						_log.Info(endText);
					}
				}
				catch (Exception e)
				{
					_builder.Clear();
					_log.Info(_builder.Append("DoJob error: ").Append(columnInfo).ToString(), e);
				}
			}
		}
	}
}
