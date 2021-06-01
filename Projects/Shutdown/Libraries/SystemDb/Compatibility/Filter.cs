using DbAccess.Attributes;

namespace SystemDb.Compatibility
{
	[DbTable("filter")]
	internal class Filter
	{
		[DbColumn("id")]
		[DbPrimaryKey]
		public int Id { get; set; }

		[DbColumn("parent_id", AllowDbNull = true)]
		public int? ParentId { get; set; }

		[DbColumn("description", Length = 128, AllowDbNull = true)]
		public string Description { get; set; }

		[DbColumn("value", Length = 128)]
		public string Value { get; set; }

		[DbColumn("opt_id", AllowDbNull = true)]
		[DbIndex("opt_id")]
		public OptimizationType OptimizationId { get; set; }

		[DbColumn("cat_id")]
		[DbIndex("cat_id")]
		public int CatalogId { get; set; }
	}
}
