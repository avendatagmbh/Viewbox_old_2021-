using DbAccess.Attributes;

namespace Viewbox.Structures
{
	[DbTable("kostenarchenhierarchy_strukture_view")]
	public class BilanzstrukturAnzeigen : IBilanzstrukturAnzeigen
	{
		[DbColumn("id")]
		public string id { get; set; }

		[DbColumn("parent")]
		public string parent { get; set; }

		[DbColumn("titel")]
		public string titel { get; set; }
	}
}
