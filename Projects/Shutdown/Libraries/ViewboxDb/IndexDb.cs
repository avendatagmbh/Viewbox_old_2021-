using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Text;
using SystemDb;
using AV.Log;
using DbAccess;
using DbAccess.Enums;
using DbAccess.Structures;
using log4net;
using Utils;

namespace ViewboxDb
{
	public class IndexDb
	{
		public static string ValueTablePrefix = "VALUE_";

		public static string IndexTablePrefix = "INDEX_";

		public static string RowNoTablePrefix = "ROW_NO_";

		private readonly ILog _log = LogHelper.GetLogger();

		private readonly global::SystemDb.SystemDb _systemDb;

		private readonly TempDb _tempDb;

		private readonly ViewboxDb _viewBoxDb;

		private ConnectionManager _connectionManager;

		private string _connectionString;

		private bool _isInitialized;

		private string _provider;

		public string Postfix { get; private set; }

		public string DbName { get; private set; }

		public ConnectionManager ConnectionManager => _connectionManager;

		public string ConnectionString => _connectionString;

		public string DataProvider => _provider;

		public IndexDb(global::SystemDb.SystemDb systemDb, ViewboxDb viewBoxDb, TempDb tempDb, string indexDb_postfix)
		{
			_viewBoxDb = viewBoxDb;
			_systemDb = systemDb;
			_tempDb = tempDb;
			Postfix = indexDb_postfix;
		}

		public void Connect()
		{
			DbName = _viewBoxDb.ConnectionManager.DbConfig.DbName + Postfix;
			if (string.IsNullOrEmpty(DbName))
			{
				_log.Error("Can not initialize IndexDb. DbName = " + DbName);
				return;
			}
			try
			{
				DbConfig cfg = new DbConfig
				{
					ConnectionString = _systemDb.ConnectionString,
					DbName = string.Empty
				};
				using (DatabaseBase db = ConnectionManager.CreateConnection(cfg))
				{
					db.Open();
					db.CreateDatabaseIfNotExists(DbName);
				}
				cfg.DbName = DbName;
				_connectionString = cfg.ConnectionString;
				_connectionManager = new ConnectionManager(cfg, 50);
				if (!_connectionManager.IsInitialized)
				{
					_connectionManager.Init();
				}
				_isInitialized = true;
			}
			catch (Exception ex)
			{
				_log.Error("Error while initializing IndexDb", ex);
			}
		}

		public TableObject GetIndexList(ref int _columnId, int _parameterId, IOptimization opt, string search, bool exactMatch, bool isPreview)
		{
			if (!_isInitialized)
			{
				_log.Error("IndexDb not initialized");
				return null;
			}
			using DatabaseBase connection = _connectionManager.GetConnection();
			StringBuilder builder = new StringBuilder();
			object tableId;
			object columnId;
			if (_columnId == 0)
			{
				builder.Clear();
				builder.Append("SELECT LOWER(COALESCE(COLUMN_NAME, '')), LOWER(COALESCE(TABLE_NAME, '')), LOWER(COALESCE(DATABASE_NAME, '')) FROM ").Append(connection.Enquote(_viewBoxDb.ConnectionManager.DbConfig.DbName, "PARAMETER")).Append(" WHERE ID = ")
					.Append(_parameterId);
				string columnName = "";
				string tableName = "";
				string databaseName = "";
				using (IDataReader reader = connection.ExecuteReader(builder.ToString()))
				{
					if (!reader.Read())
					{
						_log.Log(LogLevelEnum.Error, "Could not find the parameter");
						return null;
					}
					columnName = reader.GetString(0);
					tableName = reader.GetString(1);
					databaseName = reader.GetString(2);
				}
				if (string.IsNullOrEmpty(columnName) || string.IsNullOrEmpty(tableName))
				{
					_log.Log(LogLevelEnum.Error, "Could not identify the column name or the issue from the parameter");
					return null;
				}
				builder.Clear();
				builder.Append("SELECT ID FROM ").Append(connection.Enquote(_viewBoxDb.ConnectionManager.DbConfig.DbName, "TABLES")).Append(" WHERE LOWER(NAME) = '")
					.Append(tableName)
					.Append("'");
				if (!string.IsNullOrEmpty(databaseName))
				{
					builder.Append(" AND LOWER(").Append(connection.Enquote("DATABASE")).Append(") = '")
						.Append(databaseName)
						.Append("'");
				}
				tableId = connection.ExecuteScalar(builder.ToString());
				if (tableId == null)
				{
					_log.Log(LogLevelEnum.Error, "Could not identify the table");
					return null;
				}
				builder.Clear();
				columnId = connection.ExecuteScalar(builder.Append("SELECT ID FROM ").Append(connection.Enquote(_viewBoxDb.ConnectionManager.DbConfig.DbName, "COLUMNS")).Append(" WHERE LOWER(NAME) = '")
					.Append(columnName)
					.Append("' AND TABLE_ID = ")
					.Append(tableId)
					.ToString());
			}
			else
			{
				columnId = _columnId;
				tableId = connection.ExecuteScalar(builder.Append("SELECT TABLE_ID FROM ").Append(connection.Enquote(_viewBoxDb.ConnectionManager.DbConfig.DbName, "COLUMNS")).Append(" WHERE ID = ")
					.Append(columnId)
					.ToString());
			}
			if (columnId == null)
			{
				_log.Log(LogLevelEnum.Error, "Could not identify the column id");
				return null;
			}
			builder.Clear();
			string indexTableName = builder.Append(IndexTablePrefix).Append(columnId).ToString();
			builder.Clear();
			string valueTableName = builder.Append(ValueTablePrefix).Append(columnId).ToString();
			builder.Clear();
			builder.Append("SELECT COUNT(ID) FROM ").Append(connection.Enquote(_viewBoxDb.ConnectionManager.DbConfig.DbName, "ORDER_AREAS")).Append("WHERE")
				.Append($" table_id = {tableId}");
			int areaCount = 0;
			object objAreaCount = connection.ExecuteScalar(builder.ToString());
			if (objAreaCount != null)
			{
				int.TryParse(objAreaCount.ToString(), out areaCount);
			}
			builder.Clear();
			builder.Append("SELECT COUNT(ID) FROM ").Append(connection.Enquote(_viewBoxDb.ConnectionManager.DbConfig.DbName, "ORDER_AREAS")).Append("WHERE")
				.Append($" table_id = {tableId}")
				.Append(string.IsNullOrEmpty(opt.FindValue(OptimizationType.IndexTable)) ? "" : $" AND (index_value IS NULL OR index_value = '' OR index_value = '{opt.FindValue(OptimizationType.IndexTable)}')")
				.Append(string.IsNullOrEmpty(opt.FindValue(OptimizationType.SplitTable)) ? "" : $" AND (split_value IS NULL OR split_value = '' OR split_value = '{opt.FindValue(OptimizationType.SplitTable)}')")
				.Append(string.IsNullOrEmpty(opt.FindValue(OptimizationType.SortColumn)) ? "" : $" AND (sort_value IS NULL OR sort_value = '' OR sort_value = '{opt.FindValue(OptimizationType.SortColumn)}')");
			int filteredAreaCount = 0;
			object objfilteredAreaCount = connection.ExecuteScalar(builder.ToString());
			if (objfilteredAreaCount != null)
			{
				int.TryParse(objfilteredAreaCount.ToString(), out filteredAreaCount);
			}
			List<DbColumnInfo> columns = null;
			try
			{
				columns = connection.GetColumnInfos(valueTableName);
			}
			catch (Exception ex2)
			{
				_log.Error("The table does not exists in the index database: ", ex2);
				return null;
			}
			if (columns == null)
			{
				_log.Log(LogLevelEnum.Error, "The table does not exists in the index database: " + valueTableName);
				return null;
			}
			builder.Clear();
			builder.Append(" SELECT DISTINCT ");
			DbColumnInfo valueColumn = null;
			bool first = true;
			foreach (DbColumnInfo dbColumnInfo in columns)
			{
				if (!first)
				{
					builder.Append(", ");
				}
				else
				{
					first = false;
				}
				builder.Append("V.").Append(dbColumnInfo.Name);
				if (string.Compare(dbColumnInfo.Name, "value", StringComparison.InvariantCultureIgnoreCase) == 0)
				{
					valueColumn = dbColumnInfo;
				}
			}
			if (areaCount < 2)
			{
				builder.Append(" FROM ").Append(connection.Enquote(connection.DbConfig.DbName, valueTableName)).Append(" V ");
				if (areaCount == 0 || filteredAreaCount == 1)
				{
					if (!string.IsNullOrWhiteSpace(search))
					{
						builder.Append(" WHERE ");
						AppendFilter(search, builder, valueColumn, startWithAnd: false, exactMatch);
					}
				}
				else
				{
					builder.Append(" WHERE 1=0 ");
				}
			}
			else
			{
				builder.Append(" FROM ").Append(connection.Enquote(connection.DbConfig.DbName, indexTableName)).Append(" I ")
					.Append(" JOIN ")
					.Append(connection.Enquote(_viewBoxDb.ConnectionManager.DbConfig.DbName, "ORDER_AREAS"))
					.Append(" O ")
					.Append("  ON O.ID = I.ORDER_AREAS_ID ")
					.Append(" JOIN ")
					.Append(connection.Enquote(connection.DbConfig.DbName, valueTableName))
					.Append(" V ")
					.Append(" ON V.ID = I.VALUE_ID ")
					.Append(" WHERE ")
					.Append($" table_id = {tableId}")
					.Append(string.IsNullOrWhiteSpace(opt.FindValue(OptimizationType.IndexTable)) ? "" : $" AND (index_value IS NULL OR index_value = '{opt.FindValue(OptimizationType.IndexTable)}')")
					.Append(string.IsNullOrWhiteSpace(opt.FindValue(OptimizationType.SplitTable)) ? "" : $" AND (split_value IS NULL OR split_value = '{opt.FindValue(OptimizationType.SplitTable)}')")
					.Append(string.IsNullOrWhiteSpace(opt.FindValue(OptimizationType.SortColumn)) ? "" : $" AND (sort_value IS NULL OR sort_value = '{opt.FindValue(OptimizationType.SortColumn)}')");
				AppendFilter(search, builder, valueColumn, startWithAnd: true, exactMatch);
			}
			if (isPreview)
			{
				builder.Append(" LIMIT 150");
			}
			string sql = builder.ToString();
			try
			{
				return _tempDb.CreateTempTableFromSql(sql, columns);
			}
			catch (Exception ex)
			{
				_log.Error("Couldn't execute sql: " + sql, ex);
			}
			return null;
		}

		public void HasIndexData(DatabaseBase connection, IParameter parameter, IOptimization opt, int orderAreasCount, List<OptimizationType> issueOptimizationFilter, int tableId = -1, int columnId = -1)
		{
			parameter.IndexDataGenerated = false;
			parameter.HasIndexData = false;
			if (!_isInitialized)
			{
				_log.Error("IndexDb not initialized");
				return;
			}
			if (string.IsNullOrEmpty(parameter.ColumnName) || string.IsNullOrEmpty(parameter.TableName))
			{
				_log.Log(LogLevelEnum.Error, "Could not identify the column name or the issue from the parameter");
				return;
			}
			if (tableId == -1)
			{
				_log.Log(LogLevelEnum.Error, "Could not identify the table id");
				return;
			}
			if (columnId == -1)
			{
				_log.Log(LogLevelEnum.Error, "Could not identify the column id");
				return;
			}
			bool indexDataGenerated = false;
			bool hasIndexData = false;
			HasColumnIndexData(columnId, tableId, connection, opt, out indexDataGenerated, out hasIndexData, orderAreasCount, issueOptimizationFilter);
			parameter.IndexDataGenerated = indexDataGenerated;
			parameter.HasIndexData = hasIndexData;
		}

		public void HasColumnIndexData(object columnId, object tableId, DatabaseBase connection, IOptimization opt, out bool indexDataGenerated, out bool hasIndexData, int orderAreasCount, List<OptimizationType> issueOptimizationFilter)
		{
			indexDataGenerated = false;
			hasIndexData = false;
			try
			{
				StringBuilder builder = new StringBuilder();
				builder.Clear();
				string indexTableName = builder.Append(IndexTablePrefix).Append(columnId).ToString();
				builder.Clear();
				string valueTableName = builder.Append(ValueTablePrefix).Append(columnId).ToString();
				builder.Clear();
				builder.Append(" SELECT I.ID ");
				builder.Append(" FROM ").Append(connection.Enquote(connection.DbConfig.DbName, valueTableName)).Append(" I ")
					.Append("LIMIT 1");
				try
				{
					object obj2 = null;
					StringBuilder query = new StringBuilder();
					query.Clear();
					query.Append("select count(*) from ").Append("information_schema.tables where TABLE_SCHEMA = \"").Append(connection.DbConfig.DbName)
						.Append("\" and table_name = \"");
					query.Append(valueTableName).Append("\";");
					int num = int.Parse(connection.ExecuteScalar(query.ToString()).ToString());
					DatabaseBase conn2 = new DatabaseBase(connection.DbConfig);
					conn2.DbConfig.DbName = connection.DbConfig.DbName.Replace("_index", "");
					if (num == 0)
					{
						query.Clear();
						query.Append("select `database` from ").Append(conn2.Enquote(conn2.DbConfig.DbName, "tables")).Append(" where id = ")
							.Append(tableId.ToString());
						string dbName = conn2.GetColumnValues(query.ToString())[0].ToString();
						query.Clear();
						query.Append("select `name` from ").Append(conn2.Enquote(conn2.DbConfig.DbName, "tables")).Append(" where id = ")
							.Append(tableId.ToString());
						string tableName = conn2.GetColumnValues(query.ToString())[0].ToString();
						query.Clear();
						query.Append("select `name` from ").Append(conn2.Enquote(conn2.DbConfig.DbName, "columns")).Append(" where id = ")
							.Append(columnId.ToString());
						string colName = conn2.GetColumnValues(query.ToString())[0].ToString();
						query.Clear();
						conn2.DbConfig.DbName = dbName;
						query.Append("select count(*) from ").Append(conn2.Enquote(conn2.DbConfig.DbName, tableName)).Append(" where `")
							.Append(colName)
							.Append("` is not null and `");
						query.Append(colName).Append("` != \"\";");
						if (int.Parse(conn2.ExecuteScalar(query.ToString()).ToString()) == 0 || 1 == 0)
						{
							indexDataGenerated = true;
						}
						else
						{
							indexDataGenerated = false;
						}
					}
					else
					{
						query.Clear();
						query.Append("select count(*) from ").Append(connection.Enquote(connection.DbConfig.DbName, valueTableName));
						if (int.Parse(connection.ExecuteScalar(query.ToString()).ToString()) == 0)
						{
							indexDataGenerated = true;
						}
						else
						{
							obj2 = connection.ExecuteScalar(builder.ToString());
							indexDataGenerated = obj2 != null;
						}
					}
				}
				catch (Exception ex2)
				{
					_log.Error("Couldn't execute sql: " + builder, ex2);
				}
				builder.Clear();
				if (orderAreasCount < 1)
				{
					builder.Append(" SELECT I.ID ");
					builder.Append(" FROM ").Append(connection.Enquote(connection.DbConfig.DbName, valueTableName)).Append(" I ");
				}
				else
				{
					builder.Append(" SELECT I.VALUE_ID ");
					builder.Append(" FROM ").Append(connection.Enquote(connection.DbConfig.DbName, indexTableName)).Append(" I ")
						.Append(" JOIN ")
						.Append(connection.Enquote(_viewBoxDb.ConnectionManager.DbConfig.DbName, "ORDER_AREAS"))
						.Append(" O ")
						.Append("  ON O.ID = I.ORDER_AREAS_ID ")
						.Append(" WHERE ")
						.Append($" O.table_id = {tableId}")
						.Append((string.IsNullOrWhiteSpace(opt.FindValue(OptimizationType.IndexTable)) || (issueOptimizationFilter != null && issueOptimizationFilter.Contains(OptimizationType.IndexTable))) ? "" : $" AND (index_value IS NULL OR index_value = '{opt.FindValue(OptimizationType.IndexTable)}')")
						.Append((string.IsNullOrWhiteSpace(opt.FindValue(OptimizationType.SplitTable)) || (issueOptimizationFilter != null && issueOptimizationFilter.Contains(OptimizationType.SplitTable))) ? "" : $" AND (split_value IS NULL OR split_value = '{opt.FindValue(OptimizationType.SplitTable)}')")
						.Append((string.IsNullOrWhiteSpace(opt.FindValue(OptimizationType.SortColumn)) || (issueOptimizationFilter != null && issueOptimizationFilter.Contains(OptimizationType.SortColumn))) ? "" : $" AND (sort_value IS NULL OR sort_value = '{opt.FindValue(OptimizationType.SortColumn)}')");
				}
				builder.Append(" LIMIT 1");
				string sql = builder.ToString();
				try
				{
					object obj = connection.ExecuteScalar(sql);
					hasIndexData = obj != null;
				}
				catch (Exception ex)
				{
					_log.Error("Couldn't execute sql: " + sql, ex);
				}
			}
			catch (Exception e)
			{
				_log.Error("Couldn't execute sql: ", e);
			}
		}

		private static void AppendFilter(string search, StringBuilder builder, DbColumnInfo valueColumn, bool startWithAnd, bool exactMatch)
		{
			search = (string.IsNullOrWhiteSpace(search) ? string.Empty : search);
			switch (valueColumn?.Type ?? DbColumnTypes.DbText)
			{
			case DbColumnTypes.DbDate:
				AppendDateFilter(search, builder, startWithAnd, exactMatch, isDateTime: false);
				break;
			case DbColumnTypes.DbDateTime:
			{
				List<string> parts = new List<string>();
				if (search.IndexOf(":", StringComparison.InvariantCultureIgnoreCase) > 0)
				{
					parts = new List<string>(search.Split(new string[1] { ":" }, StringSplitOptions.RemoveEmptyEntries));
				}
				if (parts.Count > 0)
				{
					string timePart = string.Empty;
					int i = 0;
					foreach (string part in parts)
					{
						if (i != 0)
						{
							timePart += part;
						}
						i++;
					}
					string datePart = parts[0];
					int indexOfTime = parts[0].LastIndexOf(" ", StringComparison.InvariantCultureIgnoreCase);
					if (indexOfTime > 0)
					{
						timePart = parts[0].Substring(indexOfTime).Trim() + timePart;
						AppendTimePart(timePart, builder, startWithAnd, exactMatch);
						datePart = parts[0].Substring(0, indexOfTime).Trim();
					}
					AppendDateFilter(datePart, builder, startWithAnd, exactMatch, isDateTime: true);
				}
				else
				{
					AppendDateFilter(search, builder, startWithAnd, exactMatch, isDateTime: true);
				}
				break;
			}
			case DbColumnTypes.DbTime:
				AppendTimePart(search, builder, startWithAnd, exactMatch);
				break;
			case DbColumnTypes.DbNumeric:
				AppendDecimalPart(search, builder, startWithAnd, exactMatch, valueColumn);
				break;
			case DbColumnTypes.DbInt:
			case DbColumnTypes.DbBigInt:
				AppendIntPart(search, builder, startWithAnd, exactMatch);
				break;
			case DbColumnTypes.DbText:
			case DbColumnTypes.DbLongText:
			case DbColumnTypes.DbXML:
			case DbColumnTypes.DbUnknown:
				AppendStringPart(search, builder, startWithAnd, exactMatch);
				break;
			case DbColumnTypes.DbBool:
			case DbColumnTypes.DbBinary:
			case DbColumnTypes.DbTimeStamp:
				break;
			}
		}

		private static void AppendDecimalPart(string search, StringBuilder builder, bool startWithAnd, bool exactMatch, DbColumnInfo column)
		{
			search = search.Replace(",", "");
			if (!exactMatch && !string.IsNullOrEmpty(search))
			{
				search = "%" + search + "%";
				builder.Append(string.IsNullOrWhiteSpace(search) ? "" : ((startWithAnd ? " AND " : "") + $" CAST(V.VALUE AS CHAR(4000)) LIKE '{search}'"));
			}
			else
			{
				builder.Append(string.IsNullOrWhiteSpace(search) ? "" : ((startWithAnd ? " AND " : "") + string.Format(" CAST(V.VALUE AS DECIMAL({1},{2})) = CAST('{0}' AS DECIMAL({1},{2}))", search, column.MaxLength, column.NumericScale)));
			}
		}

		private static void AppendIntPart(string search, StringBuilder builder, bool startWithAnd, bool exactMatch)
		{
			search = search.Replace(",", "");
			if (!exactMatch && !string.IsNullOrEmpty(search))
			{
				search = "%" + search + "%";
				builder.Append(string.IsNullOrWhiteSpace(search) ? "" : ((startWithAnd ? " AND " : "") + $" CAST(V.VALUE AS CHAR(4000)) LIKE '{search}'"));
			}
			else
			{
				builder.Append(string.IsNullOrWhiteSpace(search) ? "" : ((startWithAnd ? " AND " : "") + $" CAST(V.VALUE AS CHAR(4000)) = '{search}'"));
			}
		}

		private static void AppendStringPart(string search, StringBuilder builder, bool startWithAnd, bool exactMatch)
		{
			if (!exactMatch && !string.IsNullOrEmpty(search))
			{
				search = "%" + search + "%";
			}
			builder.Append(string.IsNullOrWhiteSpace(search) ? "" : ((startWithAnd ? " AND " : "") + (exactMatch ? $" CAST(V.VALUE AS CHAR(4000)) = '{search}'" : $" CAST(V.VALUE AS CHAR(4000)) LIKE '{search}'")));
		}

		private static void AppendTimePart(string search, StringBuilder builder, bool startWithAnd, bool exactMatch)
		{
			if (!exactMatch && !string.IsNullOrEmpty(search))
			{
				search = "%" + search + "%";
			}
			builder.Append(string.IsNullOrWhiteSpace(search) ? "" : ((startWithAnd ? " AND " : "") + (exactMatch ? $" CAST(V.VALUE AS CHAR(4000)) = '{search}'" : $" CAST(V.VALUE AS CHAR(4000)) LIKE '{search}'")));
		}

		private static void AppendDateFilter(string search, StringBuilder builder, bool startWithAnd, bool exactMatch, bool isDateTime)
		{
			List<string> dateParts = new List<string>();
			if (search.IndexOf("/", StringComparison.InvariantCultureIgnoreCase) > 0)
			{
				dateParts = new List<string>(search.Split(new string[1] { "/" }, StringSplitOptions.RemoveEmptyEntries));
			}
			else if (search.IndexOf("-", StringComparison.InvariantCultureIgnoreCase) > 0)
			{
				dateParts = new List<string>(search.Split(new string[1] { "-" }, StringSplitOptions.RemoveEmptyEntries));
			}
			else if (search.IndexOf(".", StringComparison.InvariantCultureIgnoreCase) > 0)
			{
				dateParts = new List<string>(search.Split(new string[1] { "." }, StringSplitOptions.RemoveEmptyEntries));
			}
			if (!exactMatch)
			{
				if (dateParts.Count > 0)
				{
					if (startWithAnd)
					{
						builder.Append(" AND ");
					}
					builder.Append(" ( ");
					int i = 0;
					foreach (string datePart in dateParts)
					{
						if (i != 0)
						{
							builder.Append(" AND ");
						}
						string searchFor2 = "%" + datePart + "%";
						builder.Append(string.IsNullOrWhiteSpace(datePart) ? "1 = 1" : $" CAST(V.VALUE AS CHAR(4000)) LIKE '{searchFor2}'");
						i++;
					}
					builder.Append(" ) ");
				}
				else
				{
					if (startWithAnd)
					{
						builder.Append(" AND ");
					}
					string searchFor = "%" + search + "%";
					builder.Append(string.IsNullOrWhiteSpace(search) ? "1 = 1" : $" CAST(V.VALUE AS CHAR(4000)) LIKE '{searchFor}'");
				}
				return;
			}
			bool parsed = true;
			if (!DateTime.TryParse(search, out var valueDateCheck) && !DateTime.TryParseExact(search, "d", CultureInfo.InvariantCulture, DateTimeStyles.None, out valueDateCheck))
			{
				parsed = false;
			}
			if (parsed)
			{
				if (startWithAnd)
				{
					builder.Append(" AND ");
				}
				builder.Append(string.Format("V.VALUE = CAST('{0}' AS DATETIME)", isDateTime ? valueDateCheck.ToString("u") : valueDateCheck.ToString("yyyy/MM/dd")));
			}
			else if (!startWithAnd)
			{
				builder.Append("1 = 1");
			}
		}

		public string GetExtendedColumnInformation(string value, int parentColumnId, int childColumnId, int informationColumnId, IOptimization opt, string countryCode)
		{
			if (!_isInitialized)
			{
				_log.Error("IndexDb not initialized");
				return null;
			}
			if (value == null)
			{
				_log.Error("Parameter error. Value can not be null");
				return null;
			}
			if (childColumnId < 1)
			{
				_log.Error("Parameter error. childColumnId can not be less than 1");
				return null;
			}
			if (opt == null)
			{
				_log.Error("Parameter error. Optimization can not be null.");
				return null;
			}
			IColumn childColumn = _systemDb.Columns.FirstOrDefault((IColumn itm) => itm.Id == childColumnId);
			IColumn informationColumn = _systemDb.Columns.FirstOrDefault((IColumn itm) => itm.Id == informationColumnId);
			if (childColumn == null)
			{
				_log.Error("childColumn can not be null.");
				return null;
			}
			if (informationColumn == null)
			{
				_log.Error("informationColumn can not be null.");
				return null;
			}
			using DatabaseBase connection = _viewBoxDb.ConnectionManager.GetConnection();
			using IDbCommand cmd = connection.GetDbCommand();
			IDbConnection dbConnection2 = (cmd.Connection = connection.GetDbConnection());
			using (dbConnection2)
			{
				cmd.Connection.ConnectionString = connection.ConnectionString;
				cmd.Connection.Open();
				if (childColumn.Table.Columns.Contains("SPRAS"))
				{
					string sapCode = string.Empty;
					try
					{
						sapCode = SapLanguageCodeHelper.GetSapLanguageCode(countryCode);
					}
					catch (Exception ex)
					{
						_log.Error(ex.Message);
						sapCode = "D";
					}
					cmd.CommandText = $"SELECT {informationColumn.Name} FROM {connection.Enquote(childColumn.Table.Database, childColumn.Table.TableName)} WHERE {connection.Enquote(childColumn.Name)}=@value ORDER BY IF(`SPRAS`='{sapCode}',0,1) LIMIT 1";
				}
				else
				{
					cmd.CommandText = $"SELECT {informationColumn.Name} FROM {connection.Enquote(childColumn.Table.Database, childColumn.Table.TableName)} WHERE {connection.Enquote(childColumn.Name)}=@value LIMIT 1";
				}
				IDbDataParameter pValue = cmd.CreateParameter();
				pValue.ParameterName = "@value";
				pValue.Value = value;
				cmd.Parameters.Add(pValue);
				using IDataReader rdr = cmd.ExecuteReader();
				if (!rdr.Read())
				{
					_log.Log(LogLevelEnum.Debug, "No data at GetExtendedColumnInformation");
					return null;
				}
				if (informationColumn.DataType == SqlType.Date || informationColumn.DataType == SqlType.DateTime)
				{
					return rdr.GetValue(0).ToString();
				}
				return rdr.GetString(0);
			}
		}
	}
}
