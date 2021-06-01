using DbAccess.Attributes;

namespace SystemDb.Compatibility
{
	[DbTable("mandt")]
	internal class Client
	{
		[DbColumn("id")]
		[DbPrimaryKey]
		public int Id { get; set; }

		[DbColumn("name", Length = 128)]
		public string Name { get; set; }

		[DbColumn("ort", Length = 128)]
		public string Location { get; set; }
	}
}
