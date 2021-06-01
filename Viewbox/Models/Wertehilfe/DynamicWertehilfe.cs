using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using SystemDb;
using DbAccess;
using ViewboxDb;

namespace Viewbox.Models.Wertehilfe
{
	public class DynamicWertehilfe : Wertehilfe
	{
		protected string[] selectedValues;

		public DynamicWertehilfe(int parameterId, string search = "", bool isExactmatch = false, string[] sortColumns = null, string[] directions = null, string[] selectedValues = null, bool onlyCheck = false)
			: base(parameterId, search, isExactmatch, sortColumns, directions, onlyCheck)
		{
			this.selectedValues = selectedValues;
		}

		public override void BuildValuesCollection()
		{
			base.BuildValuesCollection();
		}

		protected override void BuildValuesNoIndexNoDescription()
		{
			StringBuilder query = new StringBuilder();
			StringBuilder count = new StringBuilder();
			List<Task> tasks = new List<Task>();
			query.Append($"SELECT DISTINCT {GetValueLogic()} FROM {viewboxIndexName}.VALUE_{columnId} v ");
			count.Append($"SELECT COUNT(*) FROM (SELECT DISTINCT v.value FROM {viewboxIndexName}.VALUE_{columnId} v ");
			query.Append($"WHERE (v.value in ({ValidValueQueries()})) ");
			count.Append($"WHERE (v.value in ({ValidValueQueries()})) ");
			if (searchText != string.Empty)
			{
				query.Append(string.Format("AND {0} ", ParseLogicBasedOnType("value")));
				count.Append(string.Format("AND {0} ", ParseLogicBasedOnType("value")));
			}
			query.Append($"LIMIT {start}, {rowsPerPage};");
			count.Append($" LIMIT {maxRowCount}) t;");
			tasks.Add(Task.Factory.StartNew(delegate
			{
				using DatabaseBase databaseBase2 = ViewboxApplication.Database.ConnectionManager.GetConnection();
				using IDataReader dataReader = databaseBase2.ExecuteReader(query.ToString());
				while (dataReader.Read() && valueListCollection.Count < rowsPerPage)
				{
					valueListCollection.Add(new ValueListElement
					{
						Id = 0,
						Value = dataReader.GetValue(1),
						Description = string.Empty
					});
				}
			}));
			tasks.Add(Task.Factory.StartNew(delegate
			{
				using DatabaseBase databaseBase = ViewboxApplication.Database.ConnectionManager.GetConnection();
				rowCount = int.Parse(databaseBase.ExecuteScalar(count.ToString()).ToString());
			}));
			Task.WaitAll(tasks.ToArray());
		}

		protected override void BuildValuesNoIndexWithDescription()
		{
			StringBuilder query = new StringBuilder();
			StringBuilder count = new StringBuilder();
			List<Task> tasks = new List<Task>();
			query.Append($"SELECT DISTINCT {GetValueLogic()}, v.description FROM {viewboxIndexName}.VALUE_{columnId} v ");
			count.Append($"SELECT COUNT(*) FROM (SELECT DISTINCT v.value, v.description) FROM {viewboxIndexName}.VALUE_{columnId} v ");
			query.Append($"WHERE (v.value in ({ValidValueQueries()})) ");
			count.Append($"WHERE (v.value in ({ValidValueQueries()})) ");
			if (searchText != string.Empty)
			{
				query.Append(string.Format("AND ({0} OR {1}) ", ParseLogicBasedOnType("value"), ParseLogicBasedOnType("description")));
				count.Append(string.Format("AND ({0} OR {1}) ", ParseLogicBasedOnType("value"), ParseLogicBasedOnType("description")));
			}
			query.Append($"LIMIT {start}, {rowsPerPage};");
			count.Append($" LIMIT {maxRowCount}) t;");
			tasks.Add(Task.Factory.StartNew(delegate
			{
				using DatabaseBase databaseBase2 = ViewboxApplication.Database.ConnectionManager.GetConnection();
				using IDataReader dataReader = databaseBase2.ExecuteReader(query.ToString());
				while (dataReader.Read() && valueListCollection.Count < rowsPerPage)
				{
					valueListCollection.Add(new ValueListElement
					{
						Id = 0,
						Value = dataReader.GetValue(1),
						Description = ((!dataReader.IsDBNull(2)) ? dataReader.GetString(2) : "")
					});
				}
			}));
			tasks.Add(Task.Factory.StartNew(delegate
			{
				using DatabaseBase databaseBase = ViewboxApplication.Database.ConnectionManager.GetConnection();
				rowCount = int.Parse(databaseBase.ExecuteScalar(count.ToString()).ToString());
			}));
			Task.WaitAll(tasks.ToArray());
		}

		protected override void BuildValuesWithIndexNoDescription()
		{
			StringBuilder query = new StringBuilder();
			StringBuilder count = new StringBuilder();
			List<Task> tasks = new List<Task>();
			IOptimization optimization = ViewboxSession.Optimizations.LastOrDefault();
			query.Append($"SELECT {GetValueLogic()} FROM {viewboxIndexName}.VALUE_{column.Id} v ");
			count.Append($"SELECT COUNT(*) FROM (SELECT v.value FROM {viewboxIndexName}.VALUE_{column.Id} v ");
			query.Append(GetJoin());
			count.Append(GetJoin());
			string validateQuery = ValidValueQueries();
			if (validateQuery != string.Empty)
			{
				query.Append($"AND (value in ({validateQuery})) ");
				count.Append($"AND (value in ({validateQuery})) ");
			}
			if (searchText != string.Empty)
			{
				query.Append(string.Format(" AND {0} ", ParseLogicBasedOnType("value")));
				count.Append(string.Format(" AND {0} ", ParseLogicBasedOnType("value")));
			}
			query.Append("GROUP BY value ");
			query.Append($"LIMIT {start}, {rowsPerPage};");
			count.Append($" LIMIT {maxRowCount}) t;");
			tasks.Add(Task.Factory.StartNew(delegate
			{
				using DatabaseBase databaseBase2 = ViewboxApplication.Database.ConnectionManager.GetConnection();
				using IDataReader dataReader = databaseBase2.ExecuteReader(query.ToString());
				while (dataReader.Read() && valueListCollection.Count < rowsPerPage)
				{
					valueListCollection.Add(new ValueListElement
					{
						Id = 0,
						Value = dataReader.GetValue(1),
						Description = string.Empty
					});
				}
			}));
			tasks.Add(Task.Factory.StartNew(delegate
			{
				using DatabaseBase databaseBase = ViewboxApplication.Database.ConnectionManager.GetConnection();
				rowCount = int.Parse(databaseBase.ExecuteScalar(count.ToString()).ToString());
			}));
			Task.WaitAll(tasks.ToArray());
		}

		protected override void BuildValuesWithIndexWithDescription()
		{
			StringBuilder query = new StringBuilder();
			StringBuilder count = new StringBuilder();
			List<Task> tasks = new List<Task>();
			IOptimization optimization = ViewboxSession.Optimizations.LastOrDefault();
			query.Append($"SELECT {GetValueLogic()}, v.description FROM {viewboxIndexName}.VALUE_{columnId} v ");
			count.Append($"SELECT COUNT(*) FROM (SELECT DISTINCT v.value, v.description FROM {viewboxIndexName}.VALUE_{columnId} v ");
			query.Append(GetJoin());
			count.Append(GetJoin());
			string validateQuery = ValidValueQueries();
			if (validateQuery != string.Empty)
			{
				query.Append($"AND (value in ({validateQuery})) ");
				count.Append($"AND (value in ({validateQuery})) ");
			}
			if (searchText != string.Empty)
			{
				query.Append(string.Format(" AND ({0} OR {1}) ", ParseLogicBasedOnType("value"), ParseLogicBasedOnType("description")));
				count.Append(string.Format(" AND ({0} OR {1}) ", ParseLogicBasedOnType("value"), ParseLogicBasedOnType("description")));
			}
			query.Append($"LIMIT {start}, {rowsPerPage};");
			count.Append($" LIMIT {maxRowCount}) t;");
			tasks.Add(Task.Factory.StartNew(delegate
			{
				using DatabaseBase databaseBase2 = ViewboxApplication.Database.ConnectionManager.GetConnection();
				using IDataReader dataReader = databaseBase2.ExecuteReader(query.ToString());
				while (dataReader.Read() && valueListCollection.Count < rowsPerPage)
				{
					valueListCollection.Add(new ValueListElement
					{
						Id = 0,
						Value = dataReader.GetValue(1),
						Description = ((!dataReader.IsDBNull(2)) ? dataReader.GetString(2) : "")
					});
				}
			}));
			tasks.Add(Task.Factory.StartNew(delegate
			{
				using DatabaseBase databaseBase = ViewboxApplication.Database.ConnectionManager.GetConnection();
				rowCount = int.Parse(databaseBase.ExecuteScalar(count.ToString()).ToString());
			}));
			Task.WaitAll(tasks.ToArray());
		}

		protected string ValidValueQueries()
		{
			IIssue issue = parameter.Issue;
			StringBuilder validValuesQuery = new StringBuilder();
			if (selectedValues != null)
			{
				string[] array = selectedValues;
				foreach (string param in array)
				{
					string[] keyValue = param.Split(';');
					string value = keyValue[1];
					string colName = issue.Parameters.Single((IParameter p) => p.Id.ToString() == keyValue[0]).ColumnName;
					SqlType columnType = issue.Parameters.Single((IParameter p) => p.Id.ToString() == keyValue[0]).DataType;
					int num;
					switch (columnType)
					{
					case SqlType.Date:
					{
						string dateFormat = Thread.CurrentThread.CurrentCulture.DateTimeFormat.ShortDatePattern;
						string mySqlDateFormat = dateFormat.Replace("dd", "%d").Replace("MM", "%m").Replace("yyyy", "%Y");
						colName = "DATE_FORMAT(`" + colName + "`, '" + mySqlDateFormat + "')";
						validValuesQuery.Append($"{colName} = '{value}' AND ");
						continue;
					}
					case SqlType.Integer:
						validValuesQuery.Append(string.Format("`{0}` = {1} AND ", colName, value.Replace(",", "").Replace(".", "")));
						continue;
					default:
						num = ((columnType == SqlType.Numeric) ? 1 : 0);
						break;
					case SqlType.Decimal:
						num = 1;
						break;
					}
					if (num != 0)
					{
						if (ViewboxSession.Language.CountryCode == "de")
						{
							validValuesQuery.Append(string.Format("`{0}` = {1} AND ", colName, value.Replace(".", "").Replace(",", ".")));
						}
						else
						{
							validValuesQuery.Append(string.Format("`{0}` = {1} AND ", colName, value.Replace(",", "")));
						}
					}
					else
					{
						validValuesQuery.Append($"`{colName}` = '{value}' AND ");
					}
				}
				return $"SELECT {column.Name} FROM `{parameter.DatabaseName}`.`{parameter.TableName}` WHERE ({validValuesQuery} true)";
			}
			return string.Empty;
		}
	}
}
