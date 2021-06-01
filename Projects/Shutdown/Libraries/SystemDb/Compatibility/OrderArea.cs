using DbAccess.Attributes;

namespace SystemDb.Compatibility
{
	[DbTable("order_area")]
	internal class OrderArea
	{
		[DbColumn("table_name", Length = 128)]
		[DbPrimaryKey]
		public string TableName { get; set; }

		[DbColumn("part_no")]
		[DbPrimaryKey]
		public int PartNumber { get; set; }

		[DbColumn("name", Length = 128)]
		[DbPrimaryKey]
		public string Name { get; set; }

		[DbColumn("start")]
		public int Start { get; set; }

		[DbColumn("count")]
		public int Count { get; set; }
	}
}
