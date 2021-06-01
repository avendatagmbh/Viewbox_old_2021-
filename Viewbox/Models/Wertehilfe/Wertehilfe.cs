using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SystemDb;
using SystemDb.Internal;
using DbAccess;
using ViewboxDb;

namespace Viewbox.Models.Wertehilfe
{
	public class Wertehilfe : IWertehilfe
	{
		protected int rowsPerPage;

		protected int maxRowCount;

		protected int start;

		protected IParameter parameter;

		protected IColumn column;

		protected bool isExactmatch;

		protected bool hasDescription;

		protected bool hasIndex;

		protected string searchText;

		protected WertehilfeSorter sorter;

		protected ValueListCollection valueListCollection = new ValueListCollection();

		protected int columnId;

		protected int rowCount = 0;

		protected string viewboxIndexName = ViewboxApplication.Database.SystemDb.ConnectionManager.DbConfig.DbName + ViewboxApplication.IndexDatabasePostFix;

		protected string viewboxDbName = ViewboxApplication.Database.SystemDb.ConnectionManager.DbConfig.DbName;

		protected bool onlyCheck = false;

		protected IOptimization opt;

		protected string language = "de";

		protected bool hasLanguage = false;

		protected bool hasMandt = false;

		protected bool hasBukrs = false;

		protected bool hasGjahr = false;

		public int RowsPerPage
		{
			get
			{
				return rowsPerPage;
			}
			set
			{
				rowsPerPage = value;
			}
		}

		public int MaxRowCount
		{
			get
			{
				return maxRowCount;
			}
			set
			{
				maxRowCount = value;
			}
		}

		public int Start
		{
			get
			{
				return start;
			}
			set
			{
				start = value;
			}
		}

		public IParameter Parameter
		{
			get
			{
				return parameter;
			}
			set
			{
				parameter = value;
			}
		}

		public IColumn Column
		{
			get
			{
				return column;
			}
			set
			{
				column = value;
			}
		}

		public IOptimization Optimization
		{
			get
			{
				return opt;
			}
			set
			{
				opt = value;
			}
		}

		public string Language
		{
			get
			{
				return language;
			}
			set
			{
				language = value;
			}
		}

		public bool IsExactmatch
		{
			get
			{
				return isExactmatch;
			}
			set
			{
				isExactmatch = value;
			}
		}

		public bool HasDescription
		{
			get
			{
				return hasDescription;
			}
			internal set
			{
				hasDescription = value;
			}
		}

		public bool HasIndex
		{
			get
			{
				return hasIndex;
			}
			internal set
			{
				hasIndex = value;
			}
		}

		public string SearchText
		{
			get
			{
				return searchText;
			}
			set
			{
				searchText = value;
			}
		}

		public WertehilfeSorter Sorter
		{
			get
			{
				return sorter;
			}
			internal set
			{
				sorter = value;
			}
		}

		public ValueListCollection ValueListCollection
		{
			get
			{
				return valueListCollection;
			}
			set
			{
				valueListCollection = value;
			}
		}

		public int FullWidth { get; internal set; }

		public int MaxValueWidth { get; internal set; }

		public string StyleFullWidth { get; internal set; }

		public int RowCount => rowCount;

		public int Page
		{
			get
			{
				int page = 0;
				if (start % rowsPerPage == 0)
				{
					return start / rowsPerPage;
				}
				return start / rowsPerPage + 1;
			}
		}

		public int MaxPage
		{
			get
			{
				if (MaxRowCountToDisplay % rowsPerPage == 0)
				{
					return MaxRowCountToDisplay / rowsPerPage;
				}
				return MaxRowCountToDisplay / rowsPerPage + 1;
			}
		}

		public int MaxRowCountToDisplay => (rowCount >= maxRowCount) ? maxRowCount : rowCount;

		public Wertehilfe(int parameterId, string search = "", bool isExactmatch = false, string[] sortColumns = null, string[] directions = null, bool onlyCheck = false)
		{
			Wertehilfe wertehilfe = this;
			this.onlyCheck = onlyCheck;
			searchText = search;
			this.isExactmatch = isExactmatch;
			sorter = new WertehilfeSorter(sortColumns, directions);
			parameter = ViewboxApplication.Database.SystemDb.Parameters.FirstOrDefault((KeyValuePair<int, IParameter> p) => p.Key == parameterId).Value;
			if (parameter == null)
			{
				IEnumerable<IParameter> parameters = ViewboxSession.Issues.SelectMany((IIssue i) => i.Parameters);
				parameter = parameters.FirstOrDefault((IParameter p) => p.Id == parameterId);
			}
			columnId = GetColumnId();
			column = ((columnId == -1) ? null : ViewboxApplication.Database.SystemDb.Columns.FirstOrDefault((IColumn c) => c.Id == wertehilfe.columnId));
			hasDescription = CheckDescription();
			hasIndex = HasIndexTable();
		}

		protected string ParseValueBasedOnType()
		{
			string languageKey = language;
			switch (parameter.DataType)
			{
			case SqlType.String:
			case SqlType.Binary:
				if (isExactmatch)
				{
					return $"'{searchText}'";
				}
				return $"'%{searchText}%'";
			case SqlType.Integer:
				if (isExactmatch)
				{
					return string.Format("{0}", searchText.Replace(",", "").Replace(".", ""));
				}
				return $"'%{searchText}%'";
			case SqlType.Decimal:
			case SqlType.Numeric:
				if (isExactmatch)
				{
					if (languageKey == "D")
					{
						return string.Format("{0}", searchText.Replace(".", "").Replace(",", "."));
					}
					return string.Format("{0}", searchText.Replace(",", ""));
				}
				if (languageKey == "D")
				{
					return string.Format("'%{0}%'", searchText.Replace(".", "").Replace(",", "."));
				}
				return string.Format("'%{0}%'", searchText.Replace(",", ""));
			case SqlType.Date:
			case SqlType.Time:
			case SqlType.DateTime:
				if (isExactmatch)
				{
					return $"'{searchText}'";
				}
				return $"'%{searchText}%'";
			default:
				if (isExactmatch)
				{
					return $"'{searchText}'";
				}
				return $"'%{searchText}%'";
			}
		}

		protected string ParseLogicBasedOnType(string columnName)
		{
			string languageKey = language;
			switch (parameter.DataType)
			{
			case SqlType.String:
			case SqlType.Integer:
			case SqlType.Decimal:
			case SqlType.Numeric:
			case SqlType.Binary:
			case SqlType.Time:
				if (isExactmatch)
				{
					return $"(v.`{columnName}` = {ParseValueBasedOnType()})";
				}
				return $"(v.`{columnName}` LIKE {ParseValueBasedOnType()})";
			case SqlType.DateTime:
				if (isExactmatch)
				{
					if (languageKey == "D")
					{
						return $"DATE_FORMAT(v.`{columnName}`, '%d.%m.%Y %H:%i:%S') = {ParseValueBasedOnType()}";
					}
					if (languageKey == "E")
					{
						return $"DATE_FORMAT(v.`{columnName}`, '%d/%m/%Y %H:%i:%S') = {ParseValueBasedOnType()}";
					}
					return $"(v.`{columnName}` = {ParseValueBasedOnType()})";
				}
				if (languageKey == "D")
				{
					return $"(CAST(DATE_FORMAT(v.`{columnName}`, '%d.%m.%Y %H:%i:%S') as CHAR(20)) LIKE {ParseValueBasedOnType()})";
				}
				if (languageKey == "E")
				{
					return $"(CAST(DATE_FORMAT(v.`{columnName}`, '%d/%m/%Y %H:%i:%S') as CHAR(20)) LIKE {ParseValueBasedOnType()})";
				}
				return $"(CAST(v.`{columnName}` as CHAR(20)) LIKE {ParseValueBasedOnType()})";
			case SqlType.Date:
				if (isExactmatch)
				{
					if (languageKey == "D")
					{
						return $"DATE_FORMAT(v.`{columnName}`, '%d.%m.%Y') = {ParseValueBasedOnType()}";
					}
					if (languageKey == "E")
					{
						return $"DATE_FORMAT(v.`{columnName}`, '%d/%m/%Y') = {ParseValueBasedOnType()}";
					}
					return $"(v.`{columnName}` = {ParseValueBasedOnType()})";
				}
				if (languageKey == "D")
				{
					return $"(CAST(DATE_FORMAT(v.`{columnName}`, '%d.%m.%Y') as CHAR(20)) LIKE {ParseValueBasedOnType()})";
				}
				if (languageKey == "E")
				{
					return $"(CAST(DATE_FORMAT(v.`{columnName}`, '%d/%m/%Y') as CHAR(20)) LIKE {ParseValueBasedOnType()})";
				}
				return $"(CAST(v.`{columnName}` as CHAR(20)) LIKE {ParseValueBasedOnType()})";
			default:
				if (isExactmatch)
				{
					return string.Format("(v.`{0}` = {1})", ParseValueBasedOnType());
				}
				return $"(v.`{columnName}` LIKE {ParseValueBasedOnType()})";
			}
		}

		protected int GetColumnId()
		{
			StringBuilder selectSql = new StringBuilder();
			using DatabaseBase connection = ViewboxApplication.Database.ConnectionManager.GetConnection();
			selectSql.Append($"SELECT DISTINCT c.id FROM {viewboxDbName}.parameter p ");
			selectSql.Append($"JOIN {viewboxDbName}.tables t ON t.name = p.table_name AND t.`database` = p.`database_name` ");
			selectSql.Append($"JOIN {viewboxDbName}.columns c ON c.table_id = t.id AND c.name = p.column_name ");
			selectSql.Append($"WHERE p.id ={parameter.Id}");
			object result = connection.ExecuteScalar(selectSql.ToString());
			if (result != null && int.TryParse(result.ToString(), out var temp))
			{
				return temp;
			}
			return -1;
		}

		protected bool HasIndexTable()
		{
			string query = $"SELECT COUNT(*) FROM {viewboxIndexName}.INDEX_{columnId};";
			using DatabaseBase conn = ViewboxApplication.Database.ConnectionManager.GetConnection();
			try
			{
				long indexCount = long.Parse(conn.ExecuteScalar(query).ToString());
				return indexCount > 0;
			}
			catch (Exception)
			{
				return false;
			}
		}

		protected bool CheckDescription()
		{
			try
			{
				using DatabaseBase conn = ViewboxApplication.Database.ConnectionManager.GetConnection();
				string query = string.Format("DESCRIBE {0}", conn.Enquote(viewboxIndexName, "VALUE_" + columnId));
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

		protected string GetValueLogic()
		{
			return (column.DataType == SqlType.Binary) ? $"CAST(v.`value` AS CHAR(255))" : "v.value";
		}

		protected string GetCalculationLimit()
		{
			return string.Format("(SELECT COUNT(*) FROM {0}.VALUE_{1} V JOIN {0}.INDEX_{1} I ON V.id = I.value_id)", viewboxIndexName, columnId);
		}

		protected string GetJoin()
		{
			StringBuilder joinQuery = new StringBuilder();
			joinQuery.Append($"JOIN `{viewboxIndexName}`.`INDEX_{columnId}` i ON i.value_id = v.id ");
			joinQuery.Append(string.Format("JOIN `{0}`.`order_areas` o ON o.id = i.order_areas_id ", viewboxDbName, columnId));
			joinQuery.Append(string.Format("WHERE table_id = {0} AND ({1}) AND ({2}) AND ({3}) AND ({4}) ", column.Table.Id, (opt.FindValue(OptimizationType.IndexTable) != null && hasMandt) ? $"index_value = '{opt.FindValue(OptimizationType.IndexTable)}'" : "TRUE", (opt.FindValue(OptimizationType.SortColumn) != null && hasGjahr) ? $"sort_value = '{opt.FindValue(OptimizationType.SortColumn)}'" : "TRUE", (opt.FindValue(OptimizationType.SplitTable) != null && hasBukrs) ? $"split_value = '{opt.FindValue(OptimizationType.SplitTable)}'" : "TRUE", (language != null && hasLanguage) ? $"language_value = '{language}'" : "TRUE"));
			return joinQuery.ToString();
		}

		public virtual void BuildValuesCollection()
		{
			if (column != null)
			{
				int orderAreasCount = column.Table.OrderAreas.Count;
				int countNullValues = column.Table.OrderAreas.Count((IOrderArea o) => o.SplitValue == null);
				hasBukrs = orderAreasCount != 0 && countNullValues != orderAreasCount;
				countNullValues = column.Table.OrderAreas.Count((IOrderArea o) => o.SortValue == null);
				hasGjahr = orderAreasCount != 0 && countNullValues != orderAreasCount;
				countNullValues = column.Table.OrderAreas.Count((IOrderArea o) => o.IndexValue == null);
				hasMandt = orderAreasCount != 0 && countNullValues != orderAreasCount;
				string languageValue = LanguageKeyTransformer.Transformer(language);
				countNullValues = column.Table.OrderAreas.Count((IOrderArea o) => o.LanguageValue == null);
				hasLanguage = orderAreasCount != 0 && countNullValues != orderAreasCount;
				if (hasIndex)
				{
					if (hasDescription)
					{
						BuildValuesWithIndexWithDescription();
					}
					else
					{
						BuildValuesWithIndexNoDescription();
					}
				}
				else if (hasDescription)
				{
					BuildValuesNoIndexWithDescription();
				}
				else
				{
					BuildValuesNoIndexNoDescription();
				}
			}
			if (ValueListCollection.Count > 0 && !onlyCheck)
			{
				int valueMaxLength = ValueListCollection.Max((ValueListElement v) => v.Value.ToString().Length);
				int descriptionMaxLength = ValueListCollection.Max((ValueListElement v) => v.Description.ToString().Length);
				FullWidth = (valueMaxLength + descriptionMaxLength) * 8 + 25;
				MaxValueWidth = valueMaxLength * 8 + 25;
				StyleFullWidth = $"width: {((FullWidth + MaxValueWidth > 500) ? (FullWidth + MaxValueWidth) : 500)}px;";
			}
		}

		protected virtual void BuildValuesNoIndexNoDescription()
		{
			StringBuilder query = new StringBuilder();
			StringBuilder count = new StringBuilder();
			List<Task> tasks = new List<Task>();
			query.Append($"SELECT DISTINCT {GetValueLogic()} FROM {viewboxIndexName}.VALUE_{columnId} v ");
			count.Append($"SELECT COUNT(*) FROM (SELECT DISTINCT value FROM {viewboxIndexName}.VALUE_{columnId} v ");
			if (searchText != string.Empty)
			{
				query.Append(string.Format("WHERE {0} ", ParseLogicBasedOnType("value")));
				count.Append(string.Format("WHERE {0} ", ParseLogicBasedOnType("value")));
			}
			if (onlyCheck)
			{
				query.Append("LIMIT 1;");
			}
			else
			{
				if (!ViewboxApplication.HideWertehilfeSorting)
				{
					query.Append($"{sorter.GetSortingLogic(GetValueLogic())} ");
				}
				query.Append($"LIMIT {start}, {rowsPerPage};");
				count.Append($" LIMIT {maxRowCount}) t;");
			}
			tasks.Add(Task.Factory.StartNew(delegate
			{
				try
				{
					using DatabaseBase databaseBase2 = ViewboxApplication.Database.ConnectionManager.GetConnection();
					using IDataReader dataReader = databaseBase2.ExecuteReader(query.ToString());
					while (dataReader.Read() && valueListCollection.Count < rowsPerPage)
					{
						valueListCollection.Add(new ValueListElement
						{
							Id = 0,
							Value = dataReader.GetValue(0),
							Description = string.Empty
						});
					}
				}
				catch (Exception)
				{
				}
			}));
			if (!onlyCheck)
			{
				tasks.Add(Task.Factory.StartNew(delegate
				{
					using DatabaseBase databaseBase = ViewboxApplication.Database.ConnectionManager.GetConnection();
					rowCount = int.Parse(databaseBase.ExecuteScalar(count.ToString()).ToString());
				}));
			}
			Task.WaitAll(tasks.ToArray());
		}

		protected virtual void BuildValuesNoIndexWithDescription()
		{
			StringBuilder query = new StringBuilder();
			StringBuilder count = new StringBuilder();
			List<Task> tasks = new List<Task>();
			query.Append($"SELECT DISTINCT {GetValueLogic()}, v.description FROM {viewboxIndexName}.VALUE_{columnId} v ");
			count.Append($"SELECT COUNT(*) FROM (SELECT DISTINCT v.value, v.description FROM {viewboxIndexName}.VALUE_{columnId} v ");
			if (searchText != string.Empty)
			{
				query.Append(string.Format("WHERE ({0} OR {1}) ", ParseLogicBasedOnType("value"), ParseLogicBasedOnType("description")));
				count.Append(string.Format("WHERE ({0} OR {1}) ", ParseLogicBasedOnType("value"), ParseLogicBasedOnType("description")));
			}
			if (onlyCheck)
			{
				query.Append("LIMIT 1;");
			}
			else
			{
				if (!ViewboxApplication.HideWertehilfeSorting)
				{
					query.Append($"{sorter.GetSortingLogic(GetValueLogic())} ");
				}
				query.Append($"LIMIT {start}, {rowsPerPage};");
				count.Append($" LIMIT {maxRowCount}) t;");
			}
			tasks.Add(Task.Factory.StartNew(delegate
			{
				try
				{
					using DatabaseBase databaseBase2 = ViewboxApplication.Database.ConnectionManager.GetConnection();
					using IDataReader dataReader = databaseBase2.ExecuteReader(query.ToString());
					while (dataReader.Read() && valueListCollection.Count < rowsPerPage)
					{
						valueListCollection.Add(new ValueListElement
						{
							Id = 0,
							Value = dataReader.GetValue(0),
							Description = ((!dataReader.IsDBNull(1)) ? dataReader.GetString(1) : "")
						});
					}
				}
				catch (Exception)
				{
				}
			}));
			if (!onlyCheck)
			{
				tasks.Add(Task.Factory.StartNew(delegate
				{
					using DatabaseBase databaseBase = ViewboxApplication.Database.ConnectionManager.GetConnection();
					rowCount = int.Parse(databaseBase.ExecuteScalar(count.ToString()).ToString());
				}));
			}
			Task.WaitAll(tasks.ToArray());
		}

		protected virtual void BuildValuesWithIndexNoDescription()
		{
			StringBuilder query = new StringBuilder();
			StringBuilder count = new StringBuilder();
			List<Task> tasks = new List<Task>();
			IOptimization optimization = opt;
			query.Append($"SELECT DISTINCT * FROM (SELECT {GetValueLogic()} FROM {viewboxIndexName}.VALUE_{column.Id} v ");
			count.Append($"SELECT COUNT(*) FROM (SELECT DISTINCT v.value FROM {viewboxIndexName}.VALUE_{column.Id} v ");
			query.Append(GetJoin());
			count.Append(GetJoin());
			if (searchText != string.Empty)
			{
				query.Append(string.Format(" AND {0} ", ParseLogicBasedOnType("value")));
				count.Append(string.Format(" AND {0} ", ParseLogicBasedOnType("value")));
			}
			if (onlyCheck)
			{
				query.Append("LIMIT 1) t;");
			}
			else
			{
				if (!ViewboxApplication.HideWertehilfeSorting)
				{
					query.Append($"{sorter.GetSortingLogic(GetValueLogic())} ");
				}
				query.Append($"LIMIT {start}, {rowsPerPage}) t;");
				count.Append($" LIMIT {maxRowCount}) t;");
			}
			tasks.Add(Task.Factory.StartNew(delegate
			{
				try
				{
					using DatabaseBase databaseBase2 = ViewboxApplication.Database.ConnectionManager.GetConnection();
					using IDataReader dataReader = databaseBase2.ExecuteReader(query.ToString());
					while (dataReader.Read() && valueListCollection.Count < rowsPerPage)
					{
						valueListCollection.Add(new ValueListElement
						{
							Id = 0,
							Value = dataReader.GetValue(0),
							Description = string.Empty
						});
					}
				}
				catch (Exception)
				{
				}
			}));
			if (!onlyCheck)
			{
				tasks.Add(Task.Factory.StartNew(delegate
				{
					using DatabaseBase databaseBase = ViewboxApplication.Database.ConnectionManager.GetConnection();
					rowCount = int.Parse(databaseBase.ExecuteScalar(count.ToString()).ToString());
				}));
			}
			Task.WaitAll(tasks.ToArray());
		}

		protected virtual void BuildValuesWithIndexWithDescription()
		{
			StringBuilder query = new StringBuilder();
			StringBuilder count = new StringBuilder();
			List<Task> tasks = new List<Task>();
			IOptimization optimization = opt;
			IOrderAreaCollection orderareas = Column.Table.OrderAreas;
			query.Append($"SELECT DISTINCT * FROM (SELECT {GetValueLogic()}, v.description FROM {viewboxIndexName}.VALUE_{columnId} v ");
			count.Append($"SELECT COUNT(*) FROM (SELECT DISTINCT v.value, v.description FROM {viewboxIndexName}.VALUE_{columnId} v ");
			query.Append(GetJoin());
			count.Append(GetJoin());
			if (searchText != string.Empty)
			{
				query.Append(string.Format(" AND ({0} OR {1}) ", ParseLogicBasedOnType("value"), ParseLogicBasedOnType("description")));
				count.Append(string.Format(" AND ({0} OR {1}) ", ParseLogicBasedOnType("value"), ParseLogicBasedOnType("description")));
			}
			if (onlyCheck)
			{
				query.Append("LIMIT 1) t;");
			}
			else
			{
				if (!ViewboxApplication.HideWertehilfeSorting)
				{
					query.Append($"{sorter.GetSortingLogic(GetValueLogic())} ");
				}
				query.Append($"LIMIT {start}, {rowsPerPage}) t;");
				count.Append($" LIMIT {maxRowCount}) t;");
			}
			tasks.Add(Task.Factory.StartNew(delegate
			{
				try
				{
					using DatabaseBase databaseBase2 = ViewboxApplication.Database.ConnectionManager.GetConnection();
					using IDataReader dataReader = databaseBase2.ExecuteReader(query.ToString());
					while (dataReader.Read() && valueListCollection.Count < rowsPerPage)
					{
						valueListCollection.Add(new ValueListElement
						{
							Id = 0,
							Value = dataReader.GetValue(0),
							Description = ((!dataReader.IsDBNull(1)) ? dataReader.GetString(1) : "")
						});
					}
				}
				catch (Exception)
				{
				}
			}));
			if (!onlyCheck)
			{
				tasks.Add(Task.Factory.StartNew(delegate
				{
					using DatabaseBase databaseBase = ViewboxApplication.Database.ConnectionManager.GetConnection();
					rowCount = int.Parse(databaseBase.ExecuteScalar(count.ToString()).ToString());
				}));
			}
			Task.WaitAll(tasks.ToArray());
		}
	}
}
