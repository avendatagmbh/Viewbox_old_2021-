using DbAccess.Attributes;

namespace SystemDb.Compatibility
{
	[DbTable("procedures")]
	internal class Procedures
	{
		[DbColumn("id")]
		[DbPrimaryKey]
		public int Id { get; set; }

		[DbColumn("table_id")]
		public int TableId { get; set; }

		[DbColumn("name", Length = 50, AllowDbNull = true)]
		public string Name { get; set; }

		[DbColumn("description", Length = 255)]
		public string Comment { get; set; }

		[DbColumn("DoClientSplit")]
		public bool ClientSplitting { get; set; }

		[DbColumn("DoCompCodeSplit")]
		public bool CompanyCodeSplitting { get; set; }

		[DbColumn("DoFYearSplit")]
		public bool FinancialYearSplitting { get; set; }
	}
}
