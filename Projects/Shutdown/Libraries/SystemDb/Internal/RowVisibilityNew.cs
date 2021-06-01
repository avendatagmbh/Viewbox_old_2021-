using DbAccess.Attributes;

namespace SystemDb.Internal
{
	[DbTable("row_visibility_new", ForceInnoDb = true)]
	public class RowVisibilityNew
	{
		[DbColumn("id", AutoIncrement = true)]
		[DbPrimaryKey]
		public long Id { get; set; }

		[DbColumn("role_id")]
		public int RoleId { get; set; }

		[DbColumn("column_name")]
		public string ColumnName { get; set; }

		[DbColumn("filter_value", Length = 1000)]
		public string FilterValue { get; set; }
	}
}
