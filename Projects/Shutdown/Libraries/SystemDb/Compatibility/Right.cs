using DbAccess.Attributes;

namespace SystemDb.Compatibility
{
	[DbTable("right")]
	internal class Right
	{
		[DbColumn("right_id")]
		[DbPrimaryKey]
		public int Id { get; set; }

		[DbColumn("name", Length = 256)]
		public string Name { get; set; }
	}
}
