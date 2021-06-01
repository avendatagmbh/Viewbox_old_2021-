using DbAccess.Attributes;

namespace SystemDb.Compatibility
{
	[DbTable("cat")]
	internal class Catalog
	{
		[DbColumn("id")]
		[DbPrimaryKey]
		public int Id { get; set; }

		[DbColumn("name", Length = 65536, AllowDbNull = true)]
		public string Name { get; set; }
	}
}
