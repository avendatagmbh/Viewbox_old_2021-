using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using SystemDb;
using SystemDb.Internal;
using AV.Log;
using DbAccess;
using log4net;
using ViewboxDb.AggregationFunctionTranslator;
using ViewboxDb.Filters;

namespace ViewboxDb
{
	public class TableObject
	{
		internal delegate void TableObjectRemoveHandler(TableObject sender);

		internal ILog _log = LogHelper.GetLogger();

		private object _additional;

		private ITableObject _table;

		private bool _multiOptimization;

		private IDictionary<int, Tuple<int, int>> _optimizationSelected;

		private IEnumerable<IOptimization> _optimizationCollection;

		private ITableObject _originalTable;

		private IOptimization _optimization;

		private string _filterIssue;

		private SortCollection _sort;

		private IColumnCollection _originalColumns;

		private List<IColumn> _sum;

		private SubTotalParameters _groupSubTotal;

		private IFilter _filter;

		private AggregationCollection _agg;

		private List<int> _colIds;

		private Join _join;

		private string _key;

		private int _scrollPosition;

		private object[] _paramValues;

		private List<int> _itemId;

		private List<int> _selectionTypes;

		private List<string> _displayValues;

		private const string COLUMN_ROW_NO = "_row_no_";

		private const string ID_COLUMN_NAME = "id";

		public object Additional
		{
			get
			{
				return _additional;
			}
			set
			{
				if (_additional != value)
				{
					_additional = value;
				}
			}
		}

		public ITableObject Table
		{
			get
			{
				return _table;
			}
			set
			{
				if (_table != value)
				{
					_table = value;
				}
			}
		}

		private static bool HasDefaultOrderField
		{
			get
			{
				if (ConfigurationManager.AppSettings["HasDefaultOrderField"] != null)
				{
					return Convert.ToBoolean(ConfigurationManager.AppSettings["HasDefaultOrderField"]);
				}
				return true;
			}
		}

		public bool MultiOptimization
		{
			get
			{
				return _multiOptimization;
			}
			set
			{
				_multiOptimization = value;
			}
		}

		public IDictionary<int, Tuple<int, int>> OptimizationSelected
		{
			get
			{
				return _optimizationSelected;
			}
			set
			{
				_optimizationSelected = value;
			}
		}

		public int BaseTableId { get; set; }

		public int OriginalTableId { get; set; }

		public int PreviousTableId { get; set; }

		public int PreviousId { get; set; }

		public IEnumerable<IOptimization> OptimizationCollection
		{
			get
			{
				return _optimizationCollection;
			}
			set
			{
				_optimizationCollection = value;
			}
		}

		public ITableObject OriginalTable
		{
			get
			{
				return _originalTable;
			}
			set
			{
				if (_originalTable != value)
				{
					_originalTable = value;
				}
			}
		}

		public IOptimization Optimization
		{
			get
			{
				return _optimization;
			}
			set
			{
				if (_optimization != value)
				{
					_optimization = value;
				}
			}
		}

		public string FilterIssue
		{
			get
			{
				return _filterIssue;
			}
			set
			{
				if (_filterIssue != value)
				{
					_filterIssue = value;
				}
			}
		}

		public SortCollection Sort
		{
			get
			{
				return _sort;
			}
			set
			{
				if (_sort != value)
				{
					_sort = value;
				}
			}
		}

		public IColumnCollection OriginalColumns
		{
			get
			{
				return _originalColumns;
			}
			set
			{
				_originalColumns = value;
			}
		}

		public List<IColumn> Sum
		{
			get
			{
				return _sum;
			}
			set
			{
				if (_sum != value)
				{
					_sum = value;
				}
			}
		}

		public SubTotalParameters GroupSubTotal
		{
			get
			{
				return _groupSubTotal;
			}
			set
			{
				if (_groupSubTotal != value)
				{
					_groupSubTotal = value;
				}
			}
		}

		public IFilter Filter
		{
			get
			{
				return _filter;
			}
			set
			{
				if (_filter != value)
				{
					_filter = value;
				}
			}
		}

		public AggregationCollection Agg
		{
			get
			{
				return _agg;
			}
			set
			{
				if (_agg != value)
				{
					_agg = value;
				}
			}
		}

		public List<int> ColIds
		{
			get
			{
				return _colIds;
			}
			set
			{
				if (_colIds != value)
				{
					_colIds = value;
				}
			}
		}

		public Join Join
		{
			get
			{
				return _join;
			}
			set
			{
				if (_join != value)
				{
					_join = value;
				}
			}
		}

		public string Key
		{
			get
			{
				return _key;
			}
			set
			{
				if (_key != value)
				{
					_key = value;
				}
			}
		}

		public int ScrollPosition
		{
			get
			{
				return _scrollPosition;
			}
			set
			{
				_scrollPosition = value;
			}
		}

		public object[] ParamValues
		{
			get
			{
				return _paramValues;
			}
			set
			{
				if (_paramValues != value)
				{
					_paramValues = value;
				}
			}
		}

		public List<int> ItemId
		{
			get
			{
				return _itemId;
			}
			set
			{
				if (_itemId != value)
				{
					_itemId = value;
				}
			}
		}

		public List<int> SelectionTypes
		{
			get
			{
				return _selectionTypes;
			}
			set
			{
				if (_selectionTypes != value)
				{
					_selectionTypes = value;
				}
			}
		}

		public List<string> DisplayValues
		{
			get
			{
				return _displayValues;
			}
			set
			{
				if (_displayValues != value)
				{
					_displayValues = value;
				}
			}
		}

		public bool IsNewGroupByOrJoinTable { get; set; }

		internal event TableObjectRemoveHandler TableObjectRemove;

		internal void Remove()
		{
			if (this.TableObjectRemove != null)
			{
				this.TableObjectRemove(this);
			}
		}

		private void AppendOptimizationToStringBuilder(StringBuilder sbToAppend, DatabaseBase connection, string tableAlias, string optTableColumnName, string optValue, bool startWithOr = false)
		{
			sbToAppend.Append(startWithOr ? " OR " : (string.IsNullOrWhiteSpace(sbToAppend.ToString()) ? " WHERE " : " AND "));
			if (!string.IsNullOrEmpty(tableAlias))
			{
				sbToAppend.Append(connection.Enquote(tableAlias));
				sbToAppend.Append(".");
			}
			sbToAppend.Append(connection.Enquote(optTableColumnName));
			sbToAppend.Append("=");
			sbToAppend.Append(connection.GetSqlString(optValue));
		}

		private void AppendAllOptimizationToStringBuilder(StringBuilder sbToAppend, DatabaseBase connection, ITableObject to, string tableAlias, string splitValue = null, string indexValue = null, string sortValue = null, bool startWithOr = false)
		{
			if (splitValue != null && to.SplitTableColumn != null)
			{
				AppendOptimizationToStringBuilder(sbToAppend, connection, tableAlias, to.SplitTableColumn.Name, splitValue, startWithOr);
				startWithOr = false;
			}
			if (indexValue != null && to.IndexTableColumn != null)
			{
				AppendOptimizationToStringBuilder(sbToAppend, connection, tableAlias, to.IndexTableColumn.Name, indexValue, startWithOr);
				startWithOr = false;
			}
			if (sortValue != null && to.SortColumn != null)
			{
				AppendOptimizationToStringBuilder(sbToAppend, connection, tableAlias, to.SortColumn.Name, sortValue, startWithOr);
				startWithOr = false;
			}
		}

		private void AppendSubQueryToStringBuilder(StringBuilder sbToAppend, DatabaseBase connection, string database, ITableObject to, string tableAlias, List<string> colsWithAlias, string splitValue = null, string indexValue = null, string sortValue = null, string additionalFilter = null)
		{
			StringBuilder sbWhere = new StringBuilder();
			sbToAppend.Append(" (SELECT ");
			sbToAppend.Append(string.Join(", ", colsWithAlias.Select((string c) => c)));
			sbToAppend.Append(" FROM ");
			sbToAppend.Append(connection.Enquote(database, to.TableName));
			AppendAllOptimizationToStringBuilder(sbWhere, connection, to, null, splitValue, indexValue, sortValue);
			if (to.PageSystem != null && !string.IsNullOrWhiteSpace(to.PageSystem.Filter))
			{
				sbWhere.Append((string.IsNullOrEmpty(sbWhere.ToString()) ? " WHERE " : " AND ") + to.PageSystem.Filter);
			}
			if (!string.IsNullOrWhiteSpace(additionalFilter))
			{
				sbWhere.Append((string.IsNullOrEmpty(sbWhere.ToString()) ? " WHERE " : " AND ") + additionalFilter);
			}
			sbToAppend.Append(sbWhere);
			sbToAppend.Append(") AS ");
			sbToAppend.Append(tableAlias);
		}

		public string GetJoinString(DatabaseBase connection, IOptimization opt, string createDb, string createTable, bool withoutCreateTable = false)
		{
			string indexValue = opt.FindValue(OptimizationType.IndexTable);
			string splitValue = opt.FindValue(OptimizationType.SplitTable);
			string sortValue = opt.FindValue(OptimizationType.SortColumn);
			StringBuilder returnCommandBuilder = new StringBuilder();
			if (_join != null)
			{
				string database1 = (_join.Table1.UserDefined ? "temp" : _join.Table1.Database);
				string database2 = (_join.Table2.UserDefined ? "temp" : _join.Table2.Database);
				StringBuilder selectionBuilder = new StringBuilder(" SELECT ");
				StringBuilder subqueryBuilder1 = new StringBuilder();
				StringBuilder subqueryBuilder2 = new StringBuilder();
				StringBuilder whereBuilder = new StringBuilder();
				List<string> colsAlias1 = new List<string>();
				List<string> colsAlias2 = new List<string>();
				List<string> colsWithAlias1 = new List<string>();
				List<string> colsWithAlias2 = new List<string>();
				foreach (IColumn c3 in _join.Table1.Columns)
				{
					string cAs2 = connection.Enquote(c3.Name + "_1");
					string col2 = ((!c3.IsEmpty) ? connection.Enquote(c3.Name) : ("'" + c3.ConstValue + "'"));
					colsAlias1.Add(cAs2);
					colsWithAlias1.Add(col2 + " AS " + cAs2);
				}
				foreach (IColumn c2 in _join.Table2.Columns)
				{
					string cAs = connection.Enquote(c2.Name + "_2");
					string col = ((!c2.IsEmpty) ? connection.Enquote(c2.Name) : ("'" + c2.ConstValue + "'"));
					colsAlias2.Add(cAs);
					colsWithAlias2.Add(col + " AS " + cAs);
				}
				string additionalFilter1 = ((_join.Table1.PageSystem == null) ? null : _join.Table1.PageSystem.Filter);
				string additionalFilter2 = ((_join.Filter2 == null) ? null : _join.Filter2.ToString());
				AppendSubQueryToStringBuilder(subqueryBuilder1, connection, database1, _join.Table1, "t1", colsWithAlias1, splitValue, indexValue, sortValue, additionalFilter1);
				AppendSubQueryToStringBuilder(subqueryBuilder2, connection, database2, _join.Table2, "t2", colsWithAlias2, splitValue, indexValue, sortValue, additionalFilter2);
				selectionBuilder.Append(" * FROM ");
				selectionBuilder.Append(subqueryBuilder1);
				switch (_join.Type)
				{
				case JoinType.OnlyLeft:
				case JoinType.Left:
					selectionBuilder.Append(" LEFT JOIN ");
					break;
				case JoinType.OnlyRight:
				case JoinType.Right:
					selectionBuilder.Append(" RIGHT JOIN ");
					break;
				case JoinType.Inner:
					selectionBuilder.Append(" INNER JOIN ");
					break;
				}
				selectionBuilder.Append(subqueryBuilder2);
				selectionBuilder.Append(" ON ");
				selectionBuilder.Append(string.Join(" AND ", _join.Columns.Select((JoinColumns c) => connection.Enquote("t1") + "." + connection.Enquote(_join.Table1.Columns[c.Column1].Name + "_1") + "=" + connection.Enquote("t2") + "." + connection.Enquote(_join.Table2.Columns[c.Column2].Name + "_2"))));
				if (_join.Type == JoinType.OnlyLeft || _join.Type == JoinType.OnlyRight)
				{
					whereBuilder.Append(string.IsNullOrWhiteSpace(whereBuilder.ToString()) ? " WHERE " : " AND ");
					if (_join.Type == JoinType.OnlyRight)
					{
						whereBuilder.Append(connection.Enquote("t1"));
						whereBuilder.Append(".");
						whereBuilder.Append(connection.Enquote(_join.Table1.Columns[_join.Columns[0].Column1].Name + "_1"));
					}
					else
					{
						whereBuilder.Append(connection.Enquote("t2"));
						whereBuilder.Append(".");
						whereBuilder.Append(connection.Enquote(_join.Table2.Columns[_join.Columns[0].Column2].Name + "_2"));
					}
					whereBuilder.Append(" IS NULL");
				}
				if (!string.IsNullOrWhiteSpace(createDb) && !string.IsNullOrWhiteSpace(createTable))
				{
					string enquotedCreateTableFQN = connection.Enquote(createDb, createTable);
					returnCommandBuilder.Append("CREATE TABLE IF NOT EXISTS ");
					returnCommandBuilder.Append(enquotedCreateTableFQN);
					returnCommandBuilder.Append(selectionBuilder);
					returnCommandBuilder.Append(" LIMIT 0;");
					returnCommandBuilder.Append("\nTRUNCATE TABLE ");
					returnCommandBuilder.Append(enquotedCreateTableFQN);
					returnCommandBuilder.Append(";");
					returnCommandBuilder.Append("\nINSERT INTO ");
					returnCommandBuilder.Append(enquotedCreateTableFQN);
					returnCommandBuilder.Append(" (");
					returnCommandBuilder.Append(string.Join(",", colsAlias1));
					returnCommandBuilder.Append(",");
					returnCommandBuilder.Append(string.Join(",", colsAlias2));
					returnCommandBuilder.Append(") ");
				}
				returnCommandBuilder.Append(selectionBuilder);
				returnCommandBuilder.Append(whereBuilder);
			}
			string returnCommand = returnCommandBuilder.ToString();
			if (!string.IsNullOrEmpty(returnCommand))
			{
				return GetSimpleQuery(returnCommand);
			}
			return "";
		}

		public string GetSelectionString(DatabaseBase db, IOptimization opt, SortCollection sort, IFilter filter, object[] paramValues, string filterIssue, string joinKey, bool joinSort, bool joinFilter, bool joinIssue, long startRow, long limit, IUser user, bool selectRowNo = true, bool onlyCount = false, string commandFilter = "", bool withFilterIssue = false, PageSystem pageSystem = null, bool limited = true, bool enabledOrder = true, string roleBasedOptimizationFilter = null, int subtotalLevel = 0, bool subtotalInternalSelection = false, bool subtotalSeparator = false)
		{
			string tname = Table.TableName;
			if (OriginalTable is IIssue && (OriginalTable as IIssue).IssueType == IssueType.Filter)
			{
				tname = (OriginalTable as IIssue).FilterTableObject.TableName;
			}
			List<string> cols = new List<string>();
			if (selectRowNo)
			{
				if (onlyCount)
				{
					cols.Add("COUNT(" + db.Enquote("_row_no_") + ") as _row_no_");
				}
				else
				{
					cols.Add(db.Enquote("_row_no_"));
					if (!string.IsNullOrWhiteSpace(Key))
					{
						if (Sum != null && Sum.Count() != 0)
						{
							foreach (IColumn c7 in Table.Columns)
							{
								if (c7.IsEmpty)
								{
									cols.Add("SUM('" + c7.ConstValue + "') as " + db.Enquote(c7.Name));
								}
								else
								{
									cols.Add("SUM(" + db.Enquote(c7.Name) + ")");
								}
							}
						}
						else if (GroupSubTotal != null && GroupSubTotal.GroupList != null)
						{
							if (subtotalInternalSelection)
							{
								int m = 0;
								foreach (string c6 in GroupSubTotal.GroupList)
								{
									if (subtotalSeparator)
									{
										cols.Add("null");
									}
									else if (m < subtotalLevel + 1)
									{
										cols.Add(db.Enquote(c6));
									}
									else
									{
										cols.Add("'*'");
									}
									m++;
								}
								foreach (string c5 in GroupSubTotal.ColumnList)
								{
									if (subtotalSeparator)
									{
										cols.Add("null");
									}
									else
									{
										cols.Add("SUM(" + db.Enquote(c5) + ")");
									}
								}
								cols.Add(subtotalLevel + 1 + " as orderby");
							}
							else
							{
								cols.Clear();
								cols.Add("*");
							}
						}
						else
						{
							foreach (IColumn c4 in Table.Columns)
							{
								if (c4.IsEmpty)
								{
									cols.Add("'" + c4.ConstValue + "' as " + db.Enquote(c4.Name));
								}
								else if (c4.DataType == SqlType.Binary)
								{
									cols.Add($"CAST({db.Enquote(c4.Name)} AS CHAR(255))");
								}
								else
								{
									cols.Add(db.Enquote(c4.Name));
								}
							}
						}
					}
				}
			}
			else if (GroupSubTotal != null && GroupSubTotal.GroupList != null)
			{
				if (subtotalInternalSelection)
				{
					int l = 0;
					foreach (string c3 in GroupSubTotal.GroupList)
					{
						if (subtotalSeparator)
						{
							cols.Add("null");
						}
						else if (l < subtotalLevel + 1)
						{
							cols.Add(db.Enquote(c3));
						}
						else
						{
							cols.Add("'*'");
						}
						l++;
					}
					foreach (string c2 in GroupSubTotal.ColumnList)
					{
						if (subtotalSeparator)
						{
							cols.Add("null");
						}
						else
						{
							cols.Add("SUM(" + db.Enquote(c2) + ")");
						}
					}
					cols.Add(subtotalLevel + 1 + " as orderby");
				}
				else
				{
					cols.Clear();
					cols.Add("*");
				}
			}
			else
			{
				cols.AddRange(from c in Table.Columns
					where !c.IsEmpty && c.IsVisible
					select db.Enquote(c.Name));
			}
			string database = ((ViewboxDb.UseNewIssueMethod && OriginalTable is IIssue && (OriginalTable as IIssue).IssueType == IssueType.StoredProcedure) ? user.UserTable(Table.Database) : Table.Database);
			string sqlForm = "";
			if (GroupSubTotal != null && GroupSubTotal.GroupList != null && subtotalLevel == GroupSubTotal.GroupList.Count)
			{
				List<string> subtotalSelectList = new List<string>();
				for (int k = subtotalLevel - 1; k >= 0; k--)
				{
					subtotalSelectList.Add("( " + GetSelectionString(db, opt, sort, filter, paramValues, filterIssue, joinKey, joinSort, joinFilter, joinIssue, startRow, limit, user, selectRowNo, onlyCount, commandFilter, withFilterIssue, pageSystem, limited, enabledOrder, roleBasedOptimizationFilter, k, subtotalInternalSelection: true) + " )");
					if (subtotalLevel - 1 > k)
					{
						subtotalSelectList.Add("( " + GetSelectionString(db, opt, sort, filter, paramValues, filterIssue, joinKey, joinSort, joinFilter, joinIssue, startRow, limit, user, selectRowNo, onlyCount, commandFilter, withFilterIssue, pageSystem, limited, enabledOrder, roleBasedOptimizationFilter, k, subtotalInternalSelection: true, subtotalSeparator: true) + " )");
					}
					if (GroupSubTotal.OnlyExpectedResult)
					{
						k = -1;
					}
				}
				subtotalSelectList.Add("( " + GetSelectionString(db, opt, sort, filter, paramValues, filterIssue, joinKey, joinSort, joinFilter, joinIssue, startRow, limit, user, selectRowNo, onlyCount, commandFilter, withFilterIssue, pageSystem, limited, enabledOrder, roleBasedOptimizationFilter, -1, subtotalInternalSelection: true) + " )");
				subtotalSelectList.Add("( " + GetSelectionString(db, opt, sort, filter, paramValues, filterIssue, joinKey, joinSort, joinFilter, joinIssue, startRow, limit, user, selectRowNo, onlyCount, commandFilter, withFilterIssue, pageSystem, limited, enabledOrder, roleBasedOptimizationFilter, -1, subtotalInternalSelection: true, subtotalSeparator: true) + " )");
				sqlForm = string.Join(" UNION ALL ", subtotalSelectList);
				if (subtotalSelectList.Count > 1)
				{
					sqlForm = "( " + sqlForm + " )";
				}
			}
			else
			{
				string database2 = (Table.UserDefined ? db.DbConfig.DbName : database);
				string checkTableExistSql = $"select count(1) from `information_schema`.`TABLES` t where t.`table_schema` = '{database2}' and t.`TABLE_NAME` = '{tname}'";
				DatabaseBase conn = db;
				int result = 2;
				if (conn != null && int.TryParse(conn.ExecuteScalar(checkTableExistSql).ToString(), out var actValue))
				{
					result = actValue;
				}
				if (result < 1)
				{
					database2 = db.DbConfig.DbName;
				}
				sqlForm = db.Enquote(database2, tname);
			}
			string creationString = string.Format("SELECT {0} FROM {1} AS t ", string.Join(", ", cols.Select((string c) => c)), sqlForm);
			List<string> conditions = new List<string>();
			if (0 < limit && pageSystem == null && limited && !string.IsNullOrWhiteSpace(Key))
			{
				conditions.Add(string.Format("t.{0} BETWEEN {1} AND {2}", db.Enquote("_row_no_"), startRow + 1, startRow + limit));
			}
			else
			{
				string indexValue = opt.FindValue(OptimizationType.IndexTable);
				string splitValue = opt.FindValue(OptimizationType.SplitTable);
				string sortValue = opt.FindValue(OptimizationType.SortColumn);
				List<string> areas = new List<string>(from o in Table.OrderAreas.GetRanges(user.CurrentLanguage, indexValue, splitValue, sortValue)
					select string.Format("{0} BETWEEN {1} AND {2}", db.Enquote("_row_no_"), o.Item1, o.Item2));
				if (_multiOptimization)
				{
					areas.Clear();
					if (OptimizationSelected != null)
					{
						List<IOptimization> mandts = new List<IOptimization>();
						List<IOptimization> bukrs = new List<IOptimization>();
						List<IOptimization> gjahrs = new List<IOptimization>();
						IOptimization system = GetOptimizationBySystem(opt);
						int j;
						for (j = 1; j < OptimizationSelected.Count + 1; j++)
						{
							switch (j)
							{
							case 1:
							{
								if (system == null)
								{
									break;
								}
								IOrderedEnumerable<IOptimization> mandtsList = system.Children.OrderBy((IOptimization o) => o.Value);
								IOptimization fromMandts = mandtsList.FirstOrDefault((IOptimization o) => o.Id == OptimizationSelected[j].Item2);
								IOptimization toMandts = mandtsList.FirstOrDefault((IOptimization o) => o.Id == OptimizationSelected[j].Item1);
								if (fromMandts != null && toMandts != null)
								{
									int startIndex = mandtsList.ToList().IndexOf(fromMandts);
									int endIndex = mandtsList.ToList().IndexOf(toMandts);
									if (startIndex > endIndex)
									{
										int num3 = startIndex;
										startIndex = endIndex;
										endIndex = num3;
									}
									int endCount = endIndex - startIndex;
									mandts = mandtsList.Skip(startIndex).Take(endCount + 1).ToList();
								}
								break;
							}
							case 2:
							{
								if (!mandts.Any())
								{
									break;
								}
								IOrderedEnumerable<IOptimization> bukrsList = from o in mandts.SelectMany((IOptimization x) => x.Children)
									orderby o.Value
									select o;
								IOptimization fromBukrs = bukrsList.FirstOrDefault((IOptimization o) => o.Id == OptimizationSelected[j].Item1);
								IOptimization toBukrs = bukrsList.FirstOrDefault((IOptimization o) => o.Id == OptimizationSelected[j].Item2);
								if (fromBukrs != null && toBukrs != null)
								{
									int startIndex2 = bukrsList.ToList().IndexOf(fromBukrs);
									int endIndex2 = bukrsList.ToList().IndexOf(toBukrs);
									if (startIndex2 > endIndex2)
									{
										int num2 = startIndex2;
										startIndex2 = endIndex2;
										endIndex2 = num2;
									}
									int endCount2 = endIndex2 - startIndex2;
									bukrs = bukrsList.Skip(startIndex2).Take(endCount2 + 1).ToList();
								}
								break;
							}
							case 3:
							{
								if (!bukrs.Any())
								{
									break;
								}
								IOrderedEnumerable<IOptimization> gjahrsList = from o in bukrs.SelectMany((IOptimization x) => x.Children)
									orderby o.Value
									select o;
								IOptimization toGjahrs = gjahrsList.FirstOrDefault((IOptimization o) => o.Id == OptimizationSelected[j].Item1);
								IOptimization fromGjahrs = gjahrsList.FirstOrDefault((IOptimization o) => o.Id == OptimizationSelected[j].Item2);
								if (fromGjahrs != null && toGjahrs != null)
								{
									int startIndex3 = gjahrsList.ToList().IndexOf(fromGjahrs);
									int endIndex3 = gjahrsList.ToList().IndexOf(toGjahrs);
									if (startIndex3 > endIndex3)
									{
										int num = startIndex3;
										startIndex3 = endIndex3;
										endIndex3 = num;
									}
									int endCount3 = endIndex3 - startIndex3;
									gjahrs = gjahrsList.Skip(startIndex3).Take(endCount3 + 1).ToList();
								}
								break;
							}
							}
						}
						foreach (IOptimization item in mandts)
						{
							indexValue = item.FindValue(OptimizationType.IndexTable);
							foreach (IOptimization item2 in bukrs)
							{
								splitValue = item2.FindValue(OptimizationType.SplitTable);
								foreach (IOptimization item3 in gjahrs)
								{
									sortValue = item3.FindValue(OptimizationType.SortColumn);
									foreach (string oFilter in from o in Table.OrderAreas.GetRanges(user.CurrentLanguage, indexValue, splitValue, sortValue)
										select string.Format("{0} BETWEEN {1} AND {2}", db.Enquote("_row_no_"), o.Item1, o.Item2))
									{
										if (!areas.Contains(oFilter))
										{
											areas.Add(oFilter);
										}
									}
								}
							}
						}
					}
				}
				conditions.Add((areas.Count > 0) ? string.Format("({0})", string.Join(" OR ", areas)) : string.Format("({0} IS NOT NULL)", db.Enquote("_row_no_")));
			}
			if (!string.IsNullOrWhiteSpace(Key))
			{
				if (pageSystem == null && joinFilter)
				{
					creationString = creationString + " INNER JOIN " + db.Enquote(db.DbConfig.DbName, Key) + " AS s ";
					creationString = creationString + "ON " + db.Enquote("t") + "." + db.Enquote("_row_no_") + "=" + db.Enquote("s") + "." + db.Enquote("_row_no_");
				}
				else if (pageSystem != null)
				{
					string FilterCorrector = pageSystem.Filter;
					FilterCorrector = FilterCorrector.Replace("(.`", "(`");
					FilterCorrector = $"({FilterCorrector})";
					if (db.Connection.Database.ToLower() != "temp")
					{
						conditions.Add(FilterCorrector);
					}
				}
			}
			if (pageSystem == null && limited && !string.IsNullOrWhiteSpace(joinKey) && string.IsNullOrWhiteSpace(Key))
			{
				creationString = creationString + " INNER JOIN " + db.Enquote(db.DbConfig.DbName, joinKey) + " AS s ";
				creationString = creationString + "ON " + db.Enquote("t") + "." + db.Enquote("_row_no_") + "=" + db.Enquote("s") + "." + db.Enquote("_row_no_");
			}
			if (filter != null && !joinFilter)
			{
				string condition = filter.ToString(db);
				if (condition != "()")
				{
					conditions.Add(condition);
				}
			}
			if (!string.IsNullOrWhiteSpace(filterIssue) && string.IsNullOrWhiteSpace(Key) && (withFilterIssue || !joinIssue || (pageSystem != null && filter == null)) && db.Connection.Database.ToLower() != "temp")
			{
				conditions.Add(filterIssue);
			}
			if (!string.IsNullOrEmpty(Key) && creationString.ToLower().Contains(database.ToLower()) && !creationString.ToLower().Contains(Key.ToLower()))
			{
				conditions.Add(pageSystem.Filter);
			}
			string roleBasedSQL = ((OriginalTable != null) ? OriginalTable.GetBaseObject() : Table.GetBaseObject()).GetRoleBasedFilterSQL(user);
			if (roleBasedSQL != string.Empty && !conditions.Contains(roleBasedSQL))
			{
				conditions.Add(roleBasedSQL);
			}
			if (Table.OptimizationHidden == 1 && !user.IsSuper && roleBasedOptimizationFilter != null)
			{
				conditions.Add(roleBasedOptimizationFilter);
				if (pageSystem != null)
				{
					pageSystem.RoleBasedOptimizationFilter = roleBasedOptimizationFilter;
				}
			}
			if (conditions.Count > 0 && ((GroupSubTotal != null && GroupSubTotal.GroupList != null && subtotalInternalSelection) || (GroupSubTotal == null && !subtotalInternalSelection)) && Table.RowCount > 0)
			{
				List<string> conds = new List<string>();
				conds.Add(conditions[0]);
				int i;
				for (i = 1; i < conditions.Count; i++)
				{
					if (conds.All((string cond) => cond.Contains("OR") || (!cond.Contains(conditions[i]) && (conditions[i].First() != '(' || conditions[i].Last() != ')' || !cond.Contains(conditions[i].Substring(1, conditions[i].Length - 2))))) && !string.IsNullOrEmpty(conditions[i]))
					{
						conds.Add(conditions[i]);
					}
				}
				creationString = creationString + " WHERE " + string.Join(" AND ", conds);
			}
			if (!subtotalSeparator && GroupSubTotal != null && GroupSubTotal.GroupList != null && subtotalInternalSelection && subtotalLevel != -1)
			{
				creationString = creationString + " GROUP BY `" + string.Join("`, `", GroupSubTotal.GroupList.Take(subtotalLevel + 1)) + "`";
			}
			if (!subtotalSeparator)
			{
				if ((Sum == null || Sum.Count == 0) && enabledOrder)
				{
					if (GroupSubTotal != null && GroupSubTotal.GroupList != null && !subtotalInternalSelection)
					{
						creationString = creationString + " ORDER BY `orderby` DESC, `" + string.Join("`, `", GroupSubTotal.GroupList) + "`";
					}
					else if (pageSystem != null)
					{
						creationString = creationString + " ORDER BY " + pageSystem.OrderBy;
					}
					else if (sort != null && sort.Count != 0 && string.IsNullOrWhiteSpace(Key) && !joinSort)
					{
						string orderBy = "";
						foreach (Sort sortItem in sort)
						{
							IColumn column = _table.Columns[sortItem.cid] ?? _table.Columns.FirstOrDefault((IColumn x) => x.Id == sortItem.cid);
							if (!column.IsEmpty)
							{
								if (orderBy.Length > 0)
								{
									orderBy += ",";
								}
								orderBy = orderBy + db.Enquote("t") + "." + db.Enquote(column.Name);
								if (sortItem.dir == SortDirection.Descending)
								{
									orderBy += " DESC";
								}
							}
						}
						if (!string.IsNullOrWhiteSpace(orderBy))
						{
							creationString = creationString + " ORDER BY " + orderBy;
						}
						if (HasDefaultOrderField)
						{
							creationString += ", _row_no_";
						}
					}
					else if (pageSystem == null && !string.IsNullOrWhiteSpace(Key))
					{
						creationString += " ORDER BY t._row_no_";
					}
					else if (HasDefaultOrderField && Table.Columns.Contains("_row_no_"))
					{
						creationString += " ORDER BY _row_no_";
					}
				}
				else if (HasDefaultOrderField && (Sum == null || Sum.Count == 0))
				{
					creationString += " ORDER BY _row_no_";
				}
			}
			if (GroupSubTotal != null && GroupSubTotal.GroupList != null && !subtotalInternalSelection)
			{
				GroupSubTotal.sql = creationString;
			}
			if (subtotalSeparator)
			{
				creationString += " LIMIT 1";
			}
			else if (pageSystem != null && limit > 0 && (Sum == null || Sum.Count == 0) && !subtotalInternalSelection)
			{
				if (startRow < 0)
				{
					if (limit - startRow >= 0)
					{
						creationString += $" LIMIT {0}, {limit - startRow}";
					}
				}
				else
				{
					creationString += $" LIMIT {startRow}, {limit}";
				}
			}
			return GetSimpleQuery(creationString);
		}

		public string GetFilterString(DatabaseBase db, IOptimization opt, IFilter filter, string filterIssue, bool joinFilter, bool joinIssue, IUser user, bool withFilterIssue = false)
		{
			List<string> conditions = new List<string>();
			if (filter != null && string.IsNullOrWhiteSpace(Key) && !joinFilter)
			{
				string condition = filter.ToString(db);
				if (condition != "()")
				{
					conditions.Add(condition);
				}
			}
			if (!string.IsNullOrWhiteSpace(filterIssue) && conditions.Count((string c) => c == filterIssue) == 0)
			{
				conditions.Add(filterIssue);
			}
			if (!string.IsNullOrWhiteSpace(filterIssue) && string.IsNullOrWhiteSpace(Key) && (withFilterIssue || !joinIssue || filter == null) && conditions.Count((string c) => c == filterIssue) == 0)
			{
				conditions.Add(filterIssue);
			}
			if (conditions.Count <= 0)
			{
				return null;
			}
			return GetSimpleQuery(string.Join(" AND ", conditions));
		}

		public string GetSortString(DatabaseBase db, SortCollection sort, bool joinSort)
		{
			if (sort != null && sort.Count != 0 && string.IsNullOrWhiteSpace(Key) && !joinSort)
			{
				string orderBy = string.Empty;
				foreach (Sort sortItem in sort)
				{
					IColumn column = _table.Columns[sortItem.cid] ?? _table.Columns.FirstOrDefault((IColumn x) => x.Id == sortItem.cid);
					if (column != null && !column.IsEmpty)
					{
						if (orderBy.Length > 0)
						{
							orderBy += ",";
						}
						orderBy = orderBy + db.Enquote("t") + "." + db.Enquote(column.Name);
						if (sortItem.dir == SortDirection.Descending)
						{
							orderBy += " DESC";
						}
					}
				}
				return orderBy + ", " + db.Enquote("_row_no_");
			}
			if (!string.IsNullOrWhiteSpace(Key))
			{
				return "s.id";
			}
			return "_row_no_";
		}

		public string GetGroupByString(DatabaseBase db, IFilter filter, string createDb, string createTable, IUser user, string filterIssue, out Dictionary<int, string> newColNames)
		{
			newColNames = new Dictionary<int, string>();
			List<string> cols = new List<string>();
			List<string> aliases = new List<string>();
			string database = ((ViewboxDb.UseNewIssueMethod && OriginalTable is IIssue && (OriginalTable as IIssue).IssueType == IssueType.StoredProcedure) ? user.UserTable(Table.Database) : Table.Database);
			database = (Table.UserDefined ? db.DbConfig.DbName : database);
			IAggregationFunctionTranslator translator = DatabaseSpecificFactory.GetAggregationFunctionTranslator();
			if (_agg != null && _agg.Count > 0)
			{
				foreach (Aggregation aggItem in _agg)
				{
					IColumn col4 = _table.Columns[aggItem.cid];
					string prefix = string.Empty;
					string name = col4.Name.ToLower();
					if (_colIds.Contains(aggItem.cid))
					{
						prefix = col4.Name.Split('_')[0];
						if (int.TryParse(prefix, out var numberPrefex))
						{
							numberPrefex++;
							prefix = numberPrefex + "_";
							name = col4.Name.Substring(col4.Name.IndexOf("_") + 1);
						}
						else
						{
							prefix = "0_";
						}
						newColNames.Add(aggItem.cid, prefix + translator.ConvertEnumToString(aggItem.agg).ToLower() + "_" + name.ToLower());
					}
					string alias = db.Enquote(prefix + translator.ConvertEnumToString(aggItem.agg).ToLower() + "_" + name.ToLower());
					cols.Add($"{translator.ConvertEnumToString(aggItem.agg).ToUpper()}({db.Enquote(col4.Name)}) AS {alias}");
					aliases.Add(alias);
				}
			}
			if (_colIds != null)
			{
				foreach (int c2 in _colIds)
				{
					IColumn col3 = _table.Columns[c2];
					cols.Add(db.Enquote(col3.Name));
					aliases.Add(db.Enquote(col3.Name));
				}
			}
			else if (_agg == null || _agg.Count <= 0)
			{
				foreach (IColumn col2 in _table.Columns)
				{
					cols.Add(db.Enquote(col2.Name));
					aliases.Add(db.Enquote(col2.Name));
				}
			}
			StringBuilder selectBuilder = new StringBuilder(db.PrepareCreateFromSelect(database, _table.TableName, string.Join(", ", cols.ToList()), createDb, createTable, withouCreateTable: true));
			List<string> conditions = new List<string>();
			string indexValue = _optimization.FindValue(OptimizationType.IndexTable);
			string splitValue = _optimization.FindValue(OptimizationType.SplitTable);
			string sortValue = _optimization.FindValue(OptimizationType.SortColumn);
			List<RoleArea> roles = Table.RoleAreas.Where((RoleArea r) => user.Roles.Any((IRole u) => u.Id == r.RoleId)).ToList();
			if (roles.Count == 0)
			{
				roles.AddRange(Table.RoleAreas.Where((RoleArea r) => r.RoleId == 0).ToList());
			}
			if (roles.Count != 0 && roles.All((RoleArea t) => t.Mark != null) && Table.Type != TableType.Table && splitValue == null)
			{
				splitValue = roles.Select((RoleArea r) => r.Mark).FirstOrDefault();
				conditions.Add($"_role_ LIKE '%{splitValue}%'");
			}
			List<string> areas = new List<string>(from o in Table.OrderAreas.GetRanges(user.CurrentLanguage, indexValue, splitValue, sortValue)
				select string.Format("{0} BETWEEN {1} AND {2}", "_row_no_", o.Item1, o.Item2));
			if (areas.Count > 0)
			{
				conditions.Add(string.Format("({0})", string.Join(" OR ", areas)));
			}
			if (filter != null && string.IsNullOrWhiteSpace(Key))
			{
				conditions.Add(filter.ToString(db, "t"));
			}
			if (!string.IsNullOrEmpty(filterIssue) && !string.IsNullOrEmpty(filterIssue.Trim()) && !Table.UserDefined)
			{
				conditions.Add(filterIssue);
			}
			if (conditions.Count > 0)
			{
				selectBuilder.Append(" WHERE ");
				selectBuilder.Append(string.Join(" AND ", conditions));
			}
			StringBuilder groupBy = new StringBuilder();
			if (_colIds != null)
			{
				foreach (int c in _colIds)
				{
					IColumn col = _table.Columns[c];
					if (groupBy.Length > 0)
					{
						groupBy.Append(",");
					}
					groupBy.Append(db.Enquote(col.Name));
				}
			}
			if (groupBy.Length > 0)
			{
				selectBuilder.Append(" GROUP BY ");
				selectBuilder.Append(groupBy);
			}
			StringBuilder resultBuilder = new StringBuilder("CREATE TABLE IF NOT EXISTS ");
			resultBuilder.Append(db.Enquote(createDb, createTable));
			resultBuilder.Append(" ");
			resultBuilder.Append(selectBuilder);
			resultBuilder.Append(" LIMIT 0;");
			resultBuilder.Append("TRUNCATE TABLE ");
			resultBuilder.Append(db.Enquote(createDb, createTable));
			resultBuilder.Append(";INSERT INTO ");
			resultBuilder.Append(db.Enquote(createDb, createTable));
			resultBuilder.Append(" (");
			resultBuilder.Append(string.Join(", ", aliases));
			resultBuilder.Append(") ");
			resultBuilder.Append(selectBuilder);
			resultBuilder.Append(";");
			return GetSimpleQuery(resultBuilder.ToString());
		}

		public string GetSelectionString(DatabaseBase db, long startRow, long limit, bool selectRowNo = true)
		{
			List<string> cols = new List<string>();
			if (selectRowNo)
			{
				cols.Add(db.Enquote("_row_no_"));
				foreach (IColumn c2 in Table.Columns)
				{
					if (c2.IsEmpty)
					{
						cols.Add("'" + c2.ConstValue + "' as " + db.Enquote(c2.Name));
					}
					else if (c2.DataType == SqlType.Binary)
					{
						cols.Add($"CAST({db.Enquote(c2.Name)} AS CHAR(255))");
					}
					else
					{
						cols.Add(db.Enquote(c2.Name));
					}
				}
			}
			else
			{
				cols.AddRange(from c in Table.Columns
					where !c.IsEmpty && c.IsVisible
					select db.Enquote(c.Name));
			}
			string database = (_table.UserDefined ? db.DbConfig.DbName : _table.Database);
			string creationString = string.Format("SELECT {0} FROM {1} AS t ", string.Join(", ", cols.Select((string c) => c)), db.Enquote(database, _table.TableName));
			if (0 < limit)
			{
				creationString += string.Format(" WHERE {0} BETWEEN {1} AND {2}", "_row_no_", startRow + 1, startRow + limit);
			}
			if (HasDefaultOrderField)
			{
				creationString += " ORDER BY _row_no_";
			}
			return GetSimpleQuery(creationString);
		}

		public string GetSelectionStringSubTotal(DatabaseBase db, IOptimization opt, IUser user, string groupName = null, string subTotalGroupName = null, string groupNameValue = null, string filter = null)
		{
			string creationString = string.Empty;
			creationString = ((!(Table is Issue) || (Table as Issue).IssueType != IssueType.StoredProcedure) ? $"SELECT distinct sql_cache sum({subTotalGroupName}) FROM {db.Enquote(Table.UserDefined ? db.DbConfig.DbName : Table.Database, Table.TableName)} AS t " : $"SELECT distinct sql_cache sum({subTotalGroupName}) FROM {db.Enquote(user.UserTable(Table.Database), Table.TableName)} AS t ");
			List<string> conditions = new List<string>();
			List<string> limits = new List<string>();
			List<RoleArea> roles = Table.RoleAreas.Where((RoleArea r) => user.Roles.Any((IRole u) => u.Id == r.RoleId)).ToList();
			string indexValue = opt.FindValue(OptimizationType.IndexTable);
			string splitValue = opt.FindValue(OptimizationType.SplitTable);
			string sortValue = opt.FindValue(OptimizationType.SortColumn);
			if (_multiOptimization)
			{
				limits.Clear();
				if (OptimizationSelected != null)
				{
					List<IOptimization> mandts = new List<IOptimization>();
					List<IOptimization> bukrs = new List<IOptimization>();
					List<IOptimization> gjahrs = new List<IOptimization>();
					int j;
					for (j = 1; j < OptimizationSelected.Count + 1; j++)
					{
						switch (j)
						{
						case 1:
							mandts = OptimizationCollection.Where((IOptimization o) => o.Id >= OptimizationSelected[j].Item1 && OptimizationSelected[j].Item2 >= o.Id).ToList();
							break;
						case 2:
							bukrs = OptimizationCollection.Where((IOptimization o) => o.Id >= OptimizationSelected[j].Item1 && OptimizationSelected[j].Item2 >= o.Id).ToList();
							break;
						case 3:
							gjahrs = OptimizationCollection.Where((IOptimization o) => o.Id >= OptimizationSelected[j].Item1 && OptimizationSelected[j].Item2 >= o.Id).ToList();
							break;
						}
					}
					foreach (IOptimization item in mandts)
					{
						indexValue = item.FindValue(OptimizationType.IndexTable);
						foreach (IOptimization item2 in bukrs)
						{
							splitValue = item2.FindValue(OptimizationType.SplitTable);
							foreach (IOptimization item3 in gjahrs)
							{
								sortValue = item3.FindValue(OptimizationType.SortColumn);
								foreach (string oFilter in from o in Table.OrderAreas.GetRanges(user.CurrentLanguage, indexValue, splitValue, sortValue)
									select string.Format("{0} BETWEEN {1} AND {2}", db.Enquote("_row_no_"), o.Item1, o.Item2))
								{
									if (!limits.Contains(oFilter))
									{
										limits.Add(oFilter);
									}
								}
							}
						}
					}
				}
			}
			if (roles.Count == 0)
			{
				roles.AddRange(Table.RoleAreas.Where((RoleArea r) => r.RoleId == 0).ToList());
			}
			if (roles.Count != 0 && roles.All((RoleArea t) => t.Mark != null) && Table.Type != TableType.Table && splitValue == null)
			{
				splitValue = roles.Select((RoleArea r) => r.Mark).FirstOrDefault();
				if (conditions.Count == 0)
				{
					conditions.Add($"_role_ LIKE '%{splitValue}%'");
				}
			}
			limits.AddRange(from o in Table.OrderAreas.GetRanges(user.CurrentLanguage, indexValue, splitValue, sortValue)
				select string.Format("({0} BETWEEN {1} AND {2})", db.Enquote("_row_no_"), o.Item1, o.Item2));
			if (limits.Count > 0)
			{
				conditions.Add(string.Format("({0})", string.Join(" OR ", limits)));
			}
			if (conditions.Count > 0)
			{
				List<string> conds = new List<string>();
				conds.Add(conditions[0]);
				for (int i = 1; i < conditions.Count; i++)
				{
					bool ok = true;
					foreach (string cond in conds)
					{
						if (!cond.Contains("OR") && (cond.Contains(conditions[i]) || (conditions[i].First() == '(' && conditions[i].Last() == ')' && cond.Contains(conditions[i].Substring(1, conditions[i].Length - 2)))))
						{
							ok = false;
							break;
						}
					}
					if (ok)
					{
						conds.Add(conditions[i]);
					}
				}
				creationString = creationString + " WHERE " + string.Join(" AND ", conds);
			}
			if (!string.IsNullOrWhiteSpace(groupName))
			{
				creationString = ((conditions.Count == 0) ? (creationString + $" WHERE ({groupName} = '{groupNameValue}')") : (string.IsNullOrWhiteSpace(groupNameValue) ? (creationString + $" AND ({groupName} = '')") : (creationString + $" AND ({groupName} = '{groupNameValue}')")));
			}
			if (conditions.Count == 0 && !string.IsNullOrWhiteSpace(filter) && !creationString.Contains("WHERE"))
			{
				creationString += $" WHERE ({filter})";
			}
			else if (!string.IsNullOrWhiteSpace(filter))
			{
				creationString += $" AND {filter}";
			}
			return GetSimpleQuery(creationString);
		}

		public string GetSelectionString(DatabaseBase db, IOptimization opt, long startRow, long limit, IUser user, bool selectRowNo = true, bool getAllFields = false, PageSystem pageSystem = null, string roleBasedOptimizationFilter = null)
		{
			if (((_sort != null || _filter != null || _paramValues != null) && _agg == null && _colIds == null && _join == null) || _filterIssue != null)
			{
				return GetSelectionString(db, opt, _sort, _filter, _paramValues, _filterIssue, string.Empty, joinSort: false, joinFilter: false, joinIssue: false, startRow, limit, user, selectRowNo, onlyCount: false, "", withFilterIssue: false, pageSystem, limited: true, enabledOrder: true, roleBasedOptimizationFilter, (GroupSubTotal != null) ? GroupSubTotal.GroupList.Count : 0);
			}
			if (_agg != null || _colIds != null || _join != null)
			{
				return GetSelectionString(db, startRow, limit, selectRowNo);
			}
			string tname = Table.TableName;
			List<string> cols = new List<string>();
			if (selectRowNo)
			{
				cols.Add(db.Enquote("_row_no_"));
				if (Sum != null && Sum.Count != 0)
				{
					foreach (IColumn c3 in Table.Columns)
					{
						if (c3.IsEmpty)
						{
							cols.Add("SUM('" + c3.ConstValue + "') as " + db.Enquote(c3.Name));
						}
						else
						{
							cols.Add("SUM(" + db.Enquote(c3.Name) + ")");
						}
					}
				}
				else
				{
					foreach (IColumn c2 in Table.Columns)
					{
						if (c2.IsEmpty)
						{
							cols.Add("'" + c2.ConstValue + "' as " + db.Enquote(c2.Name));
						}
						else if (c2.DataType == SqlType.Binary)
						{
							cols.Add($"CAST({db.Enquote(c2.Name)} AS CHAR(255))");
						}
						else
						{
							cols.Add(db.Enquote(c2.Name));
						}
					}
				}
			}
			else if (getAllFields)
			{
				cols.AddRange(Table.Columns.Select((IColumn c) => (!c.IsEmpty) ? db.Enquote(c.Name) : ("'" + c.ConstValue + "' as " + db.Enquote(c.Name))));
			}
			else
			{
				cols.AddRange(from c in Table.Columns
					where !c.IsEmpty && c.IsVisible
					select db.Enquote(c.Name));
			}
			if (cols.Count == 0)
			{
				cols.Add(db.Enquote("_row_no_"));
			}
			string creationString = string.Format("SELECT {0} FROM {1} AS t ", string.Join(", ", cols.Select((string c) => c)), db.Enquote(Table.UserDefined ? db.DbConfig.DbName : Table.Database, tname));
			List<string> conditions = new List<string>();
			List<string> limits = new List<string>();
			string indexValue = opt.FindValue(OptimizationType.IndexTable);
			string splitValue = opt.FindValue(OptimizationType.SplitTable);
			string sortValue = opt.FindValue(OptimizationType.SortColumn);
			limits.AddRange(from o in Table.OrderAreas.GetRanges(user.CurrentLanguage, indexValue, splitValue, sortValue)
				select string.Format("({0} BETWEEN {1} AND {2})", db.Enquote("_row_no_"), o.Item1, o.Item2));
			if (_multiOptimization)
			{
				limits.Clear();
				if (OptimizationSelected != null)
				{
					List<IOptimization> mandts = new List<IOptimization>();
					List<IOptimization> bukrs = new List<IOptimization>();
					List<IOptimization> gjahrs = new List<IOptimization>();
					int j;
					for (j = 1; j < OptimizationSelected.Count + 1; j++)
					{
						switch (j)
						{
						case 1:
							mandts = OptimizationCollection.Where((IOptimization o) => o.Id >= OptimizationSelected[j].Item1 && OptimizationSelected[j].Item2 >= o.Id).ToList();
							break;
						case 2:
							bukrs = OptimizationCollection.Where((IOptimization o) => o.Id >= OptimizationSelected[j].Item1 && OptimizationSelected[j].Item2 >= o.Id).ToList();
							break;
						case 3:
							gjahrs = OptimizationCollection.Where((IOptimization o) => o.Id >= OptimizationSelected[j].Item1 && OptimizationSelected[j].Item2 >= o.Id).ToList();
							break;
						}
					}
					foreach (IOptimization item in mandts)
					{
						indexValue = item.FindValue(OptimizationType.IndexTable);
						foreach (IOptimization item2 in bukrs)
						{
							splitValue = item2.FindValue(OptimizationType.SplitTable);
							foreach (IOptimization item3 in gjahrs)
							{
								sortValue = item3.FindValue(OptimizationType.SortColumn);
								foreach (string oFilter in from o in Table.OrderAreas.GetRanges(user.CurrentLanguage, indexValue, splitValue, sortValue)
									select string.Format("{0} BETWEEN {1} AND {2}", db.Enquote("_row_no_"), o.Item1, o.Item2))
								{
									if (!limits.Contains(oFilter))
									{
										limits.Add(oFilter);
									}
								}
							}
						}
					}
				}
			}
			string roleBasedSQL = ((OriginalTable != null) ? OriginalTable.GetBaseObject() : Table.GetBaseObject()).GetRoleBasedFilterSQL(user);
			if (roleBasedSQL != string.Empty && !conditions.Contains(roleBasedSQL))
			{
				conditions.Add(roleBasedSQL);
			}
			if (roleBasedOptimizationFilter != null && roleBasedOptimizationFilter != "")
			{
				conditions.Add(roleBasedOptimizationFilter);
				if (pageSystem != null)
				{
					pageSystem.RoleBasedOptimizationFilter = roleBasedOptimizationFilter;
				}
			}
			if (limits.Count > 0)
			{
				conditions.Add(string.Format("({0})", string.Join(" OR ", limits)));
			}
			if (conditions.Count > 0)
			{
				List<string> conds = new List<string>();
				conds.Add(conditions[0]);
				for (int i = 1; i < conditions.Count; i++)
				{
					bool ok = true;
					foreach (string cond in conds)
					{
						if (!cond.Contains("OR") && (cond.Contains(conditions[i]) || (conditions[i].First() == '(' && conditions[i].Last() == ')' && cond.Contains(conditions[i].Substring(1, conditions[i].Length - 2)))))
						{
							ok = false;
							break;
						}
					}
					if (ok)
					{
						conds.Add(conditions[i]);
					}
				}
				creationString = creationString + " WHERE " + string.Join(" AND ", conds);
			}
			if (HasDefaultOrderField && (Sum == null || Sum.Count == 0))
			{
				creationString += " ORDER BY ";
				if (GroupSubTotal != null && GroupSubTotal.GroupList != null && GroupSubTotal.GroupList.Any())
				{
					List<string> groupColumnList = new List<string>();
					foreach (string pair in GroupSubTotal.GroupList)
					{
						if (pair == string.Empty)
						{
							break;
						}
						IColumn column = (OriginalTable.Columns.Any() ? OriginalTable.Columns.FirstOrDefault((IColumn x) => x.Name == pair) : _table.Columns.FirstOrDefault((IColumn x) => x.Name == pair));
						if (column != null)
						{
							groupColumnList.Add(db.Enquote("t") + "." + db.Enquote(column.Name));
						}
					}
					if (groupColumnList.Count > 0)
					{
						string columns = string.Join(",", groupColumnList);
						if (!creationString.Contains(columns))
						{
							creationString = creationString + columns + " , ";
						}
					}
				}
				creationString += "`_row_no_`";
			}
			if (Sum == null || Sum.Count == 0)
			{
				creationString += $" LIMIT {startRow}, {limit}";
			}
			return GetSimpleQuery(creationString);
		}

		public string GetSelectionString(DatabaseBase db, string database)
		{
			string tname = Table.TableName;
			List<string> cols = new List<string>();
			foreach (IColumn c2 in from c in Table.Columns
				where c.IsVisible && !c.IsEmpty
				select c into x
				orderby x.Ordinal
				select x)
			{
				if (c2.DataType == SqlType.Binary)
				{
					cols.Add($"CAST({db.Enquote(c2.Name)} AS CHAR(255))");
				}
				else
				{
					cols.Add(db.Enquote(c2.Name));
				}
			}
			string creationString = string.Format("SELECT {0} FROM {1}", string.Join(", ", cols.Select((string c) => c)), db.Enquote(Table.UserDefined ? db.DbConfig.DbName : database, tname));
			if (HasDefaultOrderField)
			{
				creationString += " ORDER BY _row_no_";
			}
			return GetSimpleQuery(creationString);
		}

		private string GetSimpleQuery(string sql)
		{
			string tempSql = sql;
			tempSql = Regex.Replace(tempSql, "null is null", "true", RegexOptions.IgnoreCase);
			tempSql = Regex.Replace(tempSql, "'' is null", "false", RegexOptions.IgnoreCase);
			tempSql = Regex.Replace(tempSql, "ifnull\\('', ''\\) = ''", "true", RegexOptions.IgnoreCase);
			tempSql = Regex.Replace(tempSql, "''=''", "true", RegexOptions.IgnoreCase);
			tempSql = Regex.Replace(tempSql, "'' = ''", "true", RegexOptions.IgnoreCase);
			tempSql = Regex.Replace(tempSql, "true and ", "", RegexOptions.IgnoreCase);
			tempSql = Regex.Replace(tempSql, "(true) and ", "", RegexOptions.IgnoreCase);
			tempSql = Regex.Replace(tempSql, "false or ", "", RegexOptions.IgnoreCase);
			tempSql = Regex.Replace(tempSql, " or false", "", RegexOptions.IgnoreCase);
			tempSql = Regex.Replace(tempSql, "(false) or ", "", RegexOptions.IgnoreCase);
			tempSql = Regex.Replace(tempSql, " or (false)", "", RegexOptions.IgnoreCase);
			tempSql = Regex.Replace(tempSql, "!= '∅'", " IS NOT NULL ", RegexOptions.IgnoreCase);
			tempSql = Regex.Replace(tempSql, "= '∅'", " IS NULL ", RegexOptions.IgnoreCase);
			tempSql = tempSql.Replace("t.CAST(", "CAST(");
			_log.Debug($"SQL Was running - {tempSql}");
			return tempSql;
		}

		private IOptimization GetOptimizationBySystem(IOptimization optimization)
		{
			if (optimization.Group.Type == OptimizationType.System)
			{
				return optimization;
			}
			return GetOptimizationBySystem(optimization.Parent);
		}
	}
}
