using DbAccess.Attributes;

namespace Viewbox.Structures
{
	[DbTable("CO_projekte_summenbericht")]
	public class Summenbericht
	{
		[DbColumn("MANDT")]
		public string MANDT { get; set; }

		[DbColumn("VBUKR")]
		public string VBUKR { get; set; }

		[DbColumn("ID")]
		public string ID { get; set; }

		[DbColumn("Parent")]
		public string Parent { get; set; }

		[DbColumn("Objekt")]
		public string Objekt { get; set; }

		[DbColumn("Descr")]
		public string Descr { get; set; }

		[DbColumn("Gen.Soll")]
		public string GenSoll { get; set; }

		[DbColumn("Istkosten")]
		public string Istkosten { get; set; }

		[DbColumn("Obligo")]
		public string Obligo { get; set; }

		[DbColumn("Anzahlung")]
		public string Anzahlung { get; set; }

		[DbColumn("lief")]
		public string lief { get; set; }
	}
}
