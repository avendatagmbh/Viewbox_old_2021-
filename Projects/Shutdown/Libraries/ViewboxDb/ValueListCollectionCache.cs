using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using SystemDb;
using AV.Log;
using DbAccess;

namespace ViewboxDb
{
	public class ValueListCollectionCache
	{
		private const int MaxCachedPages = 5;

		public static Dictionary<int, ValueListCollection> CachedPages = new Dictionary<int, ValueListCollection>();

		private readonly int _hash;

		private readonly string _table;

		private readonly TempDb _tempDb;

		public int UseCount;

		public int RowCount { get; set; }

		public ValueListCollectionCache(int hash, string table, TempDb tempDb)
		{
			_hash = hash;
			_table = table;
			_tempDb = tempDb;
			using DatabaseBase connection = tempDb.ConnectionManager.GetConnection();
			object count = connection.ExecuteScalar(new StringBuilder().Append("SELECT COUNT(ID) FROM ").Append(connection.Enquote(_table)).ToString());
			if (count != null)
			{
				RowCount = int.Parse(count.ToString());
			}
		}

		public static ValueListCollection GetRealPage(IIssue issue, int parameterId, out int valueCount, out bool hasDescription, IOptimization optimization, TempDb tempDb, string viewboxDbName, int from, int size, int uid, string userID, string roleID, string allowedIndex, string allowedSplit, string allowedSort, string languageCode = "de", string search = "", bool exactMatch = false, int columnId = -1, int tableId = -1, SortDirection direction = SortDirection.Ascending, string[] selectedParamValues = null, bool oldSmallBooks = true, int limit = 150)
		{
			IParameter parameter = issue.Parameters.Single((IParameter p) => p.Id == parameterId);
			bool indexBlocked = issue.Parameters.Any((IParameter p) => p.OptimizationType == OptimizationType.IndexTable);
			bool splitBlocked = issue.Parameters.Any((IParameter p) => p.OptimizationType == OptimizationType.SplitTable);
			bool sortBlocked = issue.Parameters.Any((IParameter p) => p.OptimizationType == OptimizationType.SortColumn);
			string columnName = string.Empty;
			_ = string.Empty;
			_ = string.Empty;
			string date = DateFormat(issue, parameterId, search);
			string viewboxIndexDbName = viewboxDbName + "_index";
			(from p in issue.Parameters
				group p by p.TableName).Count();
			if (issue.Parameters.Count((IParameter p) => p.ColumnName == parameter.ColumnName) > 1 && (parameter.Name.ToLower().Contains("to") || parameter.Name.ToLower().Contains("bis")))
			{
				_ = parameter.GroupId > 0;
			}
			else
				_ = 0;
			valueCount = 0;
			ValueListCollection valueListCollection = new ValueListCollection();
			if (parameter.DataType == SqlType.Date && date != null && exactMatch)
			{
				search = date.ToString();
			}
			if (columnId == -1 || tableId == -1)
			{
				GetIdsForIndex(out tableId, out columnId, parameterId, viewboxDbName, tempDb);
			}
			if (parameter != null)
			{
				columnName = parameter.ColumnName;
				_ = parameter.TableName;
				_ = parameter.DatabaseName;
			}
			hasDescription = HasDescription(tempDb, viewboxIndexDbName, columnId);
			try
			{
				string roleBasedFilter = "";
				IDictionary<int, List<Tuple<string, string>>> actualRoleBasedFiltersNew = issue.RoleBasedFiltersNew;
				if (issue != null && issue.FilterTableObject != null && issue.FilterTableObject.RoleBasedFiltersNew != null)
				{
					actualRoleBasedFiltersNew = issue.FilterTableObject.RoleBasedFiltersNew;
					LogHelper.GetLogger().Info("From FilterObject");
				}
				else
				{
					LogHelper.GetLogger().Info("Not from FilterObject");
				}
				if (actualRoleBasedFiltersNew != null)
				{
					LogHelper.GetLogger().Info("Rolebasedfilter exists");
					foreach (KeyValuePair<int, List<Tuple<string, string>>> filterNew in actualRoleBasedFiltersNew)
					{
						if (!(roleID != "") || !roleID.Split(',').Contains(filterNew.Key.ToString()))
						{
							continue;
						}
						LogHelper.GetLogger().Info("Role " + filterNew.Key + " contains");
						foreach (IGrouping<string, Tuple<string, string>> value in from x in filterNew.Value
							group x by x.Item1)
						{
							LogHelper.GetLogger().Info("In foreach: col" + columnName + ", value.key: " + value.Key + ", value.Value " + string.Join(";", value.ToList()));
							if (!(columnName.ToLower() == value.Key.ToLower()))
							{
								continue;
							}
							LogHelper.GetLogger().Info("(if okay) Column " + columnName + " exists");
							roleBasedFilter += "V.value IN (";
							foreach (Tuple<string, string> item2 in value.ToList())
							{
								string[] array = item2.Item2.Split(',');
								foreach (string item in array)
								{
									roleBasedFilter = ((!int.TryParse(item, out var _)) ? (roleBasedFilter + (roleBasedFilter.ToString().EndsWith("(") ? "" : ",") + "'" + item + "'") : (roleBasedFilter + (roleBasedFilter.ToString().EndsWith("(") ? "" : ",") + item));
								}
							}
							roleBasedFilter += ") ";
						}
					}
				}
				LogHelper.GetLogger().Info("Rolebasedfilter: " + roleBasedFilter);
				using DatabaseBase connection = tempDb.ConnectionManager.GetConnection();
				StringBuilder selectSql = new StringBuilder();
				StringBuilder countSql = new StringBuilder();
				int to = ((from + size > limit) ? (size - 1) : size);
				selectSql.Append("SELECT COUNT(*) FROM `" + viewboxIndexDbName + "`.`INDEX_" + columnId + "`;");
				long indexCount = long.Parse(connection.ExecuteScalar(selectSql.ToString()).ToString());
				selectSql.Clear();
				if (columnId == -1 || tableId == -1 || indexCount <= 0)
				{
					selectSql.Append(string.Format("SELECT DISTINCT V.id, V.value{0} FROM `{1}`.`VALUE_{2}` V ", hasDescription ? ", V.description " : "", viewboxIndexDbName, columnId));
					countSql.Append(string.Format("SELECT COUNT(DISTINCT v.value) FROM `{1}`.`VALUE_{2}` V ", hasDescription ? ", V.description " : "", viewboxIndexDbName, columnId));
					if (!string.IsNullOrEmpty(search))
					{
						search = ((parameter.DataType == SqlType.Decimal) ? DecimalFormat(issue, parameterId, search) : search);
						if (hasDescription)
						{
							selectSql.Append("WHERE (");
							selectSql.Append("V.value" + (exactMatch ? " = " : " LIKE ") + (exactMatch ? ("'" + search + "'") : ("'%" + search + "%'")));
							selectSql.Append(" OR V.description" + (exactMatch ? " = " : " LIKE ") + (exactMatch ? ("'" + search + "'") : ("'%" + search + "%'")));
							selectSql.Append(")");
							countSql.Append("WHERE (");
							countSql.Append("V.value" + (exactMatch ? " = " : " LIKE ") + (exactMatch ? ("'" + search + "'") : ("'%" + search + "%'")));
							countSql.Append(" OR V.description" + (exactMatch ? " = " : " LIKE ") + (exactMatch ? ("'" + search + "'") : ("'%" + search + "%'")));
							countSql.Append(")");
						}
						else
						{
							selectSql.Append(string.Format(" WHERE {0} {1} {2} ", (!(languageCode == "de")) ? (exactMatch ? "V.value" : ((parameter.DataType == SqlType.Date) ? "CAST(DATE_FORMAT(V.value, '%Y-%m-%d') as CHAR(20))" : "V.value")) : (exactMatch ? "V.value" : ((parameter.DataType == SqlType.Date) ? "CAST(DATE_FORMAT(V.value, '%d.%m.%Y') as CHAR(20))" : "V.value")), exactMatch ? " = " : " LIKE ", exactMatch ? ("'" + search + "'") : ("'%" + search + "%'")));
							countSql.Append(string.Format(" WHERE {0} {1} {2} ", (!(languageCode == "de")) ? (exactMatch ? "V.value" : ((parameter.DataType == SqlType.Date) ? "CAST(DATE_FORMAT(V.value, '%Y-%m-%d') as CHAR(20))" : "V.value")) : (exactMatch ? "V.value" : ((parameter.DataType == SqlType.Date) ? "CAST(DATE_FORMAT(V.value, '%d.%m.%Y') as CHAR(20))" : "V.value")), exactMatch ? " = " : " LIKE ", exactMatch ? ("'" + search + "'") : ("'%" + search + "%'")));
						}
					}
					if (!string.IsNullOrEmpty(roleBasedFilter))
					{
						if (selectSql.ToString().Contains("WHERE"))
						{
							selectSql.Append(" AND ");
						}
						else
						{
							selectSql.Append(" WHERE ");
						}
						selectSql.Append(roleBasedFilter);
						if (countSql.ToString().Contains("WHERE"))
						{
							countSql.Append(" AND ");
						}
						else
						{
							countSql.Append(" WHERE ");
						}
						countSql.Append(roleBasedFilter);
					}
					if (parameter.OptimizationType == OptimizationType.SplitTable && allowedSplit != "")
					{
						if (selectSql.ToString().Contains("WHERE"))
						{
							selectSql.Append(" AND ");
						}
						else
						{
							selectSql.Append(" WHERE ");
						}
						selectSql.Append("V.value IN( SELECT value FROM `" + viewboxDbName + "`.`optimizations` WHERE id in(" + allowedSplit + "))");
						if (countSql.ToString().Contains("WHERE"))
						{
							countSql.Append(" AND ");
						}
						else
						{
							countSql.Append(" WHERE ");
						}
						countSql.Append("V.value IN( SELECT value FROM `" + viewboxDbName + "`.`optimizations` WHERE id in(" + allowedSplit + "))");
					}
					if (parameter.OptimizationType == OptimizationType.SortColumn && allowedSort != "")
					{
						if (selectSql.ToString().Contains("WHERE"))
						{
							selectSql.Append(" AND ");
						}
						else
						{
							selectSql.Append(" WHERE ");
						}
						selectSql.Append("V.value IN( SELECT value FROM `" + viewboxDbName + "`.`optimizations` WHERE id in(" + allowedSort + "))");
						if (countSql.ToString().Contains("WHERE"))
						{
							countSql.Append(" AND ");
						}
						else
						{
							countSql.Append(" WHERE ");
						}
						countSql.Append("V.value IN( SELECT value FROM `" + viewboxDbName + "`.`optimizations` WHERE id in(" + allowedSort + "))");
					}
					selectSql.Append(" GROUP BY v.value ");
					selectSql.Append($" LIMIT {from}, {to};");
					LogHelper.GetLogger().Info("Smallbook - something is missing: " + selectSql.ToString());
					LogHelper.GetLogger().Info("Smallbook - something is missing: " + countSql.ToString());
					using (IDataReader reader2 = connection.ExecuteReader(selectSql.ToString()))
					{
						while (reader2.Read())
						{
							valueListCollection.Add(new ValueListElement
							{
								Id = reader2.GetInt32(0),
								Value = reader2.GetValue(1),
								Description = ((hasDescription && !reader2.IsDBNull(2)) ? reader2.GetString(2) : "")
							});
						}
					}
					valueCount = int.Parse(connection.ExecuteScalar(countSql.ToString()).ToString());
					return valueListCollection;
				}
				selectSql.Clear();
				countSql.Clear();
				selectSql.Append($"SELECT V.`id`, V.`value`");
				countSql.Append($"SELECT COUNT(DISTINCT v.value)");
				if (hasDescription)
				{
					selectSql.Append($", V.`description` ");
				}
				selectSql.Append(string.Format(" FROM `" + viewboxIndexDbName + "`.`VALUE_" + columnId + "` V "));
				countSql.Append(string.Format(" FROM `" + viewboxIndexDbName + "`.`VALUE_" + columnId + "` V "));
				selectSql.Append($"WHERE V.id in (SELECT C.VALUE_ID FROM (");
				countSql.Append($"WHERE V.id in (SELECT C.VALUE_ID FROM (");
				selectSql.Append($"SELECT I.VALUE_ID ");
				countSql.Append($"SELECT I.VALUE_ID ");
				if (string.IsNullOrEmpty(search))
				{
					selectSql.Append($", @rownum := @rownum + 1 ");
					countSql.Append($", @rownum := @rownum + 1 ");
				}
				selectSql.Append(string.Format("FROM `" + viewboxIndexDbName + "`.`INDEX_" + columnId + "` I "));
				countSql.Append(string.Format("FROM `" + viewboxIndexDbName + "`.`INDEX_" + columnId + "` I "));
				if (string.IsNullOrEmpty(search))
				{
					selectSql.Append($"CROSS JOIN (SELECT @rownum := 0) r WHERE  @rownum < {limit} and I.ORDER_AREAS_ID in");
					countSql.Append($"CROSS JOIN (SELECT @rownum := 0) r WHERE  @rownum < {limit} and I.ORDER_AREAS_ID in");
				}
				else
				{
					selectSql.Append($"WHERE I.ORDER_AREAS_ID in");
					countSql.Append($"WHERE I.ORDER_AREAS_ID in");
				}
				selectSql.Append(string.Format("(SELECT id FROM `" + viewboxDbName + "`.`ORDER_AREAS` WHERE table_id = " + tableId));
				countSql.Append(string.Format("(SELECT id FROM `" + viewboxDbName + "`.`ORDER_AREAS` WHERE table_id = " + tableId));
				if (optimization.FindValue(OptimizationType.IndexTable) != null && !indexBlocked)
				{
					selectSql.Append(" AND (index_value IS NULL OR index_value = '" + optimization.FindValue(OptimizationType.IndexTable).ToString() + "') ");
					countSql.Append(" AND (index_value IS NULL OR index_value = '" + optimization.FindValue(OptimizationType.IndexTable).ToString() + "') ");
				}
				else if (indexBlocked && allowedIndex != "")
				{
					selectSql.Append(" AND (split_value IS NULL OR split_value in (");
					selectSql.Append("SELECT value FROM `" + viewboxDbName + "`.`optimizations` opt WHERE optimization_group_id IN (SELECT id from `" + viewboxDbName + "`.optimization_groups WHERE type = 5) ");
					selectSql.Append("AND id IN (" + allowedIndex + ")))");
					countSql.Append(" AND (split_value IS NULL OR split_value in (");
					countSql.Append("SELECT value FROM `" + viewboxDbName + "`.`optimizations` opt WHERE optimization_group_id IN (SELECT id from `" + viewboxDbName + "`.optimization_groups WHERE type = 5) ");
					countSql.Append("AND id IN (" + allowedIndex + ")))");
				}
				if (optimization.FindValue(OptimizationType.SplitTable) != null && !splitBlocked)
				{
					selectSql.Append(" AND (split_value IS NULL OR split_value = '" + optimization.FindValue(OptimizationType.SplitTable).ToString() + "') ");
					countSql.Append(" AND (split_value IS NULL OR split_value = '" + optimization.FindValue(OptimizationType.SplitTable).ToString() + "') ");
				}
				else if (splitBlocked && allowedSplit != "")
				{
					selectSql.Append(" AND (split_value IS NULL OR split_value in (");
					selectSql.Append("SELECT value FROM `" + viewboxDbName + "`.`optimizations` opt WHERE optimization_group_id IN (SELECT id from `" + viewboxDbName + "`.optimization_groups WHERE type = 3) ");
					selectSql.Append("AND id IN (" + allowedSplit + ")))");
					countSql.Append(" AND (split_value IS NULL OR split_value in (");
					countSql.Append("SELECT value FROM `" + viewboxDbName + "`.`optimizations` opt WHERE optimization_group_id IN (SELECT id from `" + viewboxDbName + "`.optimization_groups WHERE type = 3) ");
					countSql.Append("AND id IN (" + allowedSplit + ")))");
				}
				if (optimization.FindValue(OptimizationType.SortColumn) != null && !sortBlocked)
				{
					selectSql.Append(" AND (sort_value IS NULL OR sort_value = '" + optimization.FindValue(OptimizationType.SortColumn).ToString() + "') ");
					countSql.Append(" AND (sort_value IS NULL OR sort_value = '" + optimization.FindValue(OptimizationType.SortColumn).ToString() + "') ");
				}
				else if (sortBlocked && allowedSort != "")
				{
					selectSql.Append(" AND (split_value IS NULL OR split_value in (");
					selectSql.Append("SELECT value FROM `" + viewboxDbName + "`.`optimizations` opt WHERE optimization_group_id IN (SELECT id from `" + viewboxDbName + "`.optimization_groups WHERE type = 4) ");
					selectSql.Append("AND id IN (" + allowedSort + ")))");
					countSql.Append(" AND (split_value IS NULL OR split_value in (");
					countSql.Append("SELECT value FROM `" + viewboxDbName + "`.`optimizations` opt WHERE optimization_group_id IN (SELECT id from `" + viewboxDbName + "`.optimization_groups WHERE type = 4) ");
					countSql.Append("AND id IN (" + allowedSort + ")))");
				}
				selectSql.Append($"))C)");
				countSql.Append($"))C)");
				if (!string.IsNullOrEmpty(search))
				{
					search = ((parameter.DataType == SqlType.Decimal) ? DecimalFormat(issue, parameterId, search) : search);
					if (hasDescription)
					{
						selectSql.Append("AND (");
						selectSql.Append("V.value" + (exactMatch ? " = " : " LIKE ") + (exactMatch ? ("'" + search + "'") : ("'%" + search + "%'")));
						selectSql.Append(" OR V.description" + (exactMatch ? " = " : " LIKE ") + (exactMatch ? ("'" + search + "'") : ("'%" + search + "%'")));
						selectSql.Append(")");
						countSql.Append("AND (");
						countSql.Append("V.value" + (exactMatch ? " = " : " LIKE ") + (exactMatch ? ("'" + search + "'") : ("'%" + search + "%'")));
						countSql.Append(" OR V.description" + (exactMatch ? " = " : " LIKE ") + (exactMatch ? ("'" + search + "'") : ("'%" + search + "%'")));
						countSql.Append(")");
					}
					else
					{
						selectSql.Append(string.Format(" AND {0} {1} {2} ", (!(languageCode == "de")) ? (exactMatch ? "V.value" : ((parameter.DataType == SqlType.Date) ? "CAST(DATE_FORMAT(V.value, '%Y-%m-%d') as CHAR(20))" : "V.value")) : (exactMatch ? "V.value" : ((parameter.DataType == SqlType.Date) ? "CAST(DATE_FORMAT(V.value, '%d.%m.%Y') as CHAR(20))" : "V.value")), exactMatch ? " = " : " LIKE ", exactMatch ? ("'" + search + "'") : ("'%" + search + "%'")));
						countSql.Append(string.Format(" AND {0} {1} {2} ", (!(languageCode == "de")) ? (exactMatch ? "V.value" : ((parameter.DataType == SqlType.Date) ? "CAST(DATE_FORMAT(V.value, '%Y-%m-%d') as CHAR(20))" : "V.value")) : (exactMatch ? "V.value" : ((parameter.DataType == SqlType.Date) ? "CAST(DATE_FORMAT(V.value, '%d.%m.%Y') as CHAR(20))" : "V.value")), exactMatch ? " = " : " LIKE ", exactMatch ? ("'" + search + "'") : ("'%" + search + "%'")));
					}
				}
				if (!string.IsNullOrEmpty(roleBasedFilter))
				{
					if (selectSql.ToString().Contains("WHERE"))
					{
						selectSql.Append(" AND ");
					}
					else
					{
						selectSql.Append(" WHERE ");
					}
					selectSql.Append(roleBasedFilter);
					if (countSql.ToString().Contains("WHERE"))
					{
						countSql.Append(" AND ");
					}
					else
					{
						countSql.Append(" WHERE ");
					}
					countSql.Append(roleBasedFilter);
				}
				if (parameter.OptimizationType == OptimizationType.SplitTable && allowedSplit != "")
				{
					if (selectSql.ToString().Contains("WHERE"))
					{
						selectSql.Append(" AND ");
					}
					else
					{
						selectSql.Append(" WHERE ");
					}
					selectSql.Append("V.value IN( SELECT value FROM `" + viewboxDbName + "`.`optimizations` WHERE id in(" + allowedSplit + "))");
					if (countSql.ToString().Contains("WHERE"))
					{
						countSql.Append(" AND ");
					}
					else
					{
						countSql.Append(" WHERE ");
					}
					countSql.Append("V.value IN( SELECT value FROM `" + viewboxDbName + "`.`optimizations` WHERE id in(" + allowedSplit + "))");
				}
				if (parameter.OptimizationType == OptimizationType.SortColumn && allowedSort != "")
				{
					if (selectSql.ToString().Contains("WHERE"))
					{
						selectSql.Append(" AND ");
					}
					else
					{
						selectSql.Append(" WHERE ");
					}
					selectSql.Append("V.value IN( SELECT value FROM `" + viewboxDbName + "`.`optimizations` WHERE id in(" + allowedSort + "))");
					if (countSql.ToString().Contains("WHERE"))
					{
						countSql.Append(" AND ");
					}
					else
					{
						countSql.Append(" WHERE ");
					}
					countSql.Append("V.value IN( SELECT value FROM `" + viewboxDbName + "`.`optimizations` WHERE id in(" + allowedSort + "))");
				}
				selectSql.Append(" GROUP BY v.value ");
				switch (direction)
				{
				case SortDirection.Ascending:
					selectSql.Append(" ORDER BY V.value ASC ");
					break;
				case SortDirection.Descending:
					selectSql.Append(" ORDER BY V.value DESC ");
					break;
				}
				selectSql.Append($" LIMIT {from}, {to};");
				LogHelper.GetLogger().Info("Smallbook - everything is ok: " + selectSql.ToString());
				LogHelper.GetLogger().Info("Smallbook - everything is ok: " + countSql.ToString());
				using (IDataReader reader3 = connection.ExecuteReader(selectSql.ToString()))
				{
					while (reader3.Read())
					{
						valueListCollection.Add(new ValueListElement
						{
							Id = reader3.GetInt32(0),
							Value = reader3.GetValue(1),
							Description = ((hasDescription && !reader3.IsDBNull(2)) ? reader3.GetString(2) : "")
						});
					}
				}
				valueCount = int.Parse(connection.ExecuteScalar(countSql.ToString()).ToString());
				return valueListCollection;
			}
			catch (Exception)
			{
				return valueListCollection;
			}
		}

		private static bool HasDescription(TempDb tempDb, string indexDbName, int columnId)
		{
			try
			{
				using DatabaseBase conn = tempDb.ConnectionManager.GetConnection();
				string query = string.Format("DESCRIBE {0}", conn.Enquote(indexDbName, "VALUE_" + columnId));
				using IDataReader reader = conn.ExecuteReader(query);
				while (reader.Read())
				{
					if (reader.GetString(0).Contains("description"))
					{
						return true;
					}
				}
			}
			catch (Exception)
			{
				return false;
			}
			return false;
		}

		private static void GetIdsForIndex(out int tableId, out int columnId, int parameterId, string viewboxDbName, TempDb tempDb)
		{
			columnId = -1;
			tableId = -1;
			StringBuilder selectSql = new StringBuilder();
			using DatabaseBase connection = tempDb.ConnectionManager.GetConnection();
			selectSql.Append("SELECT c.id, t.id FROM " + viewboxDbName + ".parameter p ");
			selectSql.Append("INNER JOIN " + viewboxDbName + ".tables t ON t.name = p.table_name AND t.`database` = p.database_name ");
			selectSql.Append("INNER JOIN " + viewboxDbName + ".columns c ON c.table_id = t.id AND c.name = p.column_name ");
			selectSql.Append("WHERE p.id =").Append(parameterId + " LIMIT 1;");
			using (IDataReader reader = connection.ExecuteReader(selectSql.ToString()))
			{
				while (reader.Read())
				{
					columnId = reader.GetInt32(0);
					tableId = reader.GetInt32(1);
				}
			}
			selectSql.Clear();
		}

		public static string DateFormat(IIssue issue, int parameterId, string input)
		{
			IParameter col = issue.Parameters.SingleOrDefault((IParameter i) => i.Id == parameterId);
			if (col != null && col.DataType == SqlType.Date)
			{
				if (DateTime.TryParse(input, out var itsaDate))
				{
					input = input.Replace("/", "-");
					input = input.Replace(".", "-");
					string[] temps = input.Split('-');
					int year = 0;
					int month = 0;
					int day = 0;
					for (int j = 0; j < temps.Count(); j++)
					{
						int number = Convert.ToInt32(temps[j]);
						if (itsaDate.Year == number)
						{
							year = number;
						}
						if (itsaDate.Month == number)
						{
							month = number;
						}
						if (itsaDate.Day == number)
						{
							day = number;
						}
					}
					input = string.Empty;
					if (year > 0)
					{
						input = input + year + "-";
					}
					if (month > 0 && month < 10)
					{
						input = input + "0" + month + "-";
					}
					else if (month > 0)
					{
						input = input + month + "-";
					}
					if (day > 0 && day < 10)
					{
						input = input + "0" + day;
					}
					else if (day > 0)
					{
						input += day;
					}
				}
				else
				{
					input = input.Replace("/", string.Empty);
					input = input.Replace(".", string.Empty);
					input = input.Replace("-", string.Empty);
				}
			}
			return input;
		}

		private static string DecimalFormat(IIssue issue, int parameterId, string search)
		{
			if (issue != null && issue.Parameters != null)
			{
				IParameter col = issue.Parameters.SingleOrDefault((IParameter i) => i.Id == parameterId);
				if (col != null && col.DataType == SqlType.Decimal)
				{
					search = search.Replace(",", ".");
				}
			}
			return search;
		}
	}
}
