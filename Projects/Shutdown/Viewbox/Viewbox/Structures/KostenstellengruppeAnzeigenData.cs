using DbAccess.Attributes;

namespace Viewbox.Structures
{
	[DbTable("co_kostenstellengruppenhierarchie_dyn")]
	public class KostenstellengruppeAnzeigenData
	{
		[DbColumn("mandt")]
		public string Mandant { get; set; }

		[DbColumn("kostl")]
		public string Konto { get; set; }

		[DbColumn("kostl_desc")]
		public string Konto_Be { get; set; }

		[DbColumn("kokrs")]
		public string Year_S { get; set; }

		[DbColumn("parent")]
		public string Pos { get; set; }
	}
}
