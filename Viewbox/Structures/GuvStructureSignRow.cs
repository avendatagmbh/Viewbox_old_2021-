using DbAccess.Attributes;

namespace Viewbox.Structures
{
	[DbTable("guv_view")]
	public class GuvStructureSignRow
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

		[DbColumn("parent")]
		public string Pos { get; set; }
	}
}
