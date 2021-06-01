using DbAccess.Attributes;

namespace SystemDb.Internal
{
	[DbTable("role_areas", ForceInnoDb = true)]
	public class RoleArea : IRoleArea
	{
		[DbColumn("id")]
		[DbPrimaryKey]
		public int Id { get; set; }

		[DbColumn("table_id")]
		public int TableId { get; set; }

		[DbColumn("role_id")]
		public int RoleId { get; set; }

		[DbColumn("mark", Length = 1)]
		public string Mark { get; set; }

		public object Clone()
		{
			return new RoleArea
			{
				Id = Id,
				TableId = TableId,
				RoleId = RoleId,
				Mark = Mark
			};
		}
	}
}
