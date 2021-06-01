using DbAccess.Attributes;

namespace Viewbox.Structures
{
	[DbTable("guv_pos_new")]
	public class GuvStructureRow
	{
		[DbColumn("pos")]
		public string Pos { get; set; }

		[DbColumn("descr")]
		public string Description { get; set; }

		[DbColumn("parent")]
		public string Parent { get; set; }
	}
}
