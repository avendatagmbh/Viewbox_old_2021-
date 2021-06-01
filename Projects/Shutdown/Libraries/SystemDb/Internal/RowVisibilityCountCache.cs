using DbAccess.Attributes;

namespace SystemDb.Internal
{
	[DbTable("row_visibility_count_cache", ForceInnoDb = true)]
	public class RowVisibilityCountCache
	{
		[DbColumn("role_id")]
		public string RoleId { get; set; }

		[DbColumn("table_id")]
		public int TableId { get; set; }

		[DbColumn("opt_id")]
		public string OptimizationId { get; set; }

		[DbColumn("count")]
		public long Count { get; set; }

		[DbColumn("id", AutoIncrement = true)]
		[DbPrimaryKey]
		public long Id { get; set; }
	}
}
