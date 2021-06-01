using DbAccess.Attributes;

namespace SystemDb.Compatibility
{
	[DbTable("table_parts")]
	internal class TablePart
	{
		[DbColumn("table_name", Length = 128)]
		[DbPrimaryKey]
		public string TableName { get; set; }

		[DbColumn("part_no")]
		[DbPrimaryKey]
		public int PartNumber { get; set; }

		[DbColumn("row_no_start")]
		public int StartRow { get; set; }

		[DbColumn("name", Length = 128)]
		public string Name { get; set; }
	}
}
