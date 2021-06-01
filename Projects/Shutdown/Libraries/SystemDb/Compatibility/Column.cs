using DbAccess.Attributes;

namespace SystemDb.Compatibility
{
	[DbTable("col")]
	internal class Column
	{
		[DbColumn("col_id")]
		[DbPrimaryKey]
		public int Id { get; set; }

		[DbColumn("table_id")]
		[DbUniqueKey("update_guard")]
		public int TableId { get; set; }

		[DbColumn("name", Length = 128)]
		[DbUniqueKey("update_guard")]
		public string Name { get; set; }

		[DbColumn("comment", Length = 1024, AllowDbNull = true)]
		public string Comment { get; set; }

		[DbColumn("is_selected")]
		public bool IsSelected { get; set; }

		[DbColumn("type", Length = 128)]
		public string Type { get; set; }

		[DbColumn("length", AllowDbNull = true)]
		public int Length { get; set; }

		[DbColumn("decimal_place", AllowDbNull = true)]
		public short DecimalPlace { get; set; }

		[DbColumn("filter", Length = 4096, AllowDbNull = true)]
		public string Filter { get; set; }

		[DbColumn("is_empty")]
		public bool IsEmpty { get; set; }

		[DbColumn("const_value", Length = 128, AllowDbNull = true)]
		public string ConstantValue { get; set; }
	}
}
