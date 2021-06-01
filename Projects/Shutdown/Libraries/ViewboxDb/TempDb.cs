using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
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
using MySql.Data.MySqlClient;
using ViewboxDb.AggregationFunctionTranslator;
using ViewboxDb.Exceptions;
using ViewboxDb.Filters;

namespace ViewboxDb
{
	public class TempDb
	{
		public delegate void ReaderTransformation(DatabaseBase connection, string insertSql, Hashtable parameters = null);

		public const string DEFAULT_MAGIC_NO = "4713";

		private global::SystemDb.SystemDb _systemDb;

		private ConnectionManager _cmanager;

		private Dictionary<int, string> _hashTable;

		private const string COLUMN_ID = "id";

		private const string COLUMN_ROW_NO = "_row_no_";

		private readonly Dictionary<string, TableObject> _tableObjects = new Dictionary<string, TableObject>();

		private ILog _log = LogHelper.GetLogger();

		private string _connectionString;

		private string _provider;

		public string DbName { get; private set; }

		public ConnectionManager ConnectionManager => _cmanager;

		public string ConnectionString => _connectionString;

		public string DataProvider => _provider;

		public TempDb(global::SystemDb.SystemDb systemDb, string tempdbName)
		{
			_systemDb = systemDb;
			DbName = tempdbName;
		}

		public void Connect()
		{
			DbConfig cfg = new DbConfig
			{
				ConnectionString = _systemDb.ConnectionString,
				DbName = string.Empty
			};
			using (DatabaseBase databaseBase = ConnectionManager.CreateConnection(cfg))
			{
				databaseBase.Open();
				databaseBase.CreateDatabaseIfNotExists(DbName);
			}
			cfg.DbName = DbName;
			_connectionString = cfg.ConnectionString;
			_cmanager = new ConnectionManager(cfg, 50);
			if (!_cmanager.IsInitialized)
			{
				_cmanager.Init();
			}
			_hashTable = new Dictionary<int, string>();
			using DatabaseBase db = _cmanager.GetConnection();
			db.DbMapping.CreateTableIfNotExists<HashTable>();
			foreach (HashTable hash in db.DbMapping.Load<HashTable>())
			{
				db.DbMapping.SetTableName<Temp>(hash.TableName);
				db.DbMapping.DropTableIfExists<Temp>();
				db.DbMapping.Delete(hash);
			}
			List<string> opts = new List<string>();
			lock (_systemDb.Optimizations)
			{
				opts = (from o in _systemDb.Optimizations
					where o.Group.Type == OptimizationType.System
					select o.Value).ToList();
			}
			foreach (string s in opts)
			{
				foreach (IUser u in _systemDb.Users)
				{
					db.DropDatabaseIfExists(u.UserTable(s));
				}
			}
		}

		public static string GetOptimizationValue(IOptimization opt, OptimizationType type)
		{
			if (opt.Id == 0)
			{
				return null;
			}
			if (opt.Group.Type == type)
			{
				return opt.Value;
			}
			return GetOptimizationValue(opt.Parent, type);
		}

		public static bool HasOptGroup(IOptimization opt, OptimizationType type)
		{
			if (opt.Id == 0)
			{
				return false;
			}
			if (opt.Group.Type == type)
			{
				return true;
			}
			return HasOptGroup(opt.Parent, type);
		}

		public void RemoveTempTable(string key)
		{
			if (_hashTable.ContainsValue(key))
			{
				_hashTable.Remove(_hashTable.First((KeyValuePair<int, string> kv) => kv.Value == key).Key);
			}
			using (DatabaseBase db = _cmanager.GetConnection())
			{
				string filter = $"tablename = '{key}'";
				foreach (HashTable hash in db.DbMapping.Load<HashTable>(filter))
				{
					db.DbMapping.SetTableName<Temp>(hash.TableName);
					db.DbMapping.DropTableIfExists<Temp>();
					db.DbMapping.Delete(hash);
				}
			}
			if (_tableObjects.ContainsKey(key))
			{
				_tableObjects[key].Remove();
				_tableObjects.Remove(key);
			}
		}

		public TableObject CreateSortedAndFilteredTable(ReaderTransformation func, ITableObject table, IOptimization opt, CancellationToken token, SortCollection sortList, IFilter filter, object[] paramValues, object[] displayValues, List<int> itemId, List<int> selectionTypes, string filterIssue, string joinKey, bool joinSort, bool joinFilter, bool joinIssue, ITableObject original, IUser user, bool isNewGroupByOrJoinTable, bool onlyCount = false, string preloadText = null, long tempTableSize = 0L, List<string> summedColumns = null, bool originalColumnIds = true, SubTotalParameters groupSubTotal = null, bool multiOptimization = false, IDictionary<int, Tuple<int, int>> optimizationSelected = null)
		{
			DatabaseBase connection = _cmanager.GetConnection();
			try
			{
				string Key = "";
				TableObject to = null;
				IIssue issue = original as IIssue;
				if (paramValues != null && issue.FilterTableObject != null && !joinIssue)
				{
					if (summedColumns == null)
					{
						table = issue.FilterTableObject;
					}
					foreach (IColumn col in table.Columns)
					{
						IUserColumnSettings usc = _systemDb.UserColumnSettings[user, col];
						if (usc != null)
						{
							col.IsVisible = usc.IsVisible;
						}
						else if (original.Columns.Contains(col.Id))
						{
							col.IsVisible = original.Columns[col.Id].IsVisible;
							col.IsTempedHidden = original.Columns[col.Id].IsTempedHidden;
						}
					}
				}
				token.ThrowIfCancellationRequested();
				Key = BitConverter.ToString(Guid.NewGuid().ToByteArray()).Replace("-", string.Empty);
				to = new TableObject();
				if (!_tableObjects.ContainsKey(Key))
				{
					_tableObjects.Add(Key, to);
				}
				to.Filter = filter?.Clone();
				to.ParamValues = paramValues;
				to.DisplayValues = ((displayValues != null) ? displayValues.Select((object e) => e.ToString()).ToList() : new List<string>());
				to.ItemId = itemId;
				to.SelectionTypes = selectionTypes;
				to.Table = table;
				to.PreviousTableId = table.Id;
				to.Optimization = opt;
				to.OriginalColumns = table.Columns;
				to.OriginalTable = original;
				to.BaseTableId = table.Id;
				to.GroupSubTotal = groupSubTotal;
				to.MultiOptimization = multiOptimization;
				to.OptimizationSelected = optimizationSelected;
				to.IsNewGroupByOrJoinTable = isNewGroupByOrJoinTable;
				if (paramValues != null && !string.IsNullOrWhiteSpace(filterIssue))
				{
					to.FilterIssue = filterIssue;
				}
				Hashtable parameters = new Hashtable();
				string filterString = "";
				ITableObject tobj = _systemDb.CreateTemporaryTableObject(to.Table, to.Table.Columns, originalColumnIds && filter == null && paramValues != null && paramValues.All((object x) => x?.Equals("") ?? true) && !joinFilter);
				foreach (IColumn column in tobj.Columns)
				{
					if (column.IsTempedHidden)
					{
						column.IsVisible = true;
					}
				}
				if (summedColumns != null && summedColumns.Count != 0)
				{
					to.Sum = tobj.Columns.Where((IColumn x) => summedColumns.Contains(x.Name)).ToList();
					foreach (IColumn column2 in tobj.Columns)
					{
						if (to.Sum.All((IColumn y) => y.Name != column2.Name))
						{
							if (column2.IsVisible)
							{
								column2.IsTempedHidden = true;
							}
							column2.IsVisible = false;
						}
					}
				}
				string tempFilterIssue = filterIssue;
				if (parameters != null && filterIssue != null)
				{
					tempFilterIssue = parameters.Keys.Cast<object>().Aggregate(tempFilterIssue, (string current, object key) => current.Replace(key.ToString(), $"'{parameters[key]}'"));
				}
				string psFilter = to.GetFilterString(connection, opt, filter, tempFilterIssue, joinFilter, joinIssue, user, !string.IsNullOrEmpty(filterString)) ?? "true";
				string tname = table.TableName;
				IIssue originalIssue = original as IIssue;
				if (originalIssue != null && originalIssue.IssueType == IssueType.Filter)
				{
					tname = originalIssue.FilterTableObject.TableName;
				}
				string database = ((ViewboxDb.UseNewIssueMethod && to.OriginalTable is IIssue && (to.OriginalTable as IIssue).IssueType == IssueType.StoredProcedure) ? user.UserTable(table.Database) : table.Database);
				string indexValue = opt.FindValue(OptimizationType.IndexTable);
				string splitValue = opt.FindValue(OptimizationType.SplitTable);
				string sortValue = opt.FindValue(OptimizationType.SortColumn);
				List<string> areas = new List<string>(from o in table.OrderAreas.GetRanges(user.CurrentLanguage, indexValue, splitValue, sortValue)
					select string.Format("{0} BETWEEN {1} AND {2}", connection.Enquote("_row_no_"), o.Item1, o.Item2));
				areas.AddRange(from o in table.OrderAreas.GetRanges(user.CurrentLanguage, indexValue, splitValue, sortValue)
					select string.Format("{0} BETWEEN {1} AND {2}", connection.Enquote("_row_no_"), o.Item1, o.Item2));
				if (multiOptimization)
				{
					areas.Clear();
				}
				string optFilter = ((areas.Count > 0) ? string.Format("({0})", string.Join(" OR ", areas)) : string.Format("({0} IS NOT NULL)", connection.Enquote("_row_no_")));
				StringBuilder extendedFilter = new StringBuilder(psFilter);
				if (!string.IsNullOrWhiteSpace(psFilter))
				{
					extendedFilter.Append(" AND ");
				}
				string roleBasedSQL = original.GetRoleBasedFilterSQL(user);
				if (roleBasedSQL != string.Empty)
				{
					extendedFilter.Append($"{roleBasedSQL} AND ");
				}
				extendedFilter.Append(optFilter);
				string database2 = (to.Table.UserDefined ? connection.DbConfig.DbName : database);
				string checkTableExistSql = $"select count(1) from `information_schema`.`TABLES` t where t.`table_schema` = '{database2}' and t.`TABLE_NAME` = '{table.TableName}'";
				int result = 2;
				DatabaseBase Conn = ConnectionManager.GetConnection();
				if (Conn != null && int.TryParse(Conn.ExecuteScalar(checkTableExistSql).ToString(), out var actValue))
				{
					result = actValue;
				}
				if (result < 1)
				{
					database2 = connection.DbConfig.DbName;
				}
				PageSystem pageSystem = new PageSystem(string.Format("SELECT `{0}` FROM `{1}`.`{2}` AS t WHERE {3}", "_row_no_", database2, tname, extendedFilter), opt?.Id ?? (-1), connection.DbConfig.DbName)
				{
					Filter = psFilter,
					OrderBy = to.GetSortString(connection, sortList, joinSort),
					CountStep = ((original.RowCount > 50000) ? original.RowCount : 50000)
				};
				if (summedColumns != null && summedColumns.Count != 0)
				{
					pageSystem.Count = 1L;
					pageSystem.IsEnded = true;
				}
				else
				{
					try
					{
						if (areas.Count == 0)
						{
							pageSystem.CountTable(connection, to.Table);
						}
						else
						{
							IEnumerable<Tuple<long, long>> orderAreas = table.OrderAreas.GetRanges(user.CurrentLanguage, indexValue, splitValue, sortValue);
							if (orderAreas.Max((Tuple<long, long> o) => o.Item2) == orderAreas.Min((Tuple<long, long> o) => o.Item1))
							{
								pageSystem.Count = 1L;
							}
							else
							{
								pageSystem.Count = orderAreas.Max((Tuple<long, long> o) => o.Item2) - orderAreas.Min((Tuple<long, long> o) => o.Item1);
							}
							pageSystem.IsEnded = true;
						}
					}
					catch (Exception)
					{
						pageSystem.Count = table.RowCount;
						pageSystem.IsEnded = true;
					}
				}
				_systemDb.SetPageSystem(tobj, pageSystem);
				long rowCount = pageSystem.Count;
				if (sortList != null)
				{
					foreach (Sort item in sortList)
					{
						IColumn s = to.Table.Columns.FirstOrDefault((IColumn c) => c.Id == item.cid);
						if (s == null)
						{
							s = table.Columns.FirstOrDefault((IColumn c) => c.Id == item.cid);
							item.cid = ((s != null) ? tobj.Columns[s.Name].Id : item.cid);
						}
						else
						{
							item.cid = ((s != null) ? to.Table.Columns[s.Name].Id : item.cid);
						}
					}
				}
				to.Sort = sortList;
				to.Table = tobj;
				to.Key = Key;
				_systemDb.SetRowCount(to.Table, rowCount);
				return to;
			}
			finally
			{
				if (connection != null)
				{
					((IDisposable)connection).Dispose();
				}
			}
		}

		public string SubAndFullTotal(TableObject table, string groupColumnName, string subTotalColumnName, IOptimization opt, IUser user, bool AllSubTotal = false, string groupNameValue = null)
		{
			if (table.Table.Columns[groupColumnName] != null && (table.Table.Columns[groupColumnName].DataType == SqlType.Date || table.Table.Columns[groupColumnName].DataType == SqlType.Time || table.Table.Columns[groupColumnName].DataType == SqlType.DateTime) && DateTime.TryParse(groupNameValue, out var toDateTime))
			{
				groupNameValue = toDateTime.ToString("yyyyMMddHHmmss");
			}
			using (DatabaseBase db = _cmanager.GetConnection())
			{
				string sql = (AllSubTotal ? new TableObject
				{
					Table = table.Table,
					MultiOptimization = table.MultiOptimization,
					OptimizationSelected = table.OptimizationSelected,
					OptimizationCollection = table.OptimizationCollection
				}.GetSelectionStringSubTotal(db, opt, user, null, subTotalColumnName, null, (table.Table.PageSystem != null) ? table.Table.PageSystem.Filter : null) : new TableObject
				{
					Table = table.Table,
					MultiOptimization = table.MultiOptimization,
					OptimizationSelected = table.OptimizationSelected,
					OptimizationCollection = table.OptimizationCollection
				}.GetSelectionStringSubTotal(db, opt, user, groupColumnName, subTotalColumnName, groupNameValue ?? "", (table.Table.PageSystem != null) ? table.Table.PageSystem.Filter : null));
				try
				{
					using IDataReader reader = db.ExecuteReader(sql);
					if (reader.Read())
					{
						return reader.GetString(0);
					}
				}
				catch
				{
					_log.ErrorFormat("Opening the table is failed: Table-id: {0}; Script: {1}", table.Table.Id, sql);
					return "0";
				}
			}
			return "0";
		}

		private long DoExecuteScalar(DatabaseBase connection, string sql, Hashtable parameters)
		{
			if (parameters == null || parameters.Count == 0)
			{
				return (long)connection.ExecuteScalar(sql);
			}
			using IDbCommand cmd = connection.GetDbCommand();
			cmd.Connection = connection.Connection;
			cmd.CommandText = sql;
			foreach (object paramName in parameters.Keys)
			{
				IDbDataParameter pValue = cmd.CreateParameter();
				pValue.ParameterName = paramName.ToString();
				pValue.Value = parameters[paramName];
				if (pValue.Value != null && pValue.Value.ToString().StartsWith("0x"))
				{
					pValue.DbType = DbType.Binary;
					pValue.Value = Filter.StringToByteArrayFastest(pValue.Value.ToString());
					pValue.DbType = DbType.Binary;
				}
				cmd.Parameters.Add(pValue);
			}
			return (long)cmd.ExecuteScalar();
		}

		public DataTable LoadDataTable(ITableObject table, IOptimization opt, long start, int size, IUser user, string roleBasedOptimizationFilter = null)
		{
			using DatabaseBase db = _cmanager.GetConnection();
			string sql = new TableObject
			{
				Table = table
			}.GetSelectionString(db, opt, start, size, user, selectRowNo: true, getAllFields: false, null, roleBasedOptimizationFilter);
			try
			{
				return db.GetDataTable(sql);
			}
			catch (Exception ex)
			{
				_log.ErrorFormat("Opening the table is failed: Table-id: {0}; Script: {1}", table.Id, sql);
				throw ex;
			}
		}

		public DataTable LoadDataTableMin(ITableObject table, string database)
		{
			using DatabaseBase db = _cmanager.GetConnection();
			string sql = new TableObject
			{
				Table = table
			}.GetSelectionString(db, database);
			return db.GetDataTable(sql);
		}

		public DataTable LoadDataTableMin(string table, string database)
		{
			using DatabaseBase db = _cmanager.GetConnection();
			if (!db.TableExists(database, table))
			{
				throw new TableNotExistsException();
			}
			string sql = $"SELECT * FROM {db.Enquote(database, table)}";
			return db.GetDataTable(sql);
		}

		public string LoadExportData(DatabaseBase connection, ITableObject table, IOptimization opt, int page, long size, IUser user, bool getAllFields, string filter = "", string roleBasedOptFilter = "", IEnumerable<IOptimization> optimization = null)
		{
			return LoadExportData(connection, new TableObject
			{
				Table = table
			}, opt, page, size, user, getAllFields, null, filter, roleBasedOptFilter, optimization);
		}

		public string LoadExportData(DatabaseBase connection, TableObject table, IOptimization opt, int page, long size, IUser user, bool getAllFields, PageSystem pageSystem = null, string filter = "", string roleBasedOptFilter = "", IEnumerable<IOptimization> optimization = null)
		{
			if (filter != string.Empty)
			{
				string tmpFilter = string.Empty;
				if (filter.Contains("AND"))
				{
					string[] array = filter.Replace("(", "").Replace(")", "").Split(new string[1] { "AND" }, StringSplitOptions.None);
					foreach (string item in array)
					{
						string[] filterSplitted2 = item.Split('`');
						if (tmpFilter.Length > 0)
						{
							tmpFilter += " AND ";
						}
						IColumn col2 = table.Table.Columns.SingleOrDefault((IColumn c) => c.Id.ToString() == filterSplitted2[1]);
						tmpFilter += $"{filterSplitted2[0]} `{col2.Name}` {filterSplitted2[2]}";
					}
					filter = "(" + tmpFilter + ")";
				}
				else
				{
					string[] filterSplitted = filter.Split('`');
					try
					{
						IColumn col = table.Table.Columns.SingleOrDefault((IColumn c) => c.Id.ToString() == filterSplitted[1]);
						filter = ((col == null || filterSplitted.Count() != 3) ? string.Empty : $"{filterSplitted[0]} `{col.Name}` {filterSplitted[2]}");
					}
					catch (Exception)
					{
						_log.ErrorFormat("Filter parsing failed for: {0}", filter);
						filter = string.Empty;
					}
				}
			}
			table.FilterIssue = ((table.FilterIssue == string.Empty || table.FilterIssue == null) ? filter : table.FilterIssue);
			return table.GetSelectionString(connection, opt, page * size, size, user, selectRowNo: false, getAllFields, pageSystem, roleBasedOptFilter);
		}

		public TableObject CreateGroupByTable(ReaderTransformation func, ITableObject table, IOptimization opt, List<int> colIds, AggregationCollection aggs, IFilter filter, Dictionary<string, string> descriptions, IEnumerable<ILanguage> languages, CancellationToken token, bool save, Dictionary<Tuple<ILanguage, string>, string> aggDescriptions, IUser user, string filterIssue = "")
		{
			using DatabaseBase db = _cmanager.GetConnection();
			string Key = "";
			List<int> list = ((colIds == null) ? new List<int>() : new List<int>(colIds));
			list.Sort();
			string cols = string.Join(", ", list.Select((int c) => c));
			int hashCode = aggs?.GetHashCode() ?? 0;
			hashCode += cols.GetHashCode();
			hashCode += opt?.Id.GetHashCode() ?? 0;
			hashCode += table.Name.GetHashCode();
			token.ThrowIfCancellationRequested();
			TableObject to;
			if (!save && _hashTable.ContainsKey(hashCode))
			{
				Key = _hashTable[hashCode];
				to = _tableObjects[Key];
			}
			else
			{
				Key = BitConverter.ToString(Guid.NewGuid().ToByteArray()).Replace("-", string.Empty);
				to = new TableObject();
				string tableName = Key;
				if (!save)
				{
					HashTable h = new HashTable
					{
						HashCode = hashCode,
						TableName = Key
					};
					db.DbMapping.Save(h);
					_hashTable.Add(hashCode, Key);
					_tableObjects.Add(Key, to);
				}
				to.Agg = aggs;
				to.ColIds = colIds;
				to.Optimization = opt;
				to.Table = table;
				to.OriginalTable = table;
				to.IsNewGroupByOrJoinTable = true;
				token.ThrowIfCancellationRequested();
				Dictionary<int, string> newColNames = new Dictionary<int, string>();
				func(db, to.GetGroupByString(db, filter, db.DbConfig.DbName, tableName, user, filterIssue, out newColNames));
				db.AddAutoIncrementPrimaryColumn(db.DbConfig.DbName, tableName, "_row_no_");
				List<Tuple<IColumn, string, string>> columns = new List<Tuple<IColumn, string, string>>();
				if (colIds != null)
				{
					foreach (int c2 in colIds)
					{
						token.ThrowIfCancellationRequested();
						if (table.Columns.Contains(c2))
						{
							columns.Add(new Tuple<IColumn, string, string>(table.Columns[c2], table.Columns[c2].Name, null));
						}
					}
				}
				IAggregationFunctionTranslator translator = DatabaseSpecificFactory.GetAggregationFunctionTranslator();
				foreach (Aggregation a in aggs)
				{
					token.ThrowIfCancellationRequested();
					if (table.Columns.Contains(a.cid))
					{
						if (!newColNames.TryGetValue(a.cid, out var name))
						{
							name = translator.ConvertEnumToString(a.agg).ToLower() + "_" + table.Columns[a.cid].Name.ToLower();
						}
						columns.Add(new Tuple<IColumn, string, string>(table.Columns[a.cid], name, translator.ConvertEnumToString(a.agg)));
					}
				}
				to.Table = _systemDb.CreateTemporaryGroupTableObject(to.Table, db.DbConfig.DbName, columns, Key, descriptions, languages, aggDescriptions, tableName);
				to.Key = Key;
				_systemDb.SetRowCount(to.Table, db.CountTable(db.DbConfig.DbName, to.Key));
			}
			return to;
		}

		public TableObject CreateJoinTable(ReaderTransformation func, Join join, IOptimization opt, Dictionary<string, string> descriptions, IEnumerable<ILanguage> languages, CancellationToken token, IUser user, bool saveJoin, ViewboxDb viewboxDb)
		{
			string startedByUser = $"(started by: {user.UserName})";
			using DatabaseBase connection = _cmanager.GetConnection();
			string Key = "";
			TableObject to = null;
			int hashCode = join?.GetHashCode() ?? 0;
			hashCode += opt?.Id.GetHashCode() ?? 0;
			token.ThrowIfCancellationRequested();
			bool foundTable = false;
			int? forcedTableId = null;
			if (!saveJoin && _hashTable.ContainsKey(hashCode))
			{
				Key = _hashTable[hashCode];
				to = _tableObjects[Key];
				foundTable = true;
				forcedTableId = to.Table.Id;
			}
			if (!foundTable)
			{
				Key = BitConverter.ToString(Guid.NewGuid().ToByteArray()).Replace("-", string.Empty);
				to = new TableObject();
			}
			to.Join = join;
			to.Table = join.Table1;
			to.Optimization = opt;
			to.OriginalTable = join.Table1;
			to.IsNewGroupByOrJoinTable = true;
			token.ThrowIfCancellationRequested();
			JoinIndexHelper jih = new JoinIndexHelper(_cmanager, join, token);
			token.ThrowIfCancellationRequested();
			if (!jih.Index1.Exists && CheckDataTypeSkipIndex(viewboxDb, jih.Index1.Database, join.Table1.TableName, jih.Index1.ColNames))
			{
				LogHelper.GetLogger().InfoFormat("[FUNCTIONS] - Creating index `{0}` on table {1}(relating future join temptable will be `{2}`){3}", jih.Index1.SQLCreateIndex, connection.Enquote(jih.Index1.Database, join.Table1.TableName), Key, startedByUser);
				func(connection, jih.Index1.SQLCreateIndex);
			}
			if (!jih.Index2.Exists && jih.Index1.Name != jih.Index2.Name && CheckDataTypeSkipIndex(viewboxDb, jih.Index2.Database, join.Table2.TableName, jih.Index2.ColNames))
			{
				LogHelper.GetLogger().InfoFormat("[FUNCTIONS] - Creating index `{0}` on table {1}(relating future join temptable will be `{2}`){3}", jih.Index2.SQLCreateIndex, connection.Enquote(jih.Index2.Database, join.Table2.TableName), Key, startedByUser);
				func(connection, jih.Index2.SQLCreateIndex);
			}
			token.ThrowIfCancellationRequested();
			LogHelper.GetLogger().InfoFormat("[FUNCTIONS] - Creating join temptable (`{0}`){1}", Key, startedByUser);
			func(connection, to.GetJoinString(connection, opt, connection.DbConfig.DbName, Key));
			if (!connection.GetColumnNames(connection.DbConfig.DbName, Key).Contains("_row_no_"))
			{
				LogHelper.GetLogger().InfoFormat("[FUNCTIONS] - Adding primary key column `{0}` to join temptable `{1}`{2}", "_row_no_", Key, startedByUser);
				connection.AddAutoIncrementPrimaryColumn(connection.DbConfig.DbName, Key, "_row_no_");
			}
			if (!jih.Index1.Exists && CheckIndexIfExist(viewboxDb, jih.Index1.Database, join.Table1.TableName, jih.Index1.Name))
			{
				LogHelper.GetLogger().InfoFormat("[FUNCTIONS] - Dropping index `{0}` from table {1}(relating join temptable is `{2}`){3}", jih.Index1.SQLDropIndex, connection.Enquote(jih.Index1.Database, join.Table1.TableName), Key, startedByUser);
				func(connection, jih.Index1.SQLDropIndex);
			}
			if (!jih.Index2.Exists && jih.Index1.Name != jih.Index2.Name && CheckIndexIfExist(viewboxDb, jih.Index2.Database, join.Table2.TableName, jih.Index2.Name))
			{
				LogHelper.GetLogger().InfoFormat("[FUNCTIONS] - Dropping index `{0}` from table {1}(relating join temptable is `{2}`){3}", jih.Index2.SQLDropIndex, connection.Enquote(jih.Index2.Database, join.Table2.TableName), Key, startedByUser);
				func(connection, jih.Index2.SQLDropIndex);
			}
			to.Table = _systemDb.CreateTemporaryJoinTableObject(to.Table, connection.DbConfig.DbName, join.Table1.Columns, join.Table2.Columns, Key, descriptions, languages, null, forcedTableId);
			to.Key = Key;
			_systemDb.SetRowCount(to.Table, connection.CountTable(connection.DbConfig.DbName, to.Key));
			if (saveJoin)
			{
				using DatabaseBase viewboxDbConn = viewboxDb.ConnectionManager.GetConnection();
				string database = GetOptimizationValue(opt, OptimizationType.System);
				LogHelper.GetLogger().InfoFormat("[FUNCTIONS] - Saving join temptable (`{0}`) as view(description: '{1}'){2}", to.Table.TableName, (descriptions.Count > 0) ? descriptions.Values.ElementAt(0) : string.Empty, startedByUser);
				to.Table = _systemDb.SaveView(viewboxDbConn, to.Table, languages, user, database, joinSave: true);
			}
			else
			{
				to.Table.OrderAreas.Clear();
			}
			if (!foundTable)
			{
				if (!saveJoin)
				{
					connection.DbMapping.CreateTableIfNotExists<HashTable>();
					HashTable h = new HashTable
					{
						HashCode = hashCode,
						TableName = Key
					};
					connection.DbMapping.Save(h);
					_hashTable.Add(hashCode, Key);
				}
				_tableObjects.Add(Key, to);
			}
			return to;
		}

		public bool PreAnalyzeJoinTable(Join join, IOptimization opt, long enabledLines, long enabledLinesForIndexing, out long calculatedRowCount, out long indexingRowCount, out bool indexingAffects)
		{
			calculatedRowCount = 0L;
			indexingRowCount = 0L;
			indexingAffects = false;
			TableObject to = new TableObject();
			to.Join = join;
			to.Table = join.Table1;
			to.Optimization = opt;
			to.OriginalTable = join.Table1;
			using (DatabaseBase connection = _cmanager.GetConnection())
			{
				if (enabledLinesForIndexing > -1)
				{
					JoinIndexHelper jih = new JoinIndexHelper(_cmanager, join, CancellationToken.None);
					if (!jih.Index1.Exists)
					{
						indexingAffects = true;
						indexingRowCount += connection.GetEstimatedRowCount("SELECT * FROM " + connection.Enquote(jih.Index1.Database, join.Table1.TableName)) * 2;
					}
					if (!jih.Index2.Exists && jih.Index1.Name != jih.Index2.Name)
					{
						indexingAffects = true;
						indexingRowCount += connection.GetEstimatedRowCount("SELECT * FROM " + connection.Enquote(jih.Index2.Database, join.Table2.TableName)) * 2;
					}
					if (indexingRowCount > enabledLinesForIndexing)
					{
						return false;
					}
				}
				if (enabledLines > -1)
				{
					calculatedRowCount = join.Table1.RowCount + join.Table2.RowCount;
					to.GetJoinString(connection, opt, "", "", withoutCreateTable: true);
					if (calculatedRowCount > enabledLines)
					{
						return false;
					}
				}
			}
			return true;
		}

		public bool CheckDataTypeSkipIndex(ViewboxDb viewboxDb, string databaseName, string tableName, List<string> columnNameList)
		{
			foreach (string columnName in columnNameList)
			{
				string sql = $"select `data_type` from `information_schema`.`columns` where `table_schema` = '{databaseName}' and `table_name` = '{tableName}' and `column_name` = '{columnName.Trim('`')}'";
				IDataReader dataReaderResult = null;
				using DatabaseBase viewboxDbConn = viewboxDb.ConnectionManager.GetConnection();
				dataReaderResult = viewboxDbConn.ExecuteReader(sql);
				List<string> resultList = new List<string>();
				while (dataReaderResult.Read())
				{
					resultList.Add(dataReaderResult.GetString(0));
				}
				if (resultList.Any((string resultString) => resultString.ToLower() == "text"))
				{
					return false;
				}
			}
			return true;
		}

		private bool CheckIndexIfExist(ViewboxDb viewboxDb, string databaseName, string tableName, string indexName)
		{
			string sql = $"select count(*) FROM `information_schema`.`statistics` where `table_schema` = '{databaseName}' and `table_name` = '{tableName}' and `index_name` = '{indexName}'";
			object result = null;
			using (DatabaseBase viewboxDbConn = viewboxDb.ConnectionManager.GetConnection())
			{
				result = viewboxDbConn.ExecuteScalar(sql);
			}
			if (result == null)
			{
				return false;
			}
			string s = result.ToString();
			int i = 0;
			if (int.TryParse(s, out i) && i > 0)
			{
				return true;
			}
			return false;
		}

		public DataTable GetData(TableObject tobj, IOptimization opt, long start, int size, IUser user, PageSystem pageSystem = null, string roleBasedOptimizationFilter = null)
		{
			using DatabaseBase db = _cmanager.GetConnection();
			try
			{
				if (pageSystem != null)
				{
					pageSystem.RoleBasedOptimizationFilter = roleBasedOptimizationFilter;
					pageSystem.CountTable(db, tobj.Table);
				}
				string dataTableSelection = tobj.GetSelectionString(db, opt, start, size, user, selectRowNo: true, getAllFields: false, pageSystem, roleBasedOptimizationFilter);
				return db.GetDataTable(dataTableSelection);
			}
			catch (Exception ex)
			{
				_log.ErrorFormat("Opening the table is failed: Table-id: {0}; Script: {1}", tobj.OriginalTable.Id, tobj.GetSelectionString(db, opt, start, size, user, selectRowNo: true, getAllFields: false, pageSystem, roleBasedOptimizationFilter));
				throw ex;
			}
		}

		public long ComputeDataCount(ITableObject to)
		{
			using DatabaseBase db = _cmanager.GetConnection();
			try
			{
				return db.CountTable(to.Database, to.TableName);
			}
			catch (Exception ex)
			{
				throw new Exception("Fehler beim Ermitteln der Datensatzanzahl: " + ex.Message + ex.StackTrace);
			}
		}

		public long ComputePagedDataCount(ITableObject to)
		{
			if (to != null && to.PageSystem != null)
			{
				using (DatabaseBase db = _cmanager.GetConnection())
				{
					try
					{
						to.PageSystem.CountTable(db);
						return to.PageSystem.Count;
					}
					catch (Exception ex)
					{
						throw new Exception("Fehler beim Ermitteln der Datensatzanzahl: " + ex.Message + ex.StackTrace);
					}
				}
			}
			return 0L;
		}

		public IFilter GetFilter(ITableObject tableObject, string toParse, bool decode, IColumnCollection cols = null)
		{
			return Filter.GetFilter(tableObject, toParse, decode, cols);
		}

		public void LoadExportData(ViewboxDb.ReaderExport func, ExportType type, TableObjectCollection tempTableObjects, IOptimization opt, CancellationToken token, ILanguage language, int page, long size, ITableObjectCollection objects, IUser user, string exportDescription, string fileName, bool getAllFields, List<string> roleBasedOptFilterList, IEnumerable<IOptimization> optimization = null)
		{
			using DatabaseBase connection = _cmanager.GetConnection();
			List<string> sqlList = new List<string>();
			List<List<IColumn>> columnsList = new List<List<IColumn>>();
			int i = 0;
			foreach (TableObject tobj in tempTableObjects)
			{
				string roleBasedOptFilter = "";
				if (roleBasedOptFilterList.Count > i)
				{
					roleBasedOptFilter = roleBasedOptFilterList.ElementAt(i);
				}
				token.ThrowIfCancellationRequested();
				objects.Add(tobj.Table);
				PageSystem pageSystem = ((tobj.Table == null) ? null : tobj.Table.PageSystem);
				sqlList.Add(LoadExportData(connection, tobj, opt, page, size, user, getAllFields, pageSystem, "", roleBasedOptFilter));
				if (tobj.GroupSubTotal != null && tobj.GroupSubTotal.ColumnList != null && tobj.GroupSubTotal.GroupList != null)
				{
					List<IColumn> List = new List<IColumn>();
					foreach (string c3 in tobj.GroupSubTotal.GroupList)
					{
						List.Add(tobj.Table.Columns.FirstOrDefault((IColumn column) => column.Name == c3));
					}
					foreach (string c2 in tobj.GroupSubTotal.ColumnList)
					{
						List.Add(tobj.Table.Columns.FirstOrDefault((IColumn column) => column.Name == c2));
					}
					columnsList.Add(List);
				}
				else
				{
					columnsList.Add(new List<IColumn>(tobj.Table.Columns.Where((IColumn c) => !c.IsEmpty && c.IsVisible)));
				}
				i++;
			}
			new MySqlCommand("set net_write_timeout=99999; set net_read_timeout=99999", (MySqlConnection)connection.Connection).ExecuteNonQuery();
			token.ThrowIfCancellationRequested();
			func(connection, type, objects, sqlList, columnsList, token, language, opt, user, exportDescription, fileName, tempTableObjects, optimization);
		}

		public TableObject CreateTempTableFromSql(string selectSql, List<DbColumnInfo> columns = null)
		{
			using DatabaseBase connection = _cmanager.GetConnection();
			string Key = "";
			TableObject to = new TableObject();
			int hashCode = Math.Abs(selectSql.GetHashCode());
			bool tempTableExists = false;
			if (_hashTable.ContainsKey(hashCode))
			{
				Key = _hashTable[hashCode];
				to = _tableObjects[Key];
				tempTableExists = connection.TableExists(connection.DbConfig.DbName, Key);
			}
			if (!tempTableExists)
			{
				Key = BitConverter.ToString(Guid.NewGuid().ToByteArray()).Replace("-", string.Empty);
				to = new TableObject();
				HashTable h = new HashTable
				{
					HashCode = hashCode,
					TableName = Key
				};
				connection.DbMapping.Save(h);
				if (!_hashTable.ContainsKey(hashCode))
				{
					_hashTable.Add(hashCode, Key);
				}
				if (!_tableObjects.ContainsKey(Key))
				{
					_tableObjects.Add(Key, to);
				}
				StringBuilder builder = new StringBuilder();
				builder.Append(" INSERT INTO ").Append(connection.Enquote(connection.DbConfig.DbName, Key)).Append(" ( ");
				bool first = true;
				if (columns == null)
				{
					columns = new List<DbColumnInfo>
					{
						new DbColumnInfo
						{
							Name = "_row_no_",
							Type = DbColumnTypes.DbInt
						}
					};
				}
				foreach (DbColumnInfo dbColumnInfo in columns)
				{
					dbColumnInfo.Name = "_" + dbColumnInfo.Name + "_";
					dbColumnInfo.IsPrimaryKey = false;
					dbColumnInfo.IsIdentity = false;
					dbColumnInfo.AutoIncrement = false;
					if (!first)
					{
						builder.Append(", ");
					}
					else
					{
						first = false;
					}
					builder.Append(dbColumnInfo.Name);
				}
				builder.Append(" ) ");
				builder.Append(selectSql);
				string insertSql = builder.ToString();
				columns.Insert(0, new DbColumnInfo
				{
					Name = "id",
					Type = DbColumnTypes.DbInt,
					IsPrimaryKey = true,
					AutoIncrement = true
				});
				connection.CreateTable(Key, columns);
				columns.RemoveAt(0);
				foreach (DbColumnInfo columnInfo in columns)
				{
					connection.CreateIndex(Key, columnInfo.Name + "_I", new string[1] { columnInfo.Name });
				}
				connection.ExecuteNonQuery(insertSql);
				to.Key = Key;
			}
			return to;
		}

		public Dictionary<string, decimal> SummarizeColumns(string tableName, IList<string> columnNames)
		{
			try
			{
				using DatabaseBase db = _cmanager.GetConnection();
				string summarizeQuery = CreateSummarizeQuery(tableName, columnNames);
				DataTable dataTable = db.GetDataTable(summarizeQuery);
				return columnNames.ToDictionary((string columnName) => columnName, (string columnName) => (decimal)dataTable.Rows[0][columnName]);
			}
			catch (Exception exception)
			{
				_log.Error(exception);
				return new Dictionary<string, decimal>();
			}
		}

		private string CreateSummarizeQuery(string tableName, IEnumerable<string> columnNames)
		{
			return string.Format("SELECT {0} FROM {1}", string.Join(",", columnNames.Select((string c) => string.Format("SUM({0}) AS {0}", c)).ToList()), tableName);
		}

		public string GetThumbnailPathByPath(ITableObject table, string path)
		{
			using DatabaseBase db = _cmanager.GetConnection();
			try
			{
				return db.GetThumbnailPathByPath(table.Database, table.TableName, path);
			}
			catch (Exception)
			{
				return string.Empty;
			}
		}

		public string GetReserve(ITableObject table, string path)
		{
			using DatabaseBase db = _cmanager.GetConnection();
			try
			{
				return db.GetReserve(table.Database, table.TableName, path);
			}
			catch (Exception)
			{
				return string.Empty;
			}
		}

		public string GetPath(ITableObject table, string filter)
		{
			using DatabaseBase db = _cmanager.GetConnection();
			try
			{
				return db.GetPath(table.Database, table.TableName, filter);
			}
			catch (Exception)
			{
				return string.Empty;
			}
		}

		public int GetMaxValueOfColumn(ITableObject table, string column, string filter)
		{
			using DatabaseBase db = _cmanager.GetConnection();
			try
			{
				return db.GetMaxValueOfColumn(table.Database, table.TableName, column, filter);
			}
			catch (Exception)
			{
				return -1;
			}
		}

		public List<string> GetBelegPathes(string dbName, string dokId)
		{
			using DatabaseBase db = _cmanager.GetConnection();
			return db.GetBelegParts(dbName, dokId);
		}
	}
}
