using DbAccess.Attributes;

namespace Viewbox.Structures
{
	[DbTable("bilanz_view")]
	public class BilanzStructureSignRow
	{
		[DbColumn("set_of_books_id")]
		public string Mandant { get; set; }

		[DbColumn("konto")]
		public string Konto { get; set; }

		[DbColumn("konto_bezeichnung")]
		public string Konto_Be { get; set; }

		[DbColumn("monat_saldo")]
		public string Monat_S { get; set; }

		[DbColumn("year_saldo")]
		public string Year_S { get; set; }

		[DbColumn("id")]
		public string Pos { get; set; }
	}
}
