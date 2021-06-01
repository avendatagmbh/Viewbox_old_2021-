using DbAccess.Attributes;

namespace SystemDb.Compatibility
{
	[DbTable("table")]
	internal class Table
	{
		[DbColumn("table_id")]
		[DbPrimaryKey]
		public int Id { get; set; }

		[DbColumn("system_id")]
		[DbIndex("system_id")]
		public int SystemId { get; set; }

		[DbColumn("name", Length = 128)]
		[DbUniqueKey]
		public string Name { get; set; }

		[DbColumn("comment", Length = 1024, AllowDbNull = true)]
		public string Comment { get; set; }

		[DbColumn("filter", Length = 16384, AllowDbNull = true)]
		public string Filter { get; set; }

		[DbColumn("row_count")]
		public int RowCount { get; set; }

		[DbColumn("mandt_split")]
		public bool ClientSplitting { get; set; }

		[DbColumn("mandt_col", Length = 128, AllowDbNull = true)]
		public string ClientColumnName { get; set; }

		[DbColumn("gjahr_split")]
		public bool FinancialYearSplitting { get; set; }

		[DbColumn("gjahr_col", Length = 128, AllowDbNull = true)]
		public string FinancialYearColumnName { get; set; }

		[DbColumn("bukrs_split")]
		public bool CompanyCodeSplitting { get; set; }

		[DbColumn("bukrs_col", Length = 128, AllowDbNull = true)]
		public string CompanyCodeColumnName { get; set; }

		[DbColumn("is_view")]
		public bool UserDefined { get; set; }

		[DbColumn("parent_table_id", AllowDbNull = true)]
		[DbIndex("parent_table_id")]
		public int ParentTableId { get; set; }
	}
}
