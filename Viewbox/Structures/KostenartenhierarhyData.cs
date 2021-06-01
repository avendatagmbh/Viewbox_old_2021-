using DbAccess.Attributes;

namespace Viewbox.Structures
{
	[DbTable("co_kostenartengruppenhierarchie_dyn")]
	public class KostenartenhierarhyData
	{
		[DbColumn("mandt")]
		public string Mandant { get; set; }

		[DbColumn("kstar")]
		public string Konto { get; set; }

		[DbColumn("kstar_desc")]
		public string Konto_Be { get; set; }

		[DbColumn("ktopl")]
		public string Year_S { get; set; }

		[DbColumn("parent")]
		public string Pos { get; set; }
	}
}
