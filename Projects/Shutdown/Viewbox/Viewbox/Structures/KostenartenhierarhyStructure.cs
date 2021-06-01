using DbAccess.Attributes;

namespace Viewbox.Structures
{
	[DbTable("cost_element_hierarchy")]
	public class KostenartenhierarhyStructure
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
