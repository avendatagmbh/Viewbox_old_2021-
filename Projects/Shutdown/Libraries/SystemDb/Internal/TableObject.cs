using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Text;
using DbAccess.Attributes;
using DbAccess.Enums;

namespace SystemDb.Internal
{
	[DbTable("tables", ForceInnoDb = true)]
	public abstract class TableObject : DataObject, ITableObject, IDataObject, INotifyPropertyChanged, ICloneable
	{
		public class ColumnConnection : IColumnConnection
		{
			public IColumn Source { get; set; }

			public IDataObject Target { get; set; }

			public int Operator { get; set; }

			public int FullLine { get; set; }

			public RelationType RelationType { get; set; }

			public string ExtInfo { get; set; }

			public string ColumnExtInfo { get; set; }

			public int RelationId { get; set; }

			public bool UserDefined { get; set; }
		}

		public class Relation : List<IColumnConnection>, IRelation, IEnumerable<IColumnConnection>, IEnumerable
		{
			public int RelationId { get; set; }

			public new IEnumerator<IColumnConnection> GetEnumerator()
			{
				return base.GetEnumerator();
			}
		}

		public class RelationCollection : HashSet<IRelation>, IRelationCollection, IEnumerable<IRelation>, IEnumerable
		{
			public IEnumerable<IRelation> this[int colid] => this.Where((IRelation r) => (r as Relation).Any((IColumnConnection cc) => cc.Source.Id == colid));

			public IEnumerable<IRelation> this[IColumn column]
			{
				get
				{
					if (column.Id >= 0)
					{
						return this[column.Id];
					}
					return this[column.Name, column.Table.Name, column.Table.Database];
				}
			}

			public IEnumerable<IRelation> this[string colName, string tableName, string databaseName] => this.Where((IRelation r) => (r as Relation).Any((IColumnConnection cc) => cc.Source.Name == colName && cc.Source.Table.Name == tableName && cc.Source.Table.Database == databaseName));

			public new IEnumerator<IRelation> GetEnumerator()
			{
				return base.GetEnumerator();
			}
		}

		private class ExtendedColumnInformationCollections : Dictionary<string, IList<IColumnConnection>>, IExtendedColumnInformationCollection, IDictionary<string, IList<IColumnConnection>>, ICollection<KeyValuePair<string, IList<IColumnConnection>>>, IEnumerable<KeyValuePair<string, IList<IColumnConnection>>>, IEnumerable
		{
		}

		private Category _category;

		private readonly ColumnCollection _columns = new ColumnCollection();

		private RelationCollection _relations = new RelationCollection();

		private string _database;

		private string _tableName;

		private bool _isArchiveChecked;

		private IndexCollection _indexes = new IndexCollection();

		private IDictionary<int, List<Tuple<int, List<string>>>> _roleBasedFilters;

		private readonly ExtendedColumnInformationCollections _extendedColumnInformations = new ExtendedColumnInformationCollections();

		private Scheme _defaultScheme;

		private readonly SchemeCollection _schemes = new SchemeCollection();

		private string _transactionNumber = string.Empty;

		public IColumn SortColumn { get; set; }

		public IColumn IndexTableColumn { get; set; }

		public IColumn SplitTableColumn { get; set; }

		public string OptimizationFilter { get; set; }

		public List<RoleArea> RoleAreas { get; set; }

		[DbColumn("category")]
		public int CategoryId { get; set; }

		public ICategory Category
		{
			get
			{
				return _category;
			}
			set
			{
				_category = value as Category;
				CategoryId = Category.Id;
			}
		}

		public EngineTypes EngineType { get; set; }

		public int PreviousId { get; set; }

		public IColumnCollection Columns => _columns;

		public IRelationCollection Relations
		{
			get
			{
				return _relations;
			}
			internal set
			{
				_relations = value as RelationCollection;
			}
		}

		[DbColumn("database", Length = 128)]
		[DbUniqueKey("_unique_name_key")]
		public string Database
		{
			get
			{
				return _database;
			}
			set
			{
				_database = value;
				base.Name = $"{Database}.{TableName}";
			}
		}

		[DbColumn("name", Length = 256)]
		[DbUniqueKey("_unique_name_key")]
		public string TableName
		{
			get
			{
				return _tableName;
			}
			set
			{
				_tableName = value;
				base.Name = $"{Database}.{TableName}";
			}
		}

		public new string Name
		{
			get
			{
				return base.Name;
			}
			set
			{
				string[] names = value.Split('.');
				if (names.Length != 2)
				{
					throw new FormatException("invalid format for TableObject.Name: use database.tablename");
				}
				base.Name = value;
				Database = names[0];
				TableName = names[1];
			}
		}

		[DbColumn("type")]
		public new TableType Type
		{
			get
			{
				return (TableType)base.Type;
			}
			set
			{
				if (Type != value)
				{
					throw new ArgumentException();
				}
			}
		}

		[DbColumn("row_count")]
		public long RowCount { get; set; }

		[DbColumn("visible")]
		public bool IsVisible { get; set; }

		[DbColumn("archived")]
		public bool IsArchived { get; set; }

		[DbColumn("object_type")]
		public int ObjectTypeCode { get; set; }

		[DbColumn("optimization_hidden")]
		public int OptimizationHidden { get; set; }

		public List<int> ExtendedObjectType { get; set; }

		public bool IsUnderArchiving { get; set; }

		public bool IsNewGroupByOrJoinTable { get; set; }

		public bool IsFavorite { get; set; }

		public PageSystem PageSystem { get; set; }

		public int ColumnCount => Columns.Count;

		public bool MultiOptimizations { get; set; }

		public IDictionary<int, Tuple<int, int>> OptimizationSelected { get; set; }

		private bool IsArchiveChecked
		{
			get
			{
				return _isArchiveChecked;
			}
			set
			{
				_isArchiveChecked = value;
			}
		}

		public IIndexCollection Indexes
		{
			get
			{
				return _indexes;
			}
			private set
			{
				_indexes = value as IndexCollection;
			}
		}

		public IDictionary<int, List<Tuple<int, List<string>>>> RoleBasedFilters
		{
			get
			{
				ITableObject baseObject = GetBaseObject();
				if (baseObject.Id != base.Id)
				{
					return baseObject.RoleBasedFilters;
				}
				return _roleBasedFilters;
			}
			set
			{
				ITableObject baseObject = GetBaseObject();
				if (baseObject.Id != base.Id)
				{
					baseObject.RoleBasedFilters = value;
				}
				else
				{
					_roleBasedFilters = value;
				}
			}
		}

		public IDictionary<int, List<Tuple<string, string>>> RoleBasedFiltersNew { get; set; }

		public string RoleBasedOptimization { get; set; }

		public IEnumerable<RowVisibilityCountCache> RoleBasedFilterRowCount { get; set; }

		public IExtendedColumnInformationCollection ExtendedColumnInformations => _extendedColumnInformations;

		public IOrderAreaCollection OrderAreas { get; internal set; }

		[DbColumn("default_scheme")]
		public int DefaultSchemeId { get; set; }

		public IScheme DefaultScheme
		{
			get
			{
				return _defaultScheme;
			}
			set
			{
				_defaultScheme = value as Scheme;
				DefaultSchemeId = DefaultScheme.Id;
			}
		}

		public ISchemeCollection Schemes => _schemes;

		[DbColumn("transaction_nr")]
		public string TransactionNumber
		{
			get
			{
				return _transactionNumber;
			}
			set
			{
				if (value != null)
				{
					_transactionNumber = value;
				}
			}
		}

		protected TableObject(TableType type)
			: base((DataObjectType)type)
		{
			OrderAreas = new OrderAreaCollection(this);
			RoleAreas = new List<RoleArea>();
		}

		public void AddRelation(IRelation relation)
		{
			if (relation != null)
			{
				_relations.Add(relation as Relation);
			}
		}

		public void RemoveRelation(IRelation relation)
		{
			_relations.Remove(relation);
			_relations.RemoveWhere((IRelation x) => x.RelationId == relation.RelationId);
		}

		public override void Clone(ref TableObject t)
		{
			base.Clone(ref t);
			t.IndexTableColumn = IndexTableColumn;
			t.SortColumn = SortColumn;
			t.SplitTableColumn = SplitTableColumn;
			t.RowCount = RowCount;
			t.RoleAreas = new List<RoleArea>(RoleAreas);
			t.Category = Category;
			t.DefaultScheme = DefaultScheme;
			t.OrderAreas = OrderAreas.Clone() as IOrderAreaCollection;
			t.TransactionNumber = TransactionNumber;
			t.RoleBasedFilters = RoleBasedFilters;
			t.RoleBasedFiltersNew = RoleBasedFiltersNew;
			t.RoleBasedFilterRowCount = RoleBasedFilterRowCount;
			t.RoleBasedOptimization = RoleBasedOptimization;
			t.IsArchived = IsArchived;
			t.IsArchiveChecked = IsArchiveChecked;
			t.Indexes = Indexes.Clone() as IIndexCollection;
			t.MultiOptimizations = MultiOptimizations;
			t.OptimizationSelected = OptimizationSelected;
			t.OptimizationHidden = OptimizationHidden;
			t.ObjectTypeCode = ObjectTypeCode;
			t.IsNewGroupByOrJoinTable = IsNewGroupByOrJoinTable;
			RelationCollection relations = new RelationCollection();
			foreach (IRelation rel in Relations)
			{
				Relation r = new Relation();
				r.RelationId = rel.RelationId;
				r.AddRange(rel.Select((IColumnConnection cc) => new ColumnConnection
				{
					Source = cc.Source,
					Target = cc.Target,
					Operator = cc.Operator,
					FullLine = cc.FullLine,
					RelationType = cc.RelationType,
					ExtInfo = cc.ExtInfo,
					ColumnExtInfo = cc.ColumnExtInfo,
					RelationId = cc.RelationId,
					UserDefined = cc.UserDefined
				}));
				relations.Add(r);
			}
			t._relations = relations;
			foreach (KeyValuePair<string, IList<IColumnConnection>> extColInf in ExtendedColumnInformations)
			{
				if (!t.ExtendedColumnInformations.ContainsKey(extColInf.Key))
				{
					t.ExtendedColumnInformations.Add(extColInf.Key, new List<IColumnConnection>());
				}
				foreach (IColumnConnection colConn in extColInf.Value)
				{
					t.ExtendedColumnInformations[extColInf.Key].Add(new ColumnConnection
					{
						Source = colConn.Source,
						Target = colConn.Target,
						Operator = colConn.Operator,
						FullLine = colConn.FullLine,
						RelationType = colConn.RelationType,
						ExtInfo = colConn.ExtInfo,
						ColumnExtInfo = colConn.ColumnExtInfo,
						RelationId = colConn.RelationId
					});
				}
			}
			t.PageSystem = ((PageSystem == null) ? null : PageSystem.Clone());
		}

		public abstract object Clone();

		public ITableObject GetBaseObject()
		{
			if (Type == TableType.Issue && ((Issue)this).IssueType == IssueType.Filter)
			{
				return ((TableObject)((Issue)this).FilterTableObject).GetBaseObject();
			}
			return this;
		}

		public string GetRoleBasedFilterSQL(IUser user)
		{
			StringBuilder sb = new StringBuilder();
			ITableObject baseObject = GetBaseObject();
			if (baseObject.RoleBasedFilters != null)
			{
				foreach (KeyValuePair<int, List<Tuple<int, List<string>>>> item in baseObject.RoleBasedFilters.Where((KeyValuePair<int, List<Tuple<int, List<string>>>> rbf) => user.Roles.Any((IRole r) => r.Id == rbf.Key) && rbf.Value.Count > 0))
				{
					foreach (Tuple<int, List<string>> roleFilterItem in item.Value)
					{
						IColumn column = baseObject.Columns[roleFilterItem.Item1];
						if (column == null)
						{
							continue;
						}
						if (sb.Length != 0)
						{
							sb.Append(" AND ");
						}
						if (roleFilterItem.Item2.Contains(string.Empty))
						{
							sb.Append($"(`{column.Name}` IS NOT NULL AND ");
						}
						else
						{
							sb.Append($"(`{column.Name}` IS NULL OR ");
						}
						if (column.DataType == SqlType.Date || column.DataType == SqlType.DateTime)
						{
							List<string> dates = new List<string>();
							foreach (string item2 in roleFilterItem.Item2)
							{
								if (DateTime.TryParse(item2, CultureInfo.InvariantCulture, DateTimeStyles.None, out var date))
								{
									if (column.DataType == SqlType.Date)
									{
										dates.Add(date.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture));
									}
									else
									{
										dates.Add(date.ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture));
									}
								}
							}
							sb.Append(string.Format("`{0}` NOT IN ('{1}'))", column.Name, string.Join("', '", dates)));
						}
						else if (column.DataType == SqlType.Time)
						{
							List<string> times = new List<string>();
							foreach (string item3 in roleFilterItem.Item2)
							{
								if (TimeSpan.TryParse(item3, CultureInfo.InvariantCulture, out var time))
								{
									times.Add(time.ToString("c", CultureInfo.InvariantCulture));
								}
							}
							sb.Append(string.Format("`{0}` NOT IN ('{1}'))", column.Name, string.Join("', '", times)));
						}
						else if (column.DataType == SqlType.Decimal)
						{
							List<string> numbers3 = new List<string>();
							foreach (string item4 in roleFilterItem.Item2)
							{
								if (decimal.TryParse(item4, NumberStyles.Any, CultureInfo.InvariantCulture, out var number3))
								{
									numbers3.Add(number3.ToString(CultureInfo.InvariantCulture));
								}
							}
							sb.Append(string.Format("`{0}` NOT IN ({1}))", column.Name, string.Join(", ", numbers3)));
						}
						else if (column.DataType == SqlType.Numeric)
						{
							List<string> numbers2 = new List<string>();
							foreach (string item5 in roleFilterItem.Item2)
							{
								if (double.TryParse(item5, NumberStyles.Any, CultureInfo.InvariantCulture, out var number2))
								{
									numbers2.Add(number2.ToString(CultureInfo.InvariantCulture));
								}
							}
							sb.Append(string.Format("`{0}` NOT IN ({1}))", column.Name, string.Join(", ", numbers2)));
						}
						else if (column.DataType == SqlType.Integer)
						{
							List<string> numbers = new List<string>();
							foreach (string item6 in roleFilterItem.Item2)
							{
								if (int.TryParse(item6, NumberStyles.Any, CultureInfo.InvariantCulture, out var number))
								{
									numbers.Add(number.ToString(CultureInfo.InvariantCulture));
								}
							}
							sb.Append(string.Format("`{0}` NOT IN ({1}))", column.Name, string.Join(", ", numbers)));
						}
						else
						{
							sb.Append(string.Format("`{0}` NOT IN ('{1}'))", column.Name, string.Join("', '", roleFilterItem.Item2)));
						}
					}
				}
			}
			return sb.ToString();
		}
	}
}
