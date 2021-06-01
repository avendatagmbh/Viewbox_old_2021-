using DbAccess.Attributes;

namespace Viewbox.Structures
{
	[DbTable("COST_center_HIERARCHY")]
	public class KostenstellengruppeAnzeigenStruct
	{
		[DbColumn("mandt")]
		public string Mandt { get; set; }

		[DbColumn("position")]
		public string Pos { get; set; }

		[DbColumn("descr")]
		public string Description { get; set; }

		[DbColumn("parent")]
		public string Parent { get; set; }

		[DbColumn("Sub_type")]
		public string Sub_type { get; set; }
	}
}
