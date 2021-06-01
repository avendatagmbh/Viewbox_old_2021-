using DbAccess.Attributes;

namespace Viewbox.Structures
{
	[DbTable("bilanz_pos_new")]
	public class BilanzStructureRow
	{
		[DbColumn("id")]
		public string Pos { get; set; }

		[DbColumn("name")]
		public string Description { get; set; }

		[DbColumn("parentid")]
		public string Parent { get; set; }
	}
}
