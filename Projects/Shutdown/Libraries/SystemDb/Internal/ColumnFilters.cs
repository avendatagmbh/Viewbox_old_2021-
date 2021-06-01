using DbAccess.Attributes;

namespace SystemDb.Internal
{
	[DbTable("column_filters", ForceInnoDb = true)]
	public class ColumnFilters : IColumnFilters
	{
		[DbColumn("id")]
		[DbPrimaryKey]
		public int Id { get; set; }

		[DbColumn("table_id")]
		public int TableId { get; set; }

		[DbColumn("column_id")]
		public int ColumnId { get; set; }
	}
}
