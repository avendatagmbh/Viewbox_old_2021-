using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using SystemDb;
using SystemDb.Factories;
using SystemDb.Internal;
using AV.Log;
using DbAccess;
using DbAccess.Enums;
using DbAccess.Structures;
using log4net;
using MySql.Data.MySqlClient;
using ViewboxDb.Filters;
using ViewboxDb.RowNoFilters;

namespace ViewboxDb
{
	public class ViewboxDb : IDisposable
	{
		public delegate void CopyTableDataTransformation(DatabaseBase connection, string database, string fromTableName, string toTableName);

		public delegate void ProcedureTransformation(DatabaseBase connection, string commandText, Dictionary<string, object> cmdParams, IUser user);

		public delegate void ReaderExport(DatabaseBase connection, ExportType type, ITableObjectCollection objects, List<string> sqlList, List<List<IColumn>> columnsList, CancellationToken token, ILanguage language, IOptimization opt, IUser user, string exportDescription, string fileName = "", TableObjectCollection tempTableObjects = null, IEnumerable<IOptimization> optimization = null);

		internal ILog _log = LogHelper.GetLogger();

		public static bool UseNewIssueMethod;

		private global::SystemDb.SystemDb _systemDb;

		private TempDb _tempDatabase;

		private IndexDb _indexDatabase;

		public const string COLUMN_ROW_NO = "_row_no_";

		public const string ID_COLUMN_NAME = "id";

		private const string MANDT_MAGIC_NO = "713";

		private const string BUKRS_MAGIC_NO = "4713";

		private const string GJAHR_MAGIC_NO = "4713";

		private string _connectionString;

		private string _provider;

		private ConnectionManager _cmanager;

		public global::SystemDb.SystemDb SystemDb => _systemDb;

		public TempDb TempDatabase => _tempDatabase;

		public IndexDb IndexDatabase => _indexDatabase;

		public string ConnectionString => _connectionString;

		public string DataProvider => _provider;

		public ConnectionManager ConnectionManager => _cmanager;

		public event EventHandler ConnectionEstablished;

		public event EventHandler ViewboxDbInitialized;

		public ViewboxDb(string tempdb_name, string indexdb_postfix)
		{
			_systemDb = new global::SystemDb.SystemDb();
			_tempDatabase = new TempDb(_systemDb, tempdb_name);
			_systemDb.SystemDbInitialized += SystemDbInitialized;
			_indexDatabase = new IndexDb(_systemDb, this, _tempDatabase, indexdb_postfix);
		}

		public ViewboxDb(string tempdb_name, string indexdb_postfix, global::SystemDb.SystemDb systemDb)
		{
			_systemDb = systemDb;
			_tempDatabase = new TempDb(_systemDb, tempdb_name);
			_indexDatabase = new IndexDb(_systemDb, this, _tempDatabase, indexdb_postfix);
		}

		private void SystemDbInitialized(object sender, EventArgs e)
		{
			if (this.ViewboxDbInitialized != null)
			{
				this.ViewboxDbInitialized(this, EventArgs.Empty);
			}
		}

		public void Connect(string provider, string connectionString)
		{
			DbConfig cfg = new DbConfig
			{
				ConnectionString = connectionString
			};
			using (DatabaseBase db = ConnectionManager.CreateConnection(new DbConfig
			{
				ConnectionString = connectionString,
				DbName = string.Empty
			}))
			{
				db.Open();
				if (!db.CheckDataBaseIfExists(cfg.DbName))
				{
					throw new Exception($"Database '{cfg.DbName}' does not exists");
				}
			}
			_provider = provider;
			_connectionString = connectionString;
			_cmanager = new ConnectionManager(cfg, 100);
			_cmanager.PropertyChanged += cmanager_PropertyChanged;
			if (!_cmanager.IsInitialized)
			{
				_cmanager.Init();
			}
			if (!int.TryParse(ConfigurationManager.AppSettings["MaxDatabaseConnection"], out var maxConnection))
			{
				maxConnection = 100;
			}
			_systemDb.Connect(connectionString, maxConnection);
			_tempDatabase.Connect();
			_indexDatabase.Connect();
		}

		private void cmanager_PropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			if (e.PropertyName == "ConnectionState" && _cmanager.ConnectionState == ConnectionStates.Online && this.ConnectionEstablished != null)
			{
				this.ConnectionEstablished(this, EventArgs.Empty);
			}
		}

		public void Dispose()
		{
			if (_cmanager != null)
			{
				_cmanager.Dispose();
			}
		}

		public static IOptimization GetOptimizationForType(IOptimization opt, OptimizationType type)
		{
			if (opt.Id == 0)
			{
				return null;
			}
			if (opt.Group.Type != type)
			{
				return GetOptimizationForType(opt.Parent, type);
			}
			return opt;
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

		public void UpdateArchiveData(IArchive table, DbColumnValues columns, string filename)
		{
			using DatabaseBase connection = _cmanager.GetConnection();
			connection.Update(table.Database, table.TableName, columns, filename);
		}

		public DataTable GetArchiveData(IArchive table, IOptimization opt, TableObject obj = null, long start = 0L, int sortColumnId = 0, SortDirection direction = SortDirection.Ascending)
		{
			DatabaseBase connection = _cmanager.GetConnection();
			try
			{
				string cols = string.Join(", ", table.Columns.Select((IColumn c) => connection.Enquote(c.Name)));
				if (cols.Length > 0 && !cols.Contains("_row_no_"))
				{
					cols = "_row_no_, " + cols;
				}
				else if (!cols.Contains("_row_no_"))
				{
					cols = "_row_no_";
				}
				int limit = 35;
				string indexValue = opt.FindValue(OptimizationType.IndexTable);
				string splitValue = opt.FindValue(OptimizationType.SplitTable);
				string sortValue = opt.FindValue(OptimizationType.SortColumn);
				List<Tuple<long, long>> areas = new List<Tuple<long, long>>(from o in table.OrderAreas.GetRanges(null, indexValue, splitValue, sortValue)
					select (o));
				long count = 0L;
				long skip = start;
				List<string> limits = new List<string>();
				foreach (Tuple<long, long> o2 in areas)
				{
					if (limits.Count == 0)
					{
						skip -= o2.Item2 - o2.Item1 + 1;
						if (skip > 0)
						{
							start = skip;
							continue;
						}
					}
					else
					{
						start = 0L;
					}
					long size = Math.Min(limit - count, o2.Item2 - o2.Item1 + 1 - start);
					limits.Add(string.Format("{0} BETWEEN {1} AND {2}", connection.Enquote("_row_no_"), o2.Item1 + start, o2.Item1 + size - 1 + start));
					if ((count += size) == limit)
					{
						break;
					}
				}
				if (areas.Count > 0)
				{
					start = 0L;
				}
				string filter = string.Empty;
				if (limits.Count > 0)
				{
					filter = string.Format(" WHERE ({0})", string.Join(" OR ", limits));
				}
				if (obj != null && obj.Filter != null)
				{
					filter += (filter.Contains("WHERE") ? $" AND ({obj.Filter.ToString(connection)})" : $"WHERE ({obj.Filter.ToString(connection)})");
				}
				string orderBy = string.Empty;
				if (sortColumnId > 0)
				{
					foreach (IColumn col in table.Columns)
					{
						if (!(col.Name == "path") && !(col.Name == "thumbnail") && col.Id == sortColumnId)
						{
							orderBy += col.Name;
							orderBy += ((direction == SortDirection.Ascending) ? " ASC" : " DESC");
							break;
						}
					}
				}
				if (orderBy.Length > 0)
				{
					orderBy = " ORDER BY " + orderBy;
				}
				string sql = "SELECT " + cols + " FROM " + connection.Enquote(table.Database, table.TableName) + filter + orderBy;
				sql += $" LIMIT {start}, {limit} ";
				return connection.GetDataTable(sql);
			}
			finally
			{
				if (connection != null)
				{
					((IDisposable)connection).Dispose();
				}
			}
		}

		public long GetArchiveDataRowCount(string lastSql)
		{
			using DatabaseBase connection = _cmanager.GetConnection();
			string sql = $"select count(*) from ({lastSql}) as tt";
			string sqlString = GetSimpleQuery(sql);
			return (long)connection.ExecuteScalar(sqlString);
		}

		public DataTable GetArchiveData(TableObject table, IOptimization opt, ref string sqlQuery, long start = 0L)
		{
			DatabaseBase connection = _cmanager.GetConnection();
			try
			{
				string cols = string.Join(", ", table.OriginalTable.Columns.Select((IColumn c) => connection.Enquote(c.Name)));
				if (cols.Length > 0 && !cols.Contains("_row_no_"))
				{
					cols = "_row_no_, " + cols;
				}
				else if (!cols.Contains("_row_no_"))
				{
					cols = "_row_no_";
				}
				int limit = 35;
				string indexValue = opt.FindValue(OptimizationType.IndexTable);
				string splitValue = opt.FindValue(OptimizationType.SplitTable);
				string sortValue = opt.FindValue(OptimizationType.SortColumn);
				List<Tuple<long, long>> areas = new List<Tuple<long, long>>(from o in table.Table.OrderAreas.GetRanges(null, indexValue, splitValue, sortValue)
					select (o));
				string filter = string.Empty;
				if (areas.Count > 0)
				{
					filter = " WHERE ( ";
					int count = 0;
					foreach (Tuple<long, long> areasItem in areas)
					{
						count++;
						filter = ((count != 1) ? (filter + string.Format("OR ({0} BETWEEN {1} AND {2}) ", "_row_no_", areasItem.Item1, areasItem.Item2)) : (filter + string.Format("({0} BETWEEN {1} AND {2}) ", "_row_no_", areasItem.Item1, areasItem.Item2)));
					}
					filter += ") ";
				}
				string orderBy = string.Empty;
				if (table.Sort != null && table.Sort.Count == 1)
				{
					foreach (IColumn col in table.OriginalTable.Columns)
					{
						if (!(col.Name == "path") && !(col.Name == "thumbnail"))
						{
							Sort first = table.Sort.FirstOrDefault();
							if (col.Id == first.cid)
							{
								orderBy += connection.Enquote(col.Name);
								orderBy += ((first.dir == SortDirection.Ascending) ? " ASC" : " DESC");
								break;
							}
						}
					}
				}
				if (orderBy.Length > 0)
				{
					orderBy = " ORDER BY " + orderBy;
				}
				StringBuilder sql = new StringBuilder("SELECT ");
				sql.Append(cols);
				sql.Append(" FROM ");
				sql.Append(connection.Enquote(table.OriginalTable.Database, table.OriginalTable.TableName));
				sql.Append(" as t");
				if (table.Table != null && table.Table.PageSystem != null)
				{
					if (filter != string.Empty)
					{
						sql.Append(filter + " AND ");
					}
					else
					{
						sql.Append(" WHERE ");
					}
					sql.Append(table.Table.PageSystem.Filter);
					if (!string.IsNullOrWhiteSpace(table.Table.PageSystem.OrderBy) && string.IsNullOrWhiteSpace(orderBy))
					{
						orderBy = $" ORDER BY {table.Table.PageSystem.OrderBy} ";
					}
				}
				else if (filter != string.Empty)
				{
					sql.Append(filter);
				}
				sqlQuery = sql.ToString();
				sql.Append(orderBy);
				sql.Append($" LIMIT {start}, {limit}");
				return connection.GetDataTable(GetSimpleQuery(sql.ToString()));
			}
			finally
			{
				if (connection != null)
				{
					((IDisposable)connection).Dispose();
				}
			}
		}

		private string GetSimpleQuery(string sql)
		{
			return Regex.Replace(Regex.Replace(Regex.Replace(Regex.Replace(Regex.Replace(Regex.Replace(Regex.Replace(Regex.Replace(sql.Replace("\r\n", " ").Replace("\r", " ").Replace("\n", " ")
				.Replace("\t", " "), "null is null", "true", RegexOptions.IgnoreCase), "'' is null", "false", RegexOptions.IgnoreCase), "ifnull\\('', ''\\) = ''", "true", RegexOptions.IgnoreCase), "''=''", "true", RegexOptions.IgnoreCase), "'' = ''", "true", RegexOptions.IgnoreCase), "false or ", "", RegexOptions.IgnoreCase), "true and ", "", RegexOptions.IgnoreCase), " or false", "", RegexOptions.IgnoreCase);
		}

		public string PrepareArchiveDataParameters(string[] parameters, bool exactmatch, string database)
		{
			global::ViewboxDb.Filters.AndFilter filter = new global::ViewboxDb.Filters.AndFilter();
			IArchive archive = SystemDb.Archives.FirstOrDefault((IArchive a) => string.CompareOrdinal(a.Database.ToLower(), database.ToLower()) == 0);
			if (archive != null)
			{
				using DatabaseBase connection = SystemDb.ConnectionManager.GetConnection();
				int i = 0;
				foreach (ColumnFilters columnFilters in connection.DbMapping.Load<ColumnFilters>("table_id = " + archive.Id))
				{
					if (!string.IsNullOrEmpty(parameters[i]))
					{
						IColumn filterColumn = SystemDb.Columns.FirstOrDefault((IColumn w) => w.Id == columnFilters.ColumnId);
						if (filterColumn != null)
						{
							if (filterColumn.DataType == SqlType.Decimal && !string.IsNullOrEmpty(parameters[i]) && !string.IsNullOrWhiteSpace(parameters[i]))
							{
								parameters[i] = parameters[i].Replace(',', '.');
							}
							filter.AddCondition(new global::ViewboxDb.Filters.ColValueFilter(filterColumn, (!exactmatch) ? Operators.Like : Operators.Equal, (exactmatch ? "" : "%") + parameters[i] + (exactmatch ? "" : "%")));
						}
					}
					i++;
				}
			}
			return filter.ToOriginalString();
		}

		public string GetSingleArchiveValue(IArchive archive, string columnName)
		{
			using DatabaseBase connection = _cmanager.GetConnection();
			string sql = "SELECT " + columnName + " FROM " + connection.Enquote(archive.Database, archive.TableName);
			string path = string.Empty;
			using (IDataReader reader = connection.ExecuteReader(sql))
			{
				while (reader.Read())
				{
					path = reader[columnName].ToString();
				}
			}
			return path;
		}

		public void ArchiveTable(ITableObject table, ArchiveType archive, CancellationToken token)
		{
			using DatabaseBase connection = _cmanager.GetConnection();
			try
			{
				connection.BeginTransaction();
				if (connection.TableExists(table.Database, table.TableName + "_archive"))
				{
					connection.DropTable(table.Database, table.TableName + "_archive");
				}
				token.ThrowIfCancellationRequested();
				if (archive == ArchiveType.Archive)
				{
					string createString2 = connection.GetCreateTableStructure(table.Database, table.TableName);
					SystemDb.UpdateTableObjectCreateStructure(table, createString2);
					connection.CopyTableStructure(table.Database, table.TableName, table.TableName + "_archive");
					foreach (string index in connection.GetIndexNames(table.Database, table.TableName + "_archive"))
					{
						if (!index.ToLower().Equals("PRIMARY".ToLower()))
						{
							connection.DropTableIndex(table.Database, table.TableName + "_archive", index);
						}
					}
					token.ThrowIfCancellationRequested();
				}
				else
				{
					string createTmpString = connection.GetCreateTableStructure(table.Database, table.TableName);
					SystemDb.UpdateTableObjectCreateStructure(table, createTmpString);
					connection.RenameTable(table.Database, table.TableName, table.TableName + "_temp");
					string createString = SystemDb.GetCreationString(table);
					connection.ExecuteNonQuery(createString);
					if (!connection.TableExists(table.Database, table.TableName))
					{
						throw new ArgumentException("Table was not created");
					}
					connection.RenameTable(table.Database, table.TableName, table.TableName + "_archive");
					connection.RenameTable(table.Database, table.TableName + "_temp", table.TableName);
					token.ThrowIfCancellationRequested();
				}
				if (!connection.TableExists(table.Database, table.TableName + "_archive"))
				{
					throw new ArgumentException("Table was not created");
				}
				string engine = ((archive == ArchiveType.Archive) ? "archive" : "MyISAM");
				connection.AlterTableEngine(table.Database, table.TableName + "_archive", engine);
				token.ThrowIfCancellationRequested();
				connection.CopyTableData(table.Database, table.TableName, table.TableName + "_archive");
				token.ThrowIfCancellationRequested();
				if (connection.GetRowCount(table.Database, table.TableName) != connection.GetRowCount(table.Database, table.TableName + "_archive"))
				{
					throw new DBConcurrencyException("Row count of the new table doesn't match the row count of the old table.");
				}
				connection.DropTable(table.Database, table.TableName);
				connection.RenameTable(table.Database, table.TableName + "_archive", table.TableName);
				connection.CommitTransaction();
				table.IsUnderArchiving = false;
			}
			catch (Exception)
			{
				CleanUpArchiving(table);
				connection.RollbackTransaction();
				throw;
			}
		}

		public void ArchiveTable(ITableObject table, ArchiveType archive)
		{
			ArchiveTable(table, archive, default(CancellationToken));
		}

		public void CleanUpArchiving(ITableObject table)
		{
			using DatabaseBase connection = _cmanager.GetConnection();
			if (connection.TableExists(table.Database, table.TableName + "_archive"))
			{
				connection.DropTable(table.Database, table.TableName + "_archive");
			}
			if (connection.TableExists(table.Database, table.TableName + "_temp") && !connection.TableExists(table.Database, table.TableName))
			{
				connection.RenameTable(table.Database, table.TableName + "_temp", table.TableName);
			}
		}

		public string DecryptArchivePassword(IArchive archive, string password, string key)
		{
			byte[] keyArray = Encoding.UTF8.GetBytes(key);
			byte[] toEncryptArray = Convert.FromBase64String(password);
			TripleDESCryptoServiceProvider obj = new TripleDESCryptoServiceProvider
			{
				Key = keyArray,
				Mode = CipherMode.ECB,
				Padding = PaddingMode.PKCS7
			};
			byte[] resultArray = obj.CreateDecryptor().TransformFinalBlock(toEncryptArray, 0, toEncryptArray.Length);
			obj.Clear();
			return Encoding.UTF8.GetString(resultArray);
		}

		public string EncryptArchivePassword(string password, string key)
		{
			byte[] keyArray = Encoding.UTF8.GetBytes(key);
			byte[] toEncryptArray = Encoding.UTF8.GetBytes(password);
			TripleDESCryptoServiceProvider obj = new TripleDESCryptoServiceProvider
			{
				Key = keyArray,
				Mode = CipherMode.ECB,
				Padding = PaddingMode.PKCS7
			};
			byte[] resultArray = obj.CreateEncryptor().TransformFinalBlock(toEncryptArray, 0, toEncryptArray.Length);
			obj.Clear();
			return Convert.ToBase64String(resultArray, 0, resultArray.Length);
		}

		public void ExecuteIssueOld(ProcedureTransformation func, IIssue issue, IOptimization opt, IUser user, object[] parameters)
		{
			using DatabaseBase db = _cmanager.GetConnection();
			Dictionary<string, object> cmdParams = new Dictionary<string, object>();
			for (int i = 0; i < issue.Parameters.Count; i++)
			{
				string pName = "@" + issue.Parameters.At(i).Name;
				object pValue = null;
				switch (issue.Parameters.At(i).DataType)
				{
				case SqlType.String:
					pValue = (string.IsNullOrEmpty(parameters[i].ToString()) ? null : parameters[i].ToString());
					break;
				case SqlType.Boolean:
				{
					pValue = ((!bool.TryParse(parameters[i].ToString(), out var boolOutput)) ? null : ((object)(boolOutput ? 1 : 0)));
					break;
				}
				case SqlType.Integer:
				{
					pValue = ((!long.TryParse(parameters[i].ToString(), out var intOutput)) ? null : ((object)intOutput));
					break;
				}
				case SqlType.Decimal:
				{
					pValue = ((!double.TryParse(parameters[i].ToString(), out var doubleOutput)) ? null : ((object)doubleOutput));
					break;
				}
				case SqlType.Numeric:
				{
					pValue = ((!decimal.TryParse(parameters[i].ToString(), out var decimalOutput)) ? null : ((object)decimalOutput));
					break;
				}
				case SqlType.Date:
				case SqlType.Time:
				{
					pValue = ((!DateTime.TryParse(parameters[i].ToString(), out var dateOutput)) ? null : dateOutput.ToString("yyyy-MM-dd"));
					break;
				}
				case SqlType.DateTime:
				{
					pValue = ((!DateTime.TryParse(parameters[i].ToString(), out var dateTimeOutput)) ? null : dateTimeOutput.ToString("yyyy-MM-dd"));
					break;
				}
				}
				cmdParams.Add(pName, pValue);
			}
			bool index = HasOptGroup(opt, OptimizationType.IndexTable);
			bool split = HasOptGroup(opt, OptimizationType.SplitTable);
			bool sort = HasOptGroup(opt, OptimizationType.SortColumn);
			string opts = string.Empty;
			if (issue.UseIndexValue)
			{
				if (cmdParams.Count > 0)
				{
					opts += ", ";
				}
				string indexValue = GetOptimizationValue(opt, OptimizationType.IndexTable);
				opts = ((!index) ? (opts + "'713'") : (opts + ((indexValue != null) ? ("'" + indexValue + "'") : "'-1'")));
			}
			if (issue.UseSplitValue)
			{
				if (cmdParams.Count > 0 || !string.IsNullOrEmpty(opts))
				{
					opts += ", ";
				}
				string splitValue = GetOptimizationValue(opt, OptimizationType.SplitTable);
				opts = ((!split) ? (opts + "'4713'") : (opts + ((splitValue != null) ? ("'" + splitValue + "'") : "'-1'")));
			}
			if (issue.UseSortValue)
			{
				if (cmdParams.Count > 0 || !string.IsNullOrEmpty(opts))
				{
					opts += ", ";
				}
				string sortValue = GetOptimizationValue(opt, OptimizationType.SortColumn);
				opts = ((!sort) ? (opts + "'4713'") : (opts + ((sortValue != null) ? ("'" + sortValue + "'") : "'-1'")));
			}
			string commandText = string.Format("CALL {0}({1}{2})", db.Enquote(issue.Database, issue.Command), string.Join(",", cmdParams.Select(delegate(KeyValuePair<string, object> p)
			{
				KeyValuePair<string, object> keyValuePair = p;
				return keyValuePair.Key;
			})), opts);
			func(db, commandText, cmdParams, user);
		}

		public void ExecuteIssue(ProcedureTransformation func, IIssue issue, IOptimization opt, IUser user, object[] parameters, List<Tuple<OptimizationType, string>> AllowedOpts)
		{
			Dictionary<string, object> cmdParams = new Dictionary<string, object>();
			IOrderedEnumerable<IParameter> _parameters = issue.Parameters.OrderBy((IParameter x) => x.Ordinal);
			for (int i = 0; i < issue.Parameters.Count; i++)
			{
				IParameter param = _parameters.ElementAt(i);
				if (param == null)
				{
					continue;
				}
				string pName = "@" + param.Name;
				object pValue = "";
				if (param.FreeSelection == 0)
				{
					pValue = null;
					switch (param.DataType)
					{
					case SqlType.String:
					case SqlType.Binary:
						pValue = (string.IsNullOrEmpty(parameters[param.OriginalOrdinal].ToString()) ? string.Empty : parameters[param.OriginalOrdinal].ToString());
						break;
					case SqlType.Boolean:
						pValue = parameters[param.OriginalOrdinal].ToString();
						break;
					case SqlType.Integer:
					{
						if (long.TryParse(parameters[param.OriginalOrdinal].ToString(), out var intOutput))
						{
							pValue = intOutput;
						}
						break;
					}
					case SqlType.Decimal:
					{
						if (double.TryParse(parameters[param.OriginalOrdinal].ToString(), out var _))
						{
							pValue = parameters[param.OriginalOrdinal];
						}
						break;
					}
					case SqlType.Numeric:
					{
						if (decimal.TryParse(parameters[param.OriginalOrdinal].ToString(), out var _))
						{
							pValue = parameters[param.OriginalOrdinal];
						}
						break;
					}
					case SqlType.Date:
					case SqlType.Time:
					{
						if (DateTime.TryParse(parameters[param.OriginalOrdinal].ToString(), out var dateOutput))
						{
							pValue = dateOutput.ToString("yyyy-MM-dd");
						}
						break;
					}
					case SqlType.DateTime:
					{
						if (DateTime.TryParse(parameters[param.OriginalOrdinal].ToString(), out var dateTimeOutput))
						{
							pValue = dateTimeOutput.ToString("yyyy-MM-dd");
						}
						break;
					}
					}
				}
				else
				{
					pValue = parameters[param.OriginalOrdinal].ToString();
				}
				cmdParams.Add(pName, pValue);
			}
			bool index = HasOptGroup(opt, OptimizationType.IndexTable);
			bool split = HasOptGroup(opt, OptimizationType.SplitTable);
			bool sort = HasOptGroup(opt, OptimizationType.SortColumn);
			string opts = string.Empty;
			if (issue.UseLanguageValue)
			{
				if (cmdParams.Count > 0)
				{
					opts += ", ";
				}
				string languageValue = user.CurrentLanguage;
				opts += ((languageValue != null) ? ("'" + LanguageKeyTransformer.Transformer(languageValue) + "'") : "'D'");
			}
			if (issue.UseIndexValue)
			{
				if (cmdParams.Count > 0)
				{
					opts += ", ";
				}
				string indexValue = GetOptimizationValue(opt, OptimizationType.IndexTable);
				opts = ((!index) ? (opts + "'713'") : (opts + ((indexValue != null) ? ("'" + indexValue + "'") : "'-1'")));
			}
			if (issue.UseSplitValue)
			{
				if (!string.IsNullOrEmpty(opts) || cmdParams.Count > 0)
				{
					opts += ", ";
				}
				string splitValue = GetOptimizationValue(opt, OptimizationType.SplitTable);
				opts = ((!split) ? (opts + "'4713'") : (opts + ((splitValue != null) ? ("'" + splitValue + "'") : "'-1'")));
			}
			if (issue.UseSortValue)
			{
				if (!string.IsNullOrEmpty(opts) || cmdParams.Count > 0)
				{
					opts += ", ";
				}
				string sortValue = GetOptimizationValue(opt, OptimizationType.SortColumn);
				opts = ((!sort) ? (opts + "'4713'") : (opts + ((sortValue != null) ? ("'" + sortValue + "'") : "'-1'")));
			}
			bool enable = false;
			ICoreProperty icp = SystemDb.GetCoreProperties().FirstOrDefault((ICoreProperty x) => x.Key == "OptRoleFilter");
			if (AllowedOpts != null && issue.Parameters.Any((IParameter x) => x.OptimizationType != OptimizationType.None && x.OptimizationType != OptimizationType.NotSet))
			{
				if (icp != null && icp.Value != null && bool.TryParse(icp.Value.ToString(), out enable) && enable)
				{
					if (!string.IsNullOrEmpty(opts) || cmdParams.Count > 0)
					{
						opts += ", ";
					}
					IParameter bukrsFrom = issue.Parameters.FirstOrDefault((IParameter x) => x.OptimizationType == OptimizationType.SplitTable && x.OptimizationDirection == OptimizationDirection.From);
					IParameter bukrsTo = issue.Parameters.FirstOrDefault((IParameter x) => x.OptimizationType == OptimizationType.SplitTable && x.OptimizationDirection == OptimizationDirection.To);
					IParameter bukrs = issue.Parameters.FirstOrDefault((IParameter x) => x.OptimizationType == OptimizationType.SplitTable && x.OptimizationDirection == OptimizationDirection.NotSet);
					IParameter gJahrFrom = issue.Parameters.FirstOrDefault((IParameter x) => x.OptimizationType == OptimizationType.SortColumn && x.OptimizationDirection == OptimizationDirection.From);
					IParameter gJahrTo = issue.Parameters.FirstOrDefault((IParameter x) => x.OptimizationType == OptimizationType.SortColumn && x.OptimizationDirection == OptimizationDirection.To);
					IParameter gJahr = issue.Parameters.FirstOrDefault((IParameter x) => x.OptimizationType == OptimizationType.SortColumn && x.OptimizationDirection == OptimizationDirection.NotSet);
					string result = "";
					int iBuksFrom = 0;
					int iBukrsTo = 0;
					int iGjahrFrom = 0;
					int iGjahrTo = 0;
					int iBukrs = 0;
					int iGjahr = 0;
					if (bukrsFrom != null && bukrsFrom.FreeSelection == 0)
					{
						int.TryParse(parameters[bukrsFrom.OriginalOrdinal].ToString(), out iBuksFrom);
					}
					if (bukrsTo != null && bukrsTo.FreeSelection == 0)
					{
						int.TryParse(parameters[bukrsTo.OriginalOrdinal].ToString(), out iBukrsTo);
					}
					if (bukrs != null && bukrs.FreeSelection == 0)
					{
						int.TryParse(parameters[bukrs.OriginalOrdinal].ToString(), out iBukrs);
					}
					if (gJahrFrom != null && gJahrFrom.FreeSelection == 0)
					{
						int.TryParse(parameters[gJahrFrom.OriginalOrdinal].ToString(), out iGjahrFrom);
					}
					if (gJahrTo != null && gJahrTo.FreeSelection == 0)
					{
						int.TryParse(parameters[gJahrTo.OriginalOrdinal].ToString(), out iGjahrTo);
					}
					if (gJahr != null && gJahr.FreeSelection == 0)
					{
						int.TryParse(parameters[gJahr.OriginalOrdinal].ToString(), out iGjahr);
					}
					if (iBuksFrom != 0 && iBukrsTo == 0 && iBukrs == 0)
					{
						iBukrs = iBuksFrom;
						iBuksFrom = 0;
					}
					if (iGjahrFrom != 0 && iGjahrTo == 0 && iGjahr == 0)
					{
						iGjahr = iGjahrFrom;
						iGjahrFrom = 0;
					}
					string[] array = AllowedOpts.First((Tuple<OptimizationType, string> x) => x.Item1 == OptimizationType.SortColumn).Item2.Split(',');
					foreach (string item in array)
					{
						if (item.Contains(":") && item.Split(':').Count() == 2)
						{
							int.TryParse(item.Split(':')[0], out var currentBukr);
							int.TryParse(item.Split(':')[1], out var currentYear);
							if (((iBukrs == 0 && (currentBukr >= iBuksFrom || iBuksFrom == 0) && (currentBukr <= iBukrsTo || iBukrsTo == 0)) || (iBukrs != 0 && iBukrs == currentBukr) || (iBukrs == 0 && iBukrsTo == 0 && iBuksFrom == 0)) && ((iGjahr == 0 && (currentYear >= iGjahrFrom || iGjahrFrom == 0) && (currentYear <= iGjahrTo || iGjahrTo == 0)) || (iGjahr != 0 && iGjahr == currentYear) || (iGjahr == 0 && iGjahrTo == 0 && iGjahrFrom == 0)))
							{
								result = result + item + ",";
							}
						}
					}
					opts = opts + "'" + result + "'";
				}
			}
			else if (icp != null && icp.Value != null && bool.TryParse(icp.Value.ToString(), out enable) && enable && AllowedOpts.Any((Tuple<OptimizationType, string> x) => x.Item1 == OptimizationType.SortColumn))
			{
				opts = opts + ", '" + AllowedOpts.First((Tuple<OptimizationType, string> x) => x.Item1 == OptimizationType.SortColumn).Item2 + "'";
			}
			DatabaseBase db = PrepareProcedure(issue, user, GetOptimizationValue(opt, OptimizationType.System));
			string commandText = string.Format("CALL {0}({1}{2})", db.Enquote(issue.Command), string.Join(",", cmdParams.Select(delegate(KeyValuePair<string, object> p)
			{
				KeyValuePair<string, object> keyValuePair = p;
				return keyValuePair.Key;
			})), opts);
			func(db, commandText, cmdParams, user);
			db.Close();
		}

		private DatabaseBase PrepareProcedure(IIssue issue, IUser user, string dbName)
		{
			string database = user.UserTable(dbName);
			DatabaseBase connection = ConnectionManager.CreateConnection(new DbConfig
			{
				ConnectionString = _cmanager.DbConfig.ConnectionString,
				DbName = string.Empty
			});
			connection.Open();
			connection.CreateDatabaseIfNotExists(database);
			connection.ChangeDatabase(database);
			string proc = string.Empty;
			using (DatabaseBase connectionForProcDefinition = ConnectionManager.CreateConnection(new DbConfig
			{
				ConnectionString = _cmanager.DbConfig.ConnectionString,
				DbName = issue.Database
			}))
			{
				connectionForProcDefinition.Open();
				proc = connectionForProcDefinition.GetProcedureDefinition(issue.Database, issue.Command);
			}
			if (!connection.TableExists(issue.TableName))
			{
				connection.CopyTableStructure(issue.Database, issue.TableName, connection.DbConfig.DbName, issue.TableName);
			}
			if (connection.ColumnExists(database, issue.TableName, "_row_no_"))
			{
				connection.AlterAutoIncrementPrimaryColumn(database, issue.TableName, "_row_no_");
			}
			else
			{
				connection.AddAutoIncrementPrimaryColumn(database, issue.TableName, "_row_no_");
			}
			connection.DropProcedureIfExists(issue.Command);
			connection.ExecuteNonQuery(proc);
			return connection;
		}

		public string GetFilterIssue(IIssue issue, IOptimization opt, List<Tuple<IParameter, string, string>> ParameterColl, bool appendOptFilter = true)
		{
			object[] array = ParameterColl.Select((Tuple<IParameter, string, string> item) => item.Item2).ToArray();
			object[] parameters = array;
			using DatabaseBase db = _cmanager.GetConnection();
			bool index = TempDb.HasOptGroup(opt, OptimizationType.IndexTable);
			bool split = TempDb.HasOptGroup(opt, OptimizationType.SplitTable);
			bool sort = TempDb.HasOptGroup(opt, OptimizationType.SortColumn);
			string indexValue = TempDb.GetOptimizationValue(opt, OptimizationType.IndexTable);
			string splitValue = TempDb.GetOptimizationValue(opt, OptimizationType.SplitTable);
			string sortValue = TempDb.GetOptimizationValue(opt, OptimizationType.SortColumn);
			bool useOptIfParamIsEmpty = false;
			ICoreProperty icp = SystemDb.GetCoreProperties().FirstOrDefault((ICoreProperty x) => x.Key == "UseOptIfParamIsEmpty");
			if (icp != null)
			{
				bool.TryParse(icp.Value, out useOptIfParamIsEmpty);
			}
			string filter = "";
			string _rowNo = GetFilterIssueFromRowNo(issue, opt, parameters);
			if (string.IsNullOrEmpty(_rowNo))
			{
				if (issue.Parameters.Count != 0)
				{
					filter = issue.Command;
					int i = 0;
					foreach (IParameter p in issue.Parameters)
					{
						Tuple<IParameter, string, string> param = ParameterColl.FirstOrDefault((Tuple<IParameter, string, string> x) => x.Item1 == p);
						string value = param.Item2;
						if (useOptIfParamIsEmpty && p.OptimizationType != 0 && value == "" && (p.OptimizationDirection == OptimizationDirection.NotSet || ParameterColl.Any((Tuple<IParameter, string, string> x) => x.Item1.OptimizationType == p.OptimizationType && x.Item1.OptimizationDirection != p.OptimizationDirection && x.Item2 == "")))
						{
							switch (p.OptimizationType)
							{
							case OptimizationType.SplitTable:
								if (split && splitValue != null)
								{
									value = splitValue;
								}
								break;
							case OptimizationType.SortColumn:
								if (sort && sortValue != null)
								{
									value = sortValue;
								}
								break;
							case OptimizationType.IndexTable:
								if (index && indexValue != null)
								{
									value = indexValue;
								}
								break;
							}
						}
						switch (p.DataType)
						{
						case SqlType.Integer:
						case SqlType.Decimal:
						case SqlType.Numeric:
						{
							if (!decimal.TryParse(value, out var _))
							{
								value = null;
							}
							break;
						}
						case SqlType.Time:
						{
							value = ((!TimeSpan.TryParse(value, out var timeOutput)) ? null : timeOutput.ToString());
							break;
						}
						case SqlType.Date:
						{
							value = ((!DateTime.TryParse(value, out var dateOutput)) ? ((!(value == "0000-00-00")) ? null : "0000-00-00") : dateOutput.ToString("yyyy-MM-dd"));
							break;
						}
						case SqlType.DateTime:
						{
							value = ((!DateTime.TryParse(value, out var dateTimeOutput)) ? null : dateTimeOutput.ToString("yyyy-MM-dd HH:mm:ss"));
							break;
						}
						}
						if (p.FreeSelection == 1)
						{
							filter = Regex.Replace(filter, Regex.Escape(p.Name), "''", RegexOptions.IgnoreCase);
							filter = Regex.Replace(filter, Regex.Escape("IN_" + p.Name + "_FROM"), "''", RegexOptions.IgnoreCase);
							filter = Regex.Replace(filter, Regex.Escape("IN_" + p.Name + "_TO"), "''", RegexOptions.IgnoreCase);
						}
						else
						{
							Regex replacer = new Regex("\\b" + p.Name + "\\b", RegexOptions.IgnoreCase);
							if (param.Item3 == "in")
							{
								string[] inputs = value.Replace("\\;", "SEMICOLON").Split(new char[1] { ';' }, StringSplitOptions.RemoveEmptyEntries);
								for (int j = 0; j < inputs.Count(); j++)
								{
									inputs[j] = inputs[j].Replace("SEMICOLON", ";").Trim();
									inputs[j] = MySqlHelper.EscapeString(inputs[j]);
									inputs[j] = RegexSubstitutionReplacer(inputs[j]);
								}
								filter = replacer.Replace(filter, "'" + string.Join("','", inputs) + "'");
							}
							else
							{
								string replaceValue = db.GetValueString(value);
								replaceValue = RegexSubstitutionReplacer(replaceValue);
								filter = replacer.Replace(filter, replaceValue);
							}
						}
						i++;
					}
				}
			}
			else
			{
				filter = Parser.Parse(issue, _rowNo).ToConditionString(db, "t");
			}
			if (appendOptFilter)
			{
				if (splitValue != null)
				{
					filter = filter + ((filter == "") ? "" : " AND ") + db.Enquote(issue.FilterTableObject.SplitTableColumn.Name);
					filter = ((!split) ? (filter + "='4713'") : (filter + "='" + splitValue + "'"));
				}
				if (indexValue != null)
				{
					filter = filter + ((filter == "") ? "" : " AND ") + db.Enquote(issue.FilterTableObject.IndexTableColumn.Name);
					filter = ((!index) ? (filter + "='4713'") : (filter + "='" + indexValue + "'"));
				}
				if (sortValue != null)
				{
					filter = filter + ((filter == "") ? "" : " AND ") + db.Enquote(issue.FilterTableObject.SortColumn.Name);
					filter = ((!sort) ? (filter + "='4713'") : (filter + "='" + sortValue + "'"));
				}
			}
			if (filter != string.Empty)
			{
				return "(" + filter + ")";
			}
			return string.Empty;
		}

		public string GetFilterIssueValue(string value, SqlType t)
		{
			switch (t)
			{
			case SqlType.String:
				value = $"'{value}'";
				break;
			case SqlType.Integer:
			case SqlType.Decimal:
			case SqlType.Numeric:
			{
				if (!string.IsNullOrEmpty(value) && value[value.Length - 1] == ';')
				{
					value = value.Remove(value.Length - 1);
				}
				if (!decimal.TryParse(value, out var _))
				{
					value = "''";
				}
				break;
			}
			case SqlType.Time:
			{
				value = ((!TimeSpan.TryParse(value, out var timeOutput)) ? "''" : $"'{timeOutput.ToString()}'");
				break;
			}
			case SqlType.Date:
			{
				if (!string.IsNullOrEmpty(value) && value[value.Length - 1] == ';')
				{
					value = value.Remove(value.Length - 1);
				}
				value = ((!DateTime.TryParse(value, out var dateOutput)) ? ((!(value == "0000-00-00")) ? "''" : "'0000-00-00'") : string.Format("'{0}'", dateOutput.ToString("yyyy-MM-dd")));
				break;
			}
			case SqlType.DateTime:
			{
				value = ((!DateTime.TryParse(value, out var dateTimeOutput)) ? "''" : string.Format("'{0}'", dateTimeOutput.ToString("yyyy-MM-dd HH:mm:ss")));
				break;
			}
			}
			return value;
		}

		public List<string> GetSearchResultBeleg(string columnName = "", string database = "", string tableName = "", string value = "", int limit = 5, bool date = false)
		{
			StringBuilder sql = new StringBuilder();
			List<string> results = new List<string>();
			using DatabaseBase connection = _cmanager.GetConnection();
			columnName = connection.Enquote(columnName);
			sql.Append("SELECT distinct ");
			sql.Append(columnName);
			sql.Append(" FROM ");
			sql.Append(database).Append(".").Append(tableName);
			if (!date)
			{
				sql.Append(" WHERE ").Append(columnName).Append(" LIKE \"%" + value + "%\"");
			}
			else
			{
				sql.Append(" WHERE ").Append(columnName).Append(" LIKE '" + value + "'");
			}
			sql.Append(" LIMIT ").Append("0," + limit);
			using IDataReader reader = connection.ExecuteReader(sql.ToString());
			while (reader.Read())
			{
				results.Add(reader.GetString(0));
			}
			return results;
		}

		public string GetFilterIssueFromRowNo(IIssue issue, IOptimization opt, object[] parameters)
		{
			if (string.IsNullOrEmpty(issue.RowNoFilter))
			{
				return "";
			}
			using DatabaseBase db = _cmanager.GetConnection();
			string rowNoFilter = issue.RowNoFilter;
			int i = 0;
			foreach (IParameter p in issue.Parameters)
			{
				string param = parameters[i].ToString();
				switch (p.DataType)
				{
				case SqlType.Integer:
				case SqlType.Decimal:
				case SqlType.Numeric:
				{
					param = ((!decimal.TryParse(parameters[i].ToString(), out var decimalOutput)) ? null : decimalOutput.ToString(CultureInfo.InvariantCulture).Replace(",", "."));
					break;
				}
				case SqlType.Date:
				case SqlType.Time:
				{
					param = ((!DateTime.TryParse(parameters[i].ToString(), out var dateOutput)) ? null : dateOutput.ToString("yyyy-MM-dd"));
					break;
				}
				case SqlType.DateTime:
				{
					param = ((!DateTime.TryParse(parameters[i].ToString(), out var dateTimeOutput)) ? null : dateTimeOutput.ToString("yyyy-MM-dd"));
					break;
				}
				}
				IColumn column = issue.Columns.FirstOrDefault((IColumn w) => w.Name.ToLower() == p.ColumnName.ToLower());
				if (column != null)
				{
					rowNoFilter = rowNoFilter.Replace(p.Name.ToLower() + "_column", "%" + column.Id);
					rowNoFilter = rowNoFilter.Replace(p.Name.ToLower(), "\"" + db.GetValueString(param) + "\"");
					i++;
					continue;
				}
				return "";
			}
			return rowNoFilter;
		}

		public bool HasOptGroup(IOptimization opt, OptimizationType type)
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

		public DataTable LoadDataTable(string database, string table, int page, int size)
		{
			using DatabaseBase db = _cmanager.GetConnection();
			string sql = $"SELECT * FROM {db.Enquote(database)}.{db.Enquote(table)}";
			if (size > 0)
			{
				sql += $" LIMIT {page * size}, {size}";
			}
			return db.GetDataTable(sql);
		}

		public void LoadExportData(ReaderExport func, ExportType type, ITableObjectCollection objects, IOptimization opt, CancellationToken token, ILanguage language, int page, long size, IUser user, string exportDescription, string fileName, bool getAllFields, string filter = "", List<string> roleBasedOptFilterList = null, IEnumerable<IOptimization> optimization = null)
		{
			using DatabaseBase connection = TempDatabase.ConnectionManager.GetConnection();
			token.ThrowIfCancellationRequested();
			List<string> sqlList = new List<string>();
			List<List<IColumn>> columnsList = new List<List<IColumn>>();
			int i = 0;
			string[] filtersForEveryTable = filter.Split(';');
			foreach (ITableObject tobj in objects)
			{
				string filterForTobj = filtersForEveryTable.SingleOrDefault((string f) => f.StartsWith(tobj.Id.ToString()));
				string roleBasedOptFilter = "";
				if (roleBasedOptFilterList.Count > i)
				{
					roleBasedOptFilter = roleBasedOptFilterList.ElementAt(i);
				}
				if (filterForTobj != null)
				{
					sqlList.Add(TempDatabase.LoadExportData(connection, tobj, opt, page, size, user, getAllFields, filterForTobj.Substring(filterForTobj.IndexOf('(')), roleBasedOptFilter));
				}
				else
				{
					sqlList.Add(TempDatabase.LoadExportData(connection, tobj, opt, page, size, user, getAllFields, "", roleBasedOptFilter));
				}
				if (getAllFields)
				{
					columnsList.Add(new List<IColumn>(tobj.Columns.Select((IColumn c) => c)));
				}
				else
				{
					columnsList.Add(new List<IColumn>(tobj.Columns.Where((IColumn c) => !c.IsEmpty && c.IsVisible)));
				}
				i++;
			}
			func(connection, type, objects, sqlList, columnsList, token, language, opt, user, exportDescription, fileName, null, optimization);
		}

		public void LoadExportData(ReaderExport func, ExportType type, TableObjectCollection tempTableObjects, IOptimization opt, CancellationToken token, ILanguage language, int page, long size, IUser user, string exportDescription, string fileName, bool getAllFields, List<string> roleBasedOptFilterList = null, IEnumerable<IOptimization> optimization = null)
		{
			ITableObjectCollection objects = SystemDb.CreateTableObjectCollection();
			TempDatabase.LoadExportData(func, type, tempTableObjects, opt, token, language, page, size, objects, user, exportDescription, fileName, getAllFields, roleBasedOptFilterList, optimization);
		}

		public Action SetAction(string actionName, string controllerName, DateTime timestamp, IUser user, IDictionary<string, object> actionParameters, bool rightsMode, bool checkCharcode)
		{
			if (Enum.TryParse<ActionControllers>(controllerName + actionName, out var myEnum))
			{
				return new Action
				{
					ActionController = myEnum,
					Timestamp = timestamp,
					CheckCharacterCode = checkCharcode,
					ActionParameters = (actionParameters as Dictionary<string, object>),
					User = user,
					Controller = controllerName,
					Rightsmode = rightsMode
				};
			}
			return null;
		}

		public NewLogAction NewLogSetAction(string actionName, string controllerName, DateTime timestamp, IUser user, IDictionary<string, object> actionParameters, bool rightsMode, bool checkCharcode)
		{
			if (Enum.TryParse<NewLogActionControllers>(controllerName + actionName, out var myEnum))
			{
				return new NewLogAction
				{
					ActionController = myEnum,
					Timestamp = timestamp,
					CheckCharacterCode = checkCharcode,
					ActionParameters = (actionParameters as Dictionary<string, object>),
					User = user,
					Controller = controllerName,
					Rightsmode = rightsMode
				};
			}
			return null;
		}

		public void UpdateAction(Action action)
		{
			using DatabaseBase connection = _cmanager.GetConnection();
			connection.DbMapping.Save(action);
		}

		public void UpdateNewLogAction(NewLogAction action)
		{
			using DatabaseBase connection = _cmanager.GetConnection();
			connection.DbMapping.Save(action);
		}

		public NewLogActionCollection GetUserActions(int userId, DateTime date, bool showHidden)
		{
			NewLogActionCollection actionList = new NewLogActionCollection();
			NewLogActionCollection tempActionList;
			using (DatabaseBase connection = _cmanager.GetConnection())
			{
				tempActionList = new NewLogActionCollection(connection.DbMapping.Load<NewLogAction>(connection.Enquote("user_id") + "=" + userId + " AND " + connection.Enquote("timestamp") + ">='" + date.ToString("yyyy-MM-dd") + "'"));
			}
			foreach (NewLogAction newLogAction in tempActionList)
			{
				if (!showHidden)
				{
					if (SystemDb.UserUserLogSettings[SystemDb.Users[userId], newLogAction.Id] == null || (SystemDb.UserUserLogSettings[SystemDb.Users[userId], newLogAction.Id] != null && SystemDb.UserUserLogSettings[SystemDb.Users[userId], newLogAction.Id].IsVisible))
					{
						actionList.Add(newLogAction);
					}
				}
				else if (SystemDb.UserUserLogSettings[SystemDb.Users[userId], newLogAction.Id] != null && !SystemDb.UserUserLogSettings[SystemDb.Users[userId], newLogAction.Id].IsVisible)
				{
					actionList.Add(newLogAction);
				}
			}
			return actionList;
		}

		public IUser CreateUser(string userName, string name, SpecialRights flags, ExportRights exportAllowed, string password, string email, int id, bool isAdUser = false, string domain = null, bool firstLogin = false)
		{
			using DatabaseBase connection = _cmanager.GetConnection();
			return _systemDb.CreateUser(connection, userName, name, flags, exportAllowed, password, email, id, isAdUser, domain, firstLogin);
		}

		public IPassword AddPasswordToHistory(string passwordHash, int userId, DateTime creationDate)
		{
			using DatabaseBase connection = _cmanager.GetConnection();
			return _systemDb.AddPasswordToHistory(connection, passwordHash, creationDate, userId);
		}

		public void DeletePasswordFromHistory(IPassword password)
		{
			using DatabaseBase connection = _cmanager.GetConnection();
			_systemDb.DeletePasswordFromHistory(connection, password);
		}

		public IRole CreateRole(string name, SpecialRights flags, ExportRights exportAllowed, RoleType type, int id)
		{
			using DatabaseBase connection = _cmanager.GetConnection();
			return _systemDb.CreateRole(connection, name, flags, exportAllowed, type, id);
		}

		public IRole ModifyRole(string name, SpecialRights flags, ExportRights exportAllowed, RoleType type, int id)
		{
			using DatabaseBase connection = _cmanager.GetConnection();
			return _systemDb.ModifyRole(connection, name, flags, exportAllowed, type, id);
		}

		public void SaveUser(IUser user)
		{
			using DatabaseBase connection = _cmanager.GetConnection();
			connection.DbMapping.Save(user);
		}

		public int SaveIssue(ITableObject tobj, IFilter filter, string rownocommand, IEnumerable<ILanguage> languages, IUser user, DescriptionCollection descriptions, bool isLoggedAsGerman = false, List<bool> freeselection = null)
		{
			int retVal = -1;
			using DatabaseBase db = _cmanager.GetConnection();
			int i = 0;
			int j = 0;
			string command = filter.GetReplacedFilter(db, ref i, freeselection, ref j);
			List<Tuple<IFilter, string, object, IColumn>> parameters = filter.GetParameters(new List<Tuple<IFilter, string, object, IColumn>>());
			j = 0;
			List<IParameter> realParameters = new List<IParameter>();
			bool betweenFilter = false;
			int groupID = 1;
			foreach (Tuple<IFilter, string, object, IColumn> parameter in parameters)
			{
				Parameter param = new Parameter();
				param.Name = parameter.Item2;
				param.Default = "";
				param.DataType = parameter.Item4.DataType;
				param.ColumnName = parameter.Item4.Name;
				param.DatabaseName = tobj.Database;
				param.TableName = tobj.TableName;
				param.UserDefined = true;
				if (!betweenFilter)
				{
					param.FreeSelection = (freeselection[j] ? 1 : 0);
					j++;
					if (parameter.Item1 is BetweenFilter)
					{
						betweenFilter = true;
						param.GroupId = groupID;
					}
				}
				else
				{
					param.FreeSelection = (freeselection[j - 1] ? 1 : 0);
					param.GroupId = groupID++;
					betweenFilter = false;
				}
				param.LeadingZeros = 0;
				param.ColumnNameInView = parameter.Item4.Name;
				realParameters.Add(param);
			}
			Dictionary<string, List<string>> desc = new Dictionary<string, List<string>>();
			foreach (Description d in descriptions)
			{
				desc.Add(d.CountryCode, d.Descriptions);
			}
			List<IParameter> issueParameters = new List<IParameter>();
			return SystemDb.SaveIssue(db, tobj, command, rownocommand, realParameters, languages, user, desc, out issueParameters, isLoggedAsGerman ? 1 : 0, freeselection);
		}

		public void EditIssue(IIssue issue, IEnumerable<ILanguage> languages, DescriptionCollection descriptions)
		{
			using DatabaseBase db = _cmanager.GetConnection();
			Dictionary<string, List<string>> desc = new Dictionary<string, List<string>>();
			foreach (Description d in descriptions)
			{
				desc.Add(d.CountryCode, d.Descriptions);
			}
			SystemDb.EditIssue(db, issue, languages, desc);
		}

		public void DeleteIssue(IIssue tobj, IUser user)
		{
			using DatabaseBase db = _cmanager.GetConnection();
			SystemDb.DeleteIssue(db, tobj, user);
		}

		public TableObject LoadDataTable(TempDb.ReaderTransformation func, ITableObject table, IOptimization opt, List<int> colIds, AggregationCollection aggs, IFilter filter, Dictionary<string, string> descriptions, IEnumerable<ILanguage> languages, CancellationToken token, bool save, IUser user, Dictionary<Tuple<ILanguage, string>, string> aggDescriptions, string filterIssue)
		{
			string startedByUser = $"started by: {user.UserName}";
			TableObject to = TempDatabase.CreateGroupByTable(func, table, opt, colIds, aggs, filter, descriptions, languages, token, save, aggDescriptions, user, filterIssue);
			if (save && to.Table.Id < 0)
			{
				using (DatabaseBase connection = _cmanager.GetConnection())
				{
					string database = TempDb.GetOptimizationValue(opt, OptimizationType.System);
					to.Table = SystemDb.SaveView(connection, to.Table, languages, user, database);
				}
				LogHelper.GetLogger().InfoFormat("[FUNCTIONS] - Saving SQL Function temptable (`{0}`) as view(description: '{1}'){2}", to.Table.TableName, (descriptions.Count > 0) ? descriptions.Values.ElementAt(0) : string.Empty, startedByUser);
			}
			return to;
		}

		public void DeleteView(IView view, IUser user)
		{
			using DatabaseBase db = _cmanager.GetConnection();
			SystemDb.DeleteView(db, view, user);
		}

		public INote CreateNote(IUser user, string title, string text, DateTime date)
		{
			using DatabaseBase db = _cmanager.GetConnection();
			return SystemDb.CreateNote(db, user, title, text, date);
		}

		public void AddRelations(int tableId, int joinTableId, ITableObjectCollection tableObjectCollection, JoinColumnsCollection columnsCollection, ITableObject tableobject)
		{
			if (tableObjectCollection == null || tableObjectCollection.Count == 0 || columnsCollection == null || columnsCollection.Count == 0)
			{
				return;
			}
			foreach (JoinColumns joinColumn2 in columnsCollection)
			{
				if (joinColumn2.Column1 == 0)
				{
					return;
				}
				if (joinColumn2.Column1 < 0 && tableId < 0 && tableobject != null)
				{
					string dataBase = tableobject.Database;
					string tableName = tableobject.TableName;
					string columnName = tableobject.Columns.FirstOrDefault((IColumn x) => x.Id == joinColumn2.Column1).Name;
					int originalColumnId = SystemDb.Columns.FirstOrDefault((IColumn x) => x.Name == columnName && x.Table.Database == dataBase && x.Table.TableName == tableName).Id;
					joinColumn2.Column1 = originalColumnId;
				}
				if (joinColumn2.Column2 == 0)
				{
					return;
				}
			}
			lock (new object())
			{
				List<IRelationDatabaseObject> relationList = new List<IRelationDatabaseObject>();
				foreach (JoinColumns joinColumn in columnsCollection)
				{
					IRelationDatabaseObject relation = InternalObjectFactory.CreateRelation(joinColumn.Column1, joinColumn.Column2, 0);
					relationList.Add(relation);
				}
				foreach (IRelationDatabaseObject item in relationList)
				{
					item.UserDefined = true;
				}
				SystemDb.AddRelationsToDataBase(tableId, joinTableId, tableObjectCollection, relationList, tableobject);
			}
		}

		public void AddRelation(int child, int parent)
		{
			IRelationDatabaseObject relation = InternalObjectFactory.CreateRelation(child, parent, 6666);
			List<IRelationDatabaseObject> relationList = new List<IRelationDatabaseObject>();
			relationList.Add(relation);
			SystemDb.AddRelations(relationList);
		}

		public bool DoesRelationExist(int tableId, List<JoinColumns> columnsCollection, ITableObject tableobject)
		{
			if (columnsCollection == null || columnsCollection.Count == 0)
			{
				return true;
			}
			_ = tableobject.Relations;
			foreach (JoinColumns joinColumn in columnsCollection)
			{
				if (joinColumn.Column1 == 0)
				{
					return true;
				}
				if (joinColumn.Column1 < 0 && tableId < 0 && tableobject != null)
				{
					string dataBase = tableobject.Database;
					string tableName = tableobject.TableName;
					string columnName = tableobject.Columns.FirstOrDefault((IColumn x) => x.Id == joinColumn.Column1).Name;
					int originalColumnId = SystemDb.Columns.FirstOrDefault((IColumn x) => x.Name == columnName && x.Table.Database == dataBase && x.Table.TableName == tableName).Id;
					joinColumn.Column1 = originalColumnId;
				}
				if (joinColumn.Column2 == 0)
				{
					return true;
				}
			}
			ITableObject source = null;
			if (SystemDb.Columns[columnsCollection[0].Column1].Table is IIssue)
			{
				source = SystemDb.Issues[SystemDb.Columns[columnsCollection[0].Column1].Table.Id];
			}
			else if (SystemDb.Columns[columnsCollection[0].Column1].Table is IView)
			{
				source = SystemDb.Views[SystemDb.Columns[columnsCollection[0].Column1].Table.Id];
			}
			else if (SystemDb.Columns[columnsCollection[0].Column1].Table is ITable)
			{
				source = SystemDb.Tables[SystemDb.Columns[columnsCollection[0].Column1].Table.Id];
			}
			bool got_it = false;
			if (source != null)
			{
				foreach (IRelation rel in source.Relations)
				{
					if (rel.ToList().Count == columnsCollection.Count)
					{
						foreach (IColumnConnection colConn in rel)
						{
							IDataObject sourceCol = colConn.Source;
							if (columnsCollection.Find((JoinColumns j) => j.Column1 == sourceCol.Id && j.Column2 == colConn.Target.Id) == null)
							{
								got_it = false;
								break;
							}
							got_it = true;
						}
						if (got_it)
						{
							return got_it;
						}
					}
				}
				return got_it;
			}
			return got_it;
		}

		public INote UpdateNote(int id, IUser user, string title, string text)
		{
			using DatabaseBase db = _cmanager.GetConnection();
			return SystemDb.UpdateNote(db, id, user, title, text);
		}

		public void DeleteNote(int id, IUser user)
		{
			using DatabaseBase db = _cmanager.GetConnection();
			SystemDb.DeleteNote(db, id, user);
		}

		public void CreateServerLogEntry(DateTime dt)
		{
			using DatabaseBase db = _cmanager.GetConnection();
			SystemDb.CreateServerLogEntry(db, dt);
		}

		public void UpgradeDatabase()
		{
			using DatabaseBase db = _cmanager.GetConnection();
			SystemDb.UpgradeDatabase(db);
		}

		public DataTable GetArchiveDocumentData(IArchiveDocument table, long start = 0L, int sortColumnId = 0, SortDirection direction = SortDirection.Ascending)
		{
			DatabaseBase connection = _cmanager.GetConnection();
			try
			{
				string cols = string.Join(", ", table.Columns.Select((IColumn c) => connection.Enquote(c.Name)));
				cols = ((cols.Length <= 0) ? "_row_no_" : ("_row_no_, " + cols));
				string orderBy = string.Empty;
				if (sortColumnId > 0)
				{
					foreach (IColumn col in table.Columns)
					{
						if (col.Id == sortColumnId)
						{
							orderBy += col.Name;
							orderBy += ((direction == SortDirection.Ascending) ? " ASC" : " DESC");
							break;
						}
					}
				}
				if (orderBy.Length > 0)
				{
					orderBy = " ORDER BY " + orderBy;
				}
				string sql = "SELECT " + cols + " FROM " + connection.Enquote(connection.DbConfig.DbName, table.TableName) + orderBy;
				return connection.GetDataTable(sql);
			}
			finally
			{
				if (connection != null)
				{
					((IDisposable)connection).Dispose();
				}
			}
		}

		public DataTable GetArchiveDocumentData(TableObject table, long start = 0L, int sortColumnId = 0, SortDirection direction = SortDirection.Ascending)
		{
			string cols = string.Join(", ", table.OriginalTable.Columns.Select((IColumn c) => c.Name));
			cols = ((cols.Length <= 0) ? "_row_no_" : ("_row_no_, " + cols));
			using DatabaseBase connection = _cmanager.GetConnection();
			int limit = 35;
			string filter = string.Empty;
			string orderBy = string.Empty;
			if (table.Filter != null || (table.Sort != null && table.Sort.Count > 0))
			{
				filter = filter + " WHERE s.id BETWEEN " + (start + 1) + " AND " + (start + limit);
				orderBy = " ORDER BY s.id";
			}
			else
			{
				filter = filter + " WHERE _row_no_ BETWEEN " + (start + 1) + " AND " + (start + limit);
				orderBy = " ORDER BY _row_no_";
			}
			string sql = "SELECT " + cols + " FROM " + connection.Enquote(table.OriginalTable.Database, table.OriginalTable.TableName) + " as t";
			sql = sql + " INNER JOIN " + connection.Enquote(TempDatabase.DbName, table.Key) + " as s ON t._row_no_ = s._row_no_ " + filter + orderBy;
			return connection.GetDataTable(sql);
		}

		public string PrepareArchiveDataParameters(string[] parameters, bool exactmatch)
		{
			global::ViewboxDb.Filters.AndFilter filter = new global::ViewboxDb.Filters.AndFilter();
			IArchiveDocument archive = SystemDb.ArchiveDocuments.FirstOrDefault();
			if (archive != null)
			{
				using DatabaseBase connection = SystemDb.ConnectionManager.GetConnection();
				int i = 0;
				foreach (ColumnFilters columnFilters in connection.DbMapping.Load<ColumnFilters>("table_id = " + archive.Id))
				{
					if (!string.IsNullOrEmpty(parameters[i]))
					{
						IColumn filterColumn = SystemDb.Columns.FirstOrDefault((IColumn w) => w.Id == columnFilters.ColumnId);
						if (filterColumn != null)
						{
							if (filterColumn.DataType == SqlType.DateTime)
							{
								if (DateTime.TryParse(parameters[i], out var dateValue))
								{
									filter.AddCondition(new global::ViewboxDb.Filters.ColValueFilter(filterColumn, Operators.Equal, dateValue.ToString("yyyyMMddhhmmss")));
								}
							}
							else
							{
								filter.AddCondition(new global::ViewboxDb.Filters.ColValueFilter(filterColumn, (!exactmatch) ? Operators.Like : Operators.Equal, (exactmatch ? "" : "%") + parameters[i] + (exactmatch ? "" : "%")));
							}
						}
					}
					i++;
				}
			}
			return filter.ToOriginalString();
		}

		public static string RegexSubstitutionReplacer(string input)
		{
			return input.Replace("$", "$$").Replace("^", "^^");
		}
	}
}
