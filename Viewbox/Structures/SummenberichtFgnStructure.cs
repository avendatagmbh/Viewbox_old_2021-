using DbAccess.Attributes;

namespace Viewbox.Structures
{
	[DbTable("sets_temp")]
	public class SummenberichtFgnStructure
	{
		[DbColumn("KSTAR0")]
		public string KSTAR0 { get; set; }

		[DbColumn("setname")]
		public string setname { get; set; }

		[DbColumn("id")]
		public string id { get; set; }

		[DbColumn("parent_id")]
		public string parent_id { get; set; }

		[DbColumn("parent_sets")]
		public string parent_sets { get; set; }

		[DbColumn("KTOPL")]
		public string KTOPL { get; set; }

		[DbColumn("commen")]
		public string commen { get; set; }

		[DbColumn("nod")]
		public string nod { get; set; }

		[DbColumn("CLIENT")]
		public string CLIENT { get; set; }
	}
}
