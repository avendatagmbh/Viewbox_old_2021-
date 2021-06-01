using DbAccess.Attributes;

namespace SystemDb.Internal
{
	[DbTable("empty_distinct_columns", ForceInnoDb = true)]
	public class EmptyDistinctColumn : IEmptyDistinctColumn
	{
		[DbColumn("id")]
		[DbPrimaryKey]
		public int Id { get; set; }

		[DbColumn("min_value")]
		public string MinValue { get; set; }

		[DbColumn("max_value")]
		public string MaxValue { get; set; }

		[DbColumn("table_id")]
		[DbIndex("table_id")]
		public int TableId { get; set; }

		[DbColumn("index_value")]
		public string IndexValue { get; set; }

		[DbColumn("split_value")]
		public string SplitValue { get; set; }

		[DbColumn("sort_value")]
		public string SortValue { get; set; }

		[DbColumn("column_id")]
		[DbIndex("column_id")]
		public int ColumnId { get; set; }

		[DbColumn("is_empty")]
		public bool IsEmpty { get; set; }

		[DbColumn("one_distinct")]
		public bool OneDistinct { get; set; }
	}
}
