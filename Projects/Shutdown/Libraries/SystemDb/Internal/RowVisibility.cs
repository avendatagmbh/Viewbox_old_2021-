using DbAccess.Attributes;

namespace SystemDb.Internal
{
	[DbTable("row_visibility", ForceInnoDb = true)]
	public class RowVisibility
	{
		[DbColumn("id", AutoIncrement = true)]
		[DbPrimaryKey]
		public long Id { get; set; }

		[DbColumn("table_id")]
		public int TableId { get; set; }

		[DbColumn("role_id")]
		public int RoleId { get; set; }

		[DbColumn("column_id")]
		public int ColumnId { get; set; }

		[DbColumn("filter_value", Length = 1000)]
		public string FilterValue { get; set; }
	}
}
