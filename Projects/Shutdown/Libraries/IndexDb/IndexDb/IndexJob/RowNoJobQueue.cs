using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using SystemDb.Internal;
using AV.Log;
using DbAccess;
using DbAccess.Enums;
using DbAccess.Structures;
using log4net;
using Utils;

namespace IndexDb.IndexJob
{
	public class RowNoJobQueue
	{
		private readonly ILog _log = IndexDb.GetInstance().Log;

		private readonly ConcurrentQueue<int> _queue = new ConcurrentQueue<int>();

		private readonly List<Thread> _threads = new List<Thread>();

		private IList<Column> _columns;

		private List<OrderArea> _orderAreas;

		private int _parameterId;

		private ProgressCalculator _progress;

		private List<TableObject> _tables;

		private bool RowNoDataExists(DatabaseBase conn, Column column)
		{
			try
			{
				string indexTableName = new StringBuilder().Append("row_no_").Append(column.Id).ToString();
				using IDataReader reader = conn.ExecuteReader("show tables like \"" + indexTableName + "%\"");
				return reader.Read();
			}
			catch (Exception e)
			{
				_log.Error(e);
				return false;
			}
		}

		private IList<Column> GetColumns(DatabaseBase conn, bool onlyForParameters = false, int columnId = 0, int parameterId = 0, bool recreateIfExists = true, DatabaseBase indexConn = null)
		{
			List<Column> avaibleColumns = conn.DbMapping.Load<Column>();
			_orderAreas = conn.DbMapping.Load<OrderArea>();
			if (columnId != 0)
			{
				return avaibleColumns.Where((Column w) => w.Id == columnId && (recreateIfExists || !RowNoDataExists(indexConn, w))).ToList();
			}
			if (onlyForParameters)
			{
				_tables = new List<TableObject>();
				_tables.AddRange(conn.DbMapping.Load<Table>("type = " + 1));
				_tables.AddRange(conn.DbMapping.Load<View>("type = " + 2));
				_tables.AddRange(conn.DbMapping.Load<Issue>("type = " + 3));
				_tables.AddRange(conn.DbMapping.Load<FakeTable>("type = " + 6));
				List<Column> columns = new List<Column>();
				foreach (Parameter parameter in (parameterId == 0) ? conn.DbMapping.Load<Parameter>().ToList() : (from w in conn.DbMapping.Load<Parameter>()
					where w.Id == parameterId
					select w).ToList())
				{
					TableObject table = _tables.FirstOrDefault((TableObject w) => string.Compare(w.TableName, parameter.TableName, StringComparison.InvariantCultureIgnoreCase) == 0 && string.Compare(w.Database, parameter.DatabaseName, StringComparison.InvariantCultureIgnoreCase) == 0);
					if (table != null)
					{
						Column column = avaibleColumns.FirstOrDefault((Column w) => w.TableId == table.Id && string.Compare(w.Name, parameter.ColumnName, StringComparison.InvariantCultureIgnoreCase) == 0);
						if (column != null && !columns.Contains(column) && (recreateIfExists || !RowNoDataExists(indexConn, column)))
						{
							columns.Add(column);
						}
					}
				}
				return columns.ToList();
			}
			return avaibleColumns.Where((Column w) => (w.DataType != 0 || (w.MaxLength < 128 && w.MaxLength > 0)) && (recreateIfExists || !RowNoDataExists(indexConn, w))).ToList();
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
				_columns = GetColumns(conn, onlyForParameters: true, columnId, parameterId, recreateIfExists, indexConn);
				List<int> tables = _columns.Select((Column w) => w.TableId).Distinct().ToList();
				progress.SetStep(0);
				progress.Description = "Queing jobs...";
				progress.SetWorkSteps(tables.Count, hasChildren: false);
				progress.StepDone();
				foreach (int table in tables)
				{
					_queue.Enqueue(table);
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
			GlobalSettings.ThreadCount = 1;
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
			connection.ExecuteNonQuery("SET @@session.net_read_timeout=360;");
			connection.ExecuteNonQuery("SET @@session.net_write_timeout=360;");
			Stopwatch watch = new Stopwatch();
			int job;
			while (_queue.TryDequeue(out job))
			{
				TableObject table = _tables.FirstOrDefault((TableObject w) => w.Id == job);
				if (table == null)
				{
					continue;
				}
				StringBuilder builder = new StringBuilder();
				string info = builder.Append(table.Database).Append(".").Append(table.TableName)
					.Append(" (")
					.Append(table.Id)
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
						builder.Clear();
						_log.Info(builder.Append("DoJob started: ").Append(info).ToString());
					}
					builder.Clear();
					string beginText = builder.Append("(").Append(_queue.Count).Append(" remaining) ")
						.Append("Generating data for table: ")
						.Append(info)
						.ToString();
					_progress.Description = beginText;
					watch.Reset();
					watch.Start();
					List<Column> columns = _columns.Where((Column w) => w.TableId == table.Id).ToList();
					List<string> columnNames = new List<string>();
					if (columns.All((Column w) => w.Name != "_ROW_NO_"))
					{
						columnNames.Add("_ROW_NO_");
					}
					columnNames.AddRange(columns.Select((Column w) => w.Name));
					try
					{
						List<string> columnsInIndexes = new List<string>();
						using (IDataReader indexReader = connection.ExecuteReader(" SELECT DISTINCT column_name  FROM INFORMATION_SCHEMA.STATISTICS  WHERE TABLE_SCHEMA = \"" + table.Database + "\"   and TABLE_NAME = \"" + table.TableName + "\"   and INDEX_NAME = \"" + table.TableName + "_row_no_index\""))
						{
							while (indexReader.Read())
							{
								columnsInIndexes.Add(indexReader.GetString(0));
							}
						}
						if (columnsInIndexes.Count != columnNames.Count || columnsInIndexes[0].ToLower() != "_row_no_")
						{
							if (columnsInIndexes.Count > 0)
							{
								connection.DropTableIndex(table.Database, table.TableName, table.TableName + "_row_no_index");
							}
							connection.CreateIndex(table.Database, table.TableName, table.TableName + "_row_no_index", columnNames.ToList());
						}
					}
					catch (Exception ex)
					{
						builder.Clear();
						_log.Info(builder.Append("DoJob error: ").Append(info).ToString(), ex);
					}
					List<OrderArea> orderAreas = _orderAreas.Where((OrderArea w) => w.TableId == table.Id).ToList();
					if (!orderAreas.Any())
					{
						orderAreas.Add(new OrderArea
						{
							Id = 0,
							Start = -1L,
							End = -1L
						});
					}
					foreach (OrderArea orderArea in orderAreas)
					{
						new List<Hashtable>();
						List<RowNoJobHelper> rowNos = new List<RowNoJobHelper>();
						int k = 0;
						string columnStrings = "";
						string joinStrings = "";
						Dictionary<Column, int> recordCounts = new Dictionary<Column, int>();
						foreach (Column column2 in columns)
						{
							string sql = ((orderArea.Id != 0) ? ("SELECT COUNT(DISTINCT VALUE_ID) FROM INDEX_" + column2.Id + " WHERE ORDER_AREAS_ID = " + orderArea.Id) : ("SELECT COUNT(DISTINCT " + connection.Enquote(column2.Name) + ") FROM" + connection.Enquote(table.Database, table.TableName)));
							object count = connection.ExecuteScalar(sql);
							if (count != null)
							{
								recordCounts.Add(column2, int.Parse(count.ToString()));
							}
						}
						columns = (from w in recordCounts
							orderby w.Value
							select w.Key).ToList();
						foreach (Column column in columns)
						{
							rowNos.Add(new RowNoJobHelper
							{
								OldValueId = -1,
								StartRowNo = 1,
								ColumnValues = new List<DbColumnValues>()
							});
							connection.DropTableIfExists("ROW_NO_" + column.Id + "_" + orderArea.Id);
							connection.CreateTable("ROW_NO_" + column.Id + "_" + orderArea.Id, new List<DbColumnInfo>
							{
								new DbColumnInfo
								{
									Name = "id",
									Type = DbColumnTypes.DbInt,
									IsPrimaryKey = true,
									AutoIncrement = true
								},
								new DbColumnInfo
								{
									Name = "from_row",
									Type = DbColumnTypes.DbInt
								},
								new DbColumnInfo
								{
									Name = "to_row",
									Type = DbColumnTypes.DbInt
								},
								new DbColumnInfo
								{
									Name = "value_id",
									Type = DbColumnTypes.DbInt
								}
							}, "MyISAM");
							if (columnStrings != "")
							{
								columnStrings += ", ";
							}
							columnStrings = columnStrings + " V" + k + ".ID ";
							joinStrings = joinStrings + " left join value_" + column.Id + " V" + k + " ON V" + k + ".VALUE = T." + connection.Enquote(column.Name) + Environment.NewLine;
							k++;
						}
						builder.Clear();
						builder.Append("SELECT ").Append(columnStrings).Append(" FROM ")
							.Append(connection.Enquote(table.Database, table.TableName))
							.Append(" T ")
							.Append(joinStrings)
							.Append((orderArea.Id == 0) ? "" : (" WHERE T._ROW_NO_ >= " + orderArea.Start + " AND T._ROW_NO_ <= " + orderArea.End))
							.Append(" ORDER BY " + columnStrings);
						List<object[]> valueList = new List<object[]>();
						int columnCount = columns.Count();
						using (IDataReader dataReader = connection.ExecuteReader(builder.ToString()))
						{
							while (dataReader.Read())
							{
								object[] record = new object[columnCount];
								dataReader.GetValues(record);
								valueList.Add(record);
							}
						}
						int rowNo = -1;
						foreach (object[] reader in valueList)
						{
							rowNo++;
							for (int j = 0; j < columns.Count; j++)
							{
								int valueId = 0;
								object value = reader[j];
								if (value != null)
								{
									int.TryParse(value.ToString(), out valueId);
								}
								RowNoJobHelper helper2 = rowNos[j];
								if (helper2.OldValueId == valueId)
								{
									continue;
								}
								if (helper2.OldValueId != -1)
								{
									DbColumnValues values2 = new DbColumnValues();
									values2["from_row"] = helper2.StartRowNo;
									values2["to_row"] = rowNo - 1;
									values2["value_id"] = helper2.OldValueId;
									helper2.ColumnValues.Add(values2);
									if (helper2.ColumnValues.Count > 1000)
									{
										connection.InsertInto("ROW_NO_" + columns[j].Id + "_" + orderArea.Id, helper2.ColumnValues);
										helper2.ColumnValues.Clear();
										foreach (DbColumnValues columnValue in helper2.ColumnValues)
										{
											columnValue.Clear();
										}
									}
								}
								helper2.StartRowNo = rowNo;
								helper2.OldValueId = valueId;
							}
						}
						valueList.Clear();
						if (rowNo <= -1)
						{
							continue;
						}
						for (int i = 0; i < columns.Count; i++)
						{
							RowNoJobHelper helper = rowNos[i];
							if (helper.OldValueId != -1)
							{
								DbColumnValues values = new DbColumnValues();
								values["from_row"] = helper.StartRowNo;
								values["to_row"] = rowNo;
								values["value_id"] = helper.OldValueId;
								helper.ColumnValues.Add(values);
							}
							connection.InsertInto("ROW_NO_" + columns[i].Id + "_" + orderArea.Id, helper.ColumnValues);
							foreach (DbColumnValues columnValue2 in helper.ColumnValues)
							{
								columnValue2.Clear();
							}
							helper.ColumnValues.Clear();
						}
					}
					watch.Stop();
					builder.Clear();
					string endText = builder.Append("Table ready: ").Append(info).Append(" - elapsed time: ")
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
					builder.Clear();
					_log.Info(builder.Append("DoJob error: ").Append(info).ToString(), e);
				}
			}
		}
	}
}
