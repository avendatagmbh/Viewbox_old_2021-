using DbAccess.Attributes;

namespace Viewbox.Structures
{
	[DbTable("zckost80_002_dyn")]
	public class SummenberichtFgnData
	{
		[DbColumn("kstar")]
		public string kstar { get; set; }

		[DbColumn("plankosten")]
		public string plankosten { get; set; }

		[DbColumn("istkosten")]
		public string istkosten { get; set; }

		[DbColumn("abw")]
		public string abw { get; set; }

		[DbColumn("abwp")]
		public string abwp { get; set; }

		[DbColumn("id")]
		public string id { get; set; }

		[DbColumn("ltext")]
		public string ltext { get; set; }
	}
}
