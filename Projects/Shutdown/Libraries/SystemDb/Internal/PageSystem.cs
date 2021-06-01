using System;
using System.Collections.Generic;
using System.Linq;
using AV.Log;
using DbAccess;

namespace SystemDb.Internal
{
	public class PageSystem
	{
		private int _optimalizationId;

		private string _roleBasedOptimizationFilter = "";

		private string _sql;

		public long Start { get; set; }

		public string Filter { get; set; }

		public string OrderBy { get; set; }

		public int OptimalizationId
		{
			get
			{
				return _optimalizationId;
			}
			set
			{
				if (OptimalizationId != value)
				{
					_optimalizationId = value;
					Count = 0L;
					IsEnded = false;
				}
			}
		}

		private string TempDbName { get; set; }

		public string RoleBasedOptimizationFilter
		{
			get
			{
				return _roleBasedOptimizationFilter;
			}
			set
			{
				if (_roleBasedOptimizationFilter != value)
				{
					if (!string.IsNullOrEmpty(_roleBasedOptimizationFilter) && Sql.Contains(_roleBasedOptimizationFilter))
					{
						Sql.Replace(_roleBasedOptimizationFilter, "");
					}
					_roleBasedOptimizationFilter = value;
					Sql = Sql + " AND " + _roleBasedOptimizationFilter;
				}
			}
		}

		public long Count { get; set; }

		public bool IsEnded { get; set; }

		public string Sql
		{
			get
			{
				return _sql;
			}
			set
			{
				_sql = value;
			}
		}

		public long CountStep { get; set; }

		public DatabaseBase Conn { get; set; }

		public PageSystem(string rowSql, int optId, string tempDbName)
		{
			Count = 0L;
			Sql = rowSql;
			IsEnded = false;
			_optimalizationId = optId;
			TempDbName = tempDbName;
		}

		public void CountTable(DatabaseBase database, ITableObject to = null)
		{
			if (IsEnded || database == null || Sql == null)
			{
				return;
			}
			try
			{
				if (CountStep > 0)
				{
					Count = (long)database.ExecuteScalar($"SELECT count(*) FROM ({Sql} LIMIT 0, {CountStep}) s");
					LogHelper.GetLogger().Info("PageSystem: " + Sql);
					IsEnded = Count < CountStep;
				}
				else
				{
					Count = (long)database.ExecuteScalar($"SELECT count(*) FROM ({Sql}) s");
					LogHelper.GetLogger().Info("PageSystem: " + Sql);
					IsEnded = true;
				}
			}
			catch (Exception)
			{
				Count = 0L;
				IsEnded = true;
			}
		}

		public void UpdateSQL(ITableObject table, ITableObject original, IEnumerable<IOrderArea> list, IUser user, IOptimizationCollection optimizations, bool MultiOptimizations = false, IDictionary<int, Tuple<int, int>> OptimizationSelected = null)
		{
			string database = (table.UserDefined ? TempDbName : table.Database);
			Issue t = table as Issue;
			if (t != null)
			{
				database = ((t.IssueType == IssueType.StoredProcedure) ? user.UserTable(database) : database);
			}
			string checkTableExistSql = $"select count(1) from `information_schema`.`TABLES` t where t.`table_schema` = '{database}' and t.`TABLE_NAME` = '{table.TableName}'";
			int result = 2;
			if (Conn != null && int.TryParse(Conn.ExecuteScalar(checkTableExistSql).ToString(), out var actValue))
			{
				result = actValue;
			}
			if (result < 1)
			{
				database = table.Database;
			}
			string sql = $"SELECT `_row_no_` FROM `{database}`.`{table.TableName}` WHERE ";
			string sqlOrderAreas = string.Empty;
			string sqlFilter = string.Empty;
			foreach (IOrderArea o2 in list)
			{
				if (sqlOrderAreas != string.Empty)
				{
					sqlOrderAreas += " OR ";
				}
				sqlOrderAreas += string.Format("{0} BETWEEN {1} AND {2}", "`_row_no_`", o2.Start, o2.End);
			}
			if (MultiOptimizations)
			{
				List<string> areas = new List<string>();
				sqlOrderAreas = string.Empty;
				if (OptimizationSelected != null)
				{
					List<IOptimization> mandts = new List<IOptimization>();
					List<IOptimization> bukrs = new List<IOptimization>();
					List<IOptimization> gjahrs = new List<IOptimization>();
					IOptimization opt = optimizations.FirstOrDefault((IOptimization x) => x.Id == OptimalizationId);
					IOptimization system = null;
					if (opt != null)
					{
						system = GetOptimizationBySystem(opt);
					}
					int i;
					for (i = 1; i < OptimizationSelected.Count + 1; i++)
					{
						switch (i)
						{
						case 1:
							if (system != null)
							{
								List<IOptimization> mandtsList = system.Children.OrderBy((IOptimization o) => o.Value).ToList();
								IOptimization fromMandts = mandtsList.FirstOrDefault((IOptimization o) => o.Id == OptimizationSelected[i].Item1);
								IOptimization toMandts = mandtsList.FirstOrDefault((IOptimization o) => o.Id == OptimizationSelected[i].Item2);
								if (fromMandts != null && toMandts != null)
								{
									int startIndex = mandtsList.ToList().IndexOf(fromMandts);
									int endIndex = mandtsList.ToList().IndexOf(toMandts);
									startIndex = MakeOrderSwapIndex(startIndex, ref endIndex);
									int endCount = endIndex - startIndex;
									mandts = mandtsList.Skip(startIndex).Take(endCount + 1).ToList();
								}
							}
							break;
						case 2:
							if (mandts.Any())
							{
								List<IOptimization> bukrsList = OrderOneLevelDown(mandts.OrderBy((IOptimization x) => x.Value).ToList()).ToList();
								IOptimization fromBukrs = bukrsList.FirstOrDefault((IOptimization o) => o.Id == OptimizationSelected[i].Item1);
								IOptimization toBukrs = bukrsList.FirstOrDefault((IOptimization o) => o.Id == OptimizationSelected[i].Item2);
								if (fromBukrs != null && toBukrs != null)
								{
									int startIndex2 = bukrsList.IndexOf(fromBukrs);
									int endIndex2 = bukrsList.IndexOf(toBukrs);
									startIndex2 = MakeOrderSwapIndex(startIndex2, ref endIndex2);
									int endCount2 = endIndex2 - startIndex2;
									bukrs = bukrsList.Skip(startIndex2).Take(endCount2 + 1).ToList();
								}
							}
							break;
						case 3:
							if (bukrs.Any())
							{
								List<IOptimization> gjahrsList = OrderOneLevelDown(bukrs).ToList();
								IOptimization fromGjahrs = gjahrsList.FirstOrDefault((IOptimization o) => o.Id == OptimizationSelected[i].Item1);
								IOptimization toGjahrs = gjahrsList.FirstOrDefault((IOptimization o) => o.Id == OptimizationSelected[i].Item2);
								if (fromGjahrs != null && toGjahrs != null)
								{
									int startIndex3 = gjahrsList.ToList().IndexOf(fromGjahrs);
									int endIndex3 = gjahrsList.ToList().IndexOf(toGjahrs);
									startIndex3 = MakeOrderSwapIndex(startIndex3, ref endIndex3);
									int endCount3 = endIndex3 - startIndex3;
									gjahrs = gjahrsList.Skip(startIndex3).Take(endCount3 + 1).ToList();
								}
							}
							break;
						}
					}
					string sortValue = string.Empty;
					foreach (IOptimization item in mandts)
					{
						string indexValue = item.FindValue(OptimizationType.IndexTable);
						foreach (IOptimization item2 in bukrs)
						{
							string splitValue = item2.FindValue(OptimizationType.SplitTable);
							foreach (IOptimization item3 in gjahrs)
							{
								sortValue = item3.FindValue(OptimizationType.SortColumn);
								foreach (string oFilter3 in from o in table.OrderAreas.GetRanges(user.CurrentLanguage, indexValue, splitValue, sortValue)
									select string.Format("{0} BETWEEN {1} AND {2}", "`_row_no_`", o.Item1, o.Item2))
								{
									if (!areas.Contains(oFilter3))
									{
										areas.Add(oFilter3);
									}
								}
							}
							if (gjahrs.Count != 0)
							{
								continue;
							}
							foreach (string oFilter2 in from o in table.OrderAreas.GetRanges(user.CurrentLanguage, indexValue, splitValue, null)
								select string.Format("{0} BETWEEN {1} AND {2}", "`_row_no_`", o.Item1, o.Item2))
							{
								if (!areas.Contains(oFilter2))
								{
									areas.Add(oFilter2);
								}
							}
						}
						if (bukrs.Count != 0)
						{
							continue;
						}
						foreach (string oFilter in from o in table.OrderAreas.GetRanges(user.CurrentLanguage, indexValue, null, null)
							select string.Format("{0} BETWEEN {1} AND {2}", "`_row_no_`", o.Item1, o.Item2))
						{
							if (!areas.Contains(oFilter))
							{
								areas.Add(oFilter);
							}
						}
					}
					sqlOrderAreas = string.Join(" OR ", areas.ToArray());
				}
			}
			if (sqlOrderAreas != string.Empty)
			{
				sqlFilter = sqlFilter + "(" + sqlOrderAreas + ") ";
			}
			if (!string.IsNullOrEmpty(Filter))
			{
				sqlFilter = ((!(sqlFilter != string.Empty)) ? (sqlFilter + $" ({Filter})") : $"({sqlFilter}) AND ({Filter})");
			}
			if (table.Database.ToLower() != TempDbName && sqlFilter != "")
			{
				sql = (Sql = sql + sqlFilter);
			}
			else
			{
				string orderAreas = string.Empty;
				if (sqlOrderAreas != string.Empty)
				{
					orderAreas = "(" + sqlOrderAreas + ") ";
					sql = (Sql = sql + orderAreas);
				}
				else
				{
					sql = sql.TrimEnd();
					string removeWhere = "WHERE";
					sql = sql.TrimEnd();
					if (sql.EndsWith(removeWhere))
					{
						sql = sql.Substring(0, sql.Length - removeWhere.Length);
					}
					Sql = sql;
				}
			}
			string roleBasedSQL = original.GetRoleBasedFilterSQL(user);
			if (roleBasedSQL != string.Empty)
			{
				Sql += (Sql.Contains("WHERE") ? (" AND " + roleBasedSQL) : (" WHERE " + roleBasedSQL));
			}
			IsEnded = false;
		}

		private static int MakeOrderSwapIndex(int startIndex, ref int endIndex)
		{
			if (startIndex > endIndex)
			{
				int tempIndex = startIndex;
				startIndex = endIndex;
				endIndex = tempIndex;
			}
			return startIndex;
		}

		public List<IOptimization> OrderOneLevelDown(List<IOptimization> parentOptimizationList)
		{
			List<IOptimization> childLevelList = new List<IOptimization>();
			foreach (IOptimization parentOptimization in parentOptimizationList)
			{
				IOrderedEnumerable<IOptimization> OptValues = parentOptimization.Children.OrderBy((IOptimization x) => x.Value);
				childLevelList.AddRange(OptValues);
			}
			return childLevelList;
		}

		public PageSystem Clone()
		{
			return (PageSystem)MemberwiseClone();
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
