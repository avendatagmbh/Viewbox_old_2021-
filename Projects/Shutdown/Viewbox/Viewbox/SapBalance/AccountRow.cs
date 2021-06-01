using DbAccess.Attributes;

namespace Viewbox.SapBalance
{
	[DbTable("blg_bilanz_guv")]
	public class AccountRow
	{
		[DbColumn("MANDT", Length = 3)]
		public string Client { get; set; }

		[DbColumn("BUKRS", Length = 4)]
		public string CompanyCode { get; set; }

		[DbColumn("GJAHR", Length = 4)]
		public string FinancialYear { get; set; }

		[DbColumn("GesBer", Length = 20)]
		public string GesBer { get; set; }

		[DbColumn("Konto", Length = 20)]
		public string Account { get; set; }

		[DbColumn("KontoBez", Length = 50)]
		public string Description { get; set; }

		[DbColumn("Betrag")]
		public decimal Value { get; set; }

		[DbColumn("_row_no_", AutoIncrement = true)]
		[DbPrimaryKey]
		public int Id { get; set; }

		[DbColumn("WAERS")]
		public string Currency { get; set; }

		[DbColumn("ktopl")]
		public string AccountStructure { get; set; }
	}
}
