using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SystemDb;
using SystemDb.Internal;
using DbAccess;
using ViewboxDb;

namespace Viewbox.Models
{
	public class RowVisibilityHelper
	{
		private static List<Task> tasks = new List<Task>();

		private static ConcurrentBag<RowVisibilityCountCache> rowCounts = new ConcurrentBag<RowVisibilityCountCache>();

		private static IDataObjectCollection<ITableObject> SwitchCollections(Type typE)
		{
			if (typE == typeof(View))
			{
				return ViewboxApplication.Database.SystemDb.Views;
			}
			if (typE == typeof(Table))
			{
				return ViewboxApplication.Database.SystemDb.Tables;
			}
			if (typE == typeof(Issue))
			{
				return ViewboxApplication.Database.SystemDb.Issues;
			}
			return null;
		}

		public static void SetRoleBasedFilter(int tableId, int columnId, string filterValue)
		{
			ITableObject t = null;
			int roleId = ViewboxApplication.Roles[ViewboxSession.RightsModeCredential.Id].Id;
			IColumn col;
			if (tableId > 0)
			{
				t = ViewboxSession.TableObjects[tableId];
				col = t.Columns.FirstOrDefault((IColumn c) => c.Id == columnId);
			}
			else
			{
				ViewboxDb.TableObject tempTo = ViewboxSession.TempTableObjects[tableId];
				t = ((SystemDb.Internal.TableObject)tempTo.OriginalTable).GetBaseObject();
				IColumn tempCol = tempTo.Table.Columns.FirstOrDefault((IColumn c) => c != null && c.Id == columnId);
				col = t.Columns.Where((IColumn x) => x.Name == tempCol.Name).First();
			}
			columnId = col.Id;
			tableId = t.Id;
			filterValue = filterValue.Replace("∅", "");
			if (col != null && filterValue != "undefined")
			{
				if (string.IsNullOrEmpty(filterValue))
				{
					filterValue = "";
				}
				else
				{
					switch (col.DataType)
					{
					case SqlType.Date:
					case SqlType.DateTime:
					{
						if (DateTime.TryParse(filterValue, out var valueDateTime))
						{
							filterValue = valueDateTime.ToString(CultureInfo.InvariantCulture);
							break;
						}
						throw new Exception("Could not convert value into required format.");
					}
					case SqlType.Time:
					{
						if (TimeSpan.TryParse(filterValue, out var tempTime))
						{
							filterValue = tempTime.ToString("c", CultureInfo.InvariantCulture);
							break;
						}
						throw new Exception("Could not convert value into required format.");
					}
					case SqlType.Decimal:
					{
						if (double.TryParse(filterValue, out var toDouble))
						{
							filterValue = toDouble.ToString("F" + col.MaxLength, CultureInfo.InvariantCulture);
							break;
						}
						throw new Exception("Could not convert value into required format.");
					}
					}
				}
				IDataObjectCollection<ITableObject> dOc = SwitchCollections(t.GetType());
				if (dOc[tableId].RoleBasedFilters == null)
				{
					dOc[tableId].RoleBasedFilters = new Dictionary<int, List<Tuple<int, List<string>>>>();
				}
				using (DatabaseBase db = ViewboxApplication.Database.ConnectionManager.GetConnection())
				{
					db.ExecuteNonQuery($"DELETE FROM {db.DbConfig.DbName}.`row_visibility_count_cache` WHERE `table_id` = {tableId} AND `role_id` = {roleId};");
					t.RoleBasedFilterRowCount = new List<RowVisibilityCountCache>();
					rowCounts = new ConcurrentBag<RowVisibilityCountCache>();
					string sql = "SELECT * FROM " + db.DbConfig.DbName + ".row_visibility WHERE table_id = " + tableId + " AND role_id = " + roleId + " AND column_id = " + columnId + " AND filter_value = '" + filterValue + "'";
					using IDataReader reader = db.ExecuteReader(sql);
					if (!reader.Read())
					{
						reader.Close();
						sql = "INSERT INTO " + db.DbConfig.DbName + ".row_visibility (table_id, role_id, column_id, filter_value) VALUES (" + tableId + ", " + roleId + ", " + columnId + ", '" + filterValue + "')";
						db.ExecuteNonQuery(sql);
						if (!dOc[tableId].RoleBasedFilters.ContainsKey(roleId))
						{
							dOc[tableId].RoleBasedFilters.Add(roleId, new List<Tuple<int, List<string>>>());
						}
						if (dOc[tableId].RoleBasedFilters[roleId].Any((Tuple<int, List<string>> n) => n.Item1 == columnId))
						{
							dOc[tableId].RoleBasedFilters[roleId].Single((Tuple<int, List<string>> n) => n.Item1 == columnId).Item2.Add(filterValue);
						}
						else
						{
							dOc[tableId].RoleBasedFilters[roleId].Add(new Tuple<int, List<string>>(columnId, new List<string> { filterValue }));
						}
						t.RoleBasedFilters = dOc[tableId].RoleBasedFilters;
						return;
					}
					reader.Close();
					sql = "DELETE FROM " + db.DbConfig.DbName + ".row_visibility WHERE table_id = " + tableId + " AND role_id = " + roleId + " AND column_id = " + columnId + " AND filter_value = '" + filterValue + "'";
					db.ExecuteNonQuery(sql);
					dOc[tableId].RoleBasedFilters[roleId].Single((Tuple<int, List<string>> n) => n.Item1 == columnId).Item2.Remove(filterValue);
					if (dOc[tableId].RoleBasedFilters[roleId].Single((Tuple<int, List<string>> n) => n.Item1 == columnId).Item2.Count == 0)
					{
						Tuple<int, List<string>> toRemove = dOc[tableId].RoleBasedFilters[roleId].Single((Tuple<int, List<string>> n) => n.Item1 == columnId);
						dOc[tableId].RoleBasedFilters[roleId].Remove(toRemove);
					}
					if (dOc[tableId].RoleBasedFilters[roleId].Count() == 0)
					{
						dOc[tableId].RoleBasedFilters.Remove(roleId);
					}
					t.RoleBasedFilters = dOc[tableId].RoleBasedFilters;
				}
				return;
			}
			throw new Exception("No such column found");
		}

		public static ConcurrentBag<RowVisibilityCountCache> ComputeRowCounts(ITableObject t, int roleId, int origiTableId)
		{
			IColumnCollection columns = t.Columns;
			List<Tuple<int, List<string>>> roleBasedFilters = t.RoleBasedFilters[roleId].Where((Tuple<int, List<string>> x) => columns.Contains(x.Item1)).ToList();
			List<Tuple<string, string>> roleBasedFiltersNew = ((t.RoleBasedFiltersNew != null && t.RoleBasedFiltersNew.Count > 0) ? t.RoleBasedFiltersNew[roleId].Where((Tuple<string, string> x) => columns.Contains(x.Item1)).ToList() : null);
			IOptimization currentSystem = ViewboxSession.Optimizations.LastOrDefault().GetOptimization(OptimizationType.System);
			ConcurrentDictionary<string, long> calculatedRowCounts = new ConcurrentDictionary<string, long>();
			RecursiveSol(currentSystem, t, origiTableId, columns, roleBasedFilters, roleBasedFiltersNew, roleId, calculatedRowCounts);
			using (DatabaseBase db = ViewboxApplication.Database.ConnectionManager.GetConnection())
			{
				StringBuilder sql = new StringBuilder($"INSERT INTO `{db.DbConfig.DbName}`.`row_visibility_count_cache` (role_id, table_id, opt_id, count, id) VALUES ");
				foreach (RowVisibilityCountCache item in rowCounts)
				{
					sql.Append($"({item.RoleId}, {item.TableId}, {item.OptimizationId}, {item.Count}, null),");
				}
				sql.Length--;
				db.ExecuteNonQuery(sql.ToString());
			}
			return rowCounts;
		}

		private static void RecursiveSol(IOptimization opt, ITableObject t, int origiTableId, IEnumerable<IColumn> columns, IEnumerable<Tuple<int, List<string>>> roleBasedFilters, IEnumerable<Tuple<string, string>> roleBasedFiltersNew, int roleId, ConcurrentDictionary<string, long> areaCache)
		{
			foreach (IOptimization levelX in opt.Children)
			{
				if (levelX.Children != null && levelX.Children.Count > 0)
				{
					RecursiveSol(levelX, t, origiTableId, columns, roleBasedFilters, roleBasedFiltersNew, roleId, areaCache);
				}
				else
				{
					rowCounts.Add(GetCountCache((levelX.Parent == null) ? null : levelX, t, origiTableId, columns, roleBasedFilters, roleBasedFiltersNew, roleId, areaCache));
				}
			}
		}

		private static long ComputeRowCount(ITableObject t, IEnumerable<IColumn> columns, IEnumerable<Tuple<int, List<string>>> roleBasedFilters, IEnumerable<Tuple<string, string>> roleBasedFiltersNew, int roleId, List<IOrderArea> areas, bool? decision = null)
		{
			StringBuilder columnFilters = new StringBuilder();
			foreach (IColumn col in columns)
			{
				if (roleBasedFilters.SingleOrDefault((Tuple<int, List<string>> c) => c.Item1 == col.Id) != null)
				{
					columnFilters.Append(col.Name + " NOT IN('" + string.Join("','", roleBasedFilters.Single((Tuple<int, List<string>> c) => c.Item1 == col.Id).Item2.ToArray()) + "') AND ");
				}
				if (roleBasedFiltersNew != null && roleBasedFiltersNew.SingleOrDefault((Tuple<string, string> c) => c.Item1 == col.Name) != null)
				{
					columnFilters.Append(col.Name + " IN('" + string.Join("','", roleBasedFiltersNew.Single((Tuple<string, string> c) => c.Item1 == col.Name).Item2.Split(',')) + "') AND ");
				}
			}
			if (columnFilters.Length == 0)
			{
				return t.RowCount;
			}
			StringBuilder oaFilters = new StringBuilder();
			foreach (IOrderArea o in areas)
			{
				oaFilters.Append(string.Format("({0} BETWEEN {1} AND {2}) OR ", "_row_no_", o.Start, o.End));
			}
			if (oaFilters.Length > 0)
			{
				oaFilters.Length -= 4;
			}
			using DatabaseBase db = ViewboxApplication.Database.SystemDb.ConnectionManager.GetConnection();
			StringBuilder sql = new StringBuilder("");
			if (oaFilters.Length > 0)
			{
				sql.Append("SELECT COUNT(*) FROM " + t.Name + " WHERE " + columnFilters.ToString() + "(" + oaFilters.ToString() + ")");
			}
			else
			{
				sql.Append("SELECT COUNT(*) FROM " + t.Name + " WHERE " + columnFilters.ToString());
				sql.Length -= 4;
			}
			return long.Parse(db.ExecuteScalar(sql.ToString()).ToString());
		}

		private static RowVisibilityCountCache GetCountCache(IOptimization opt, ITableObject t, int origiTableId, IEnumerable<IColumn> columns, IEnumerable<Tuple<int, List<string>>> roleBasedFilters, IEnumerable<Tuple<string, string>> roleBasedFiltersNew, int roleId, ConcurrentDictionary<string, long> areaCache)
		{
			RowVisibilityCountCache countCache = new RowVisibilityCountCache();
			if (opt != null)
			{
				string indexValue = opt.GetOptimizationValue(OptimizationType.IndexTable);
				string splitValue = opt.GetOptimizationValue(OptimizationType.SplitTable);
				string sortValue = opt.GetOptimizationValue(OptimizationType.SortColumn);
				List<IOrderArea> areas = t.OrderAreas.GetOrderAreas(ViewboxSession.Language.CountryCode, indexValue, splitValue, sortValue);
				countCache.OptimizationId = opt.Id.ToString();
				countCache.RoleId = roleId.ToString();
				countCache.TableId = origiTableId;
				string areaId = string.Join("", (from o in areas
					orderby o.Id
					select o.Id.ToString()).ToArray());
				if (areaCache.Keys.Contains(areaId))
				{
					long cnt = 0L;
					areaCache.TryGetValue(areaId, out cnt);
					countCache.Count = cnt;
				}
				else
				{
					countCache.Count = ComputeRowCount(t, columns, roleBasedFilters, roleBasedFiltersNew, roleId, areas);
					areaCache.TryAdd(areaId, countCache.Count);
				}
			}
			else
			{
				countCache.OptimizationId = opt.Id.ToString();
				countCache.RoleId = roleId.ToString();
				countCache.TableId = origiTableId;
				countCache.Count = t.PageSystem.Count;
			}
			return countCache;
		}
	}
}
