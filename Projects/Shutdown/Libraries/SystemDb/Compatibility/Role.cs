using DbAccess.Attributes;

namespace SystemDb.Compatibility
{
	[DbTable("role")]
	internal class Role
	{
		[DbColumn("id")]
		[DbPrimaryKey]
		public int Id { get; set; }

		[DbColumn("name", Length = 64, AllowDbNull = true)]
		public string Name { get; set; }

		[DbColumn("description", Length = 4096, AllowDbNull = true)]
		public string Description { get; set; }

		[DbColumn("is_admin")]
		public bool IsAdmin { get; set; }
	}
}
