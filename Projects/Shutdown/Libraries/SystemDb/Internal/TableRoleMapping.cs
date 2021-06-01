using System;
using DbAccess.Attributes;

namespace SystemDb.Internal
{
	[DbTable("table_roles", ForceInnoDb = true)]
	internal class TableRoleMapping : ICloneable
	{
		[DbColumn("id", AutoIncrement = true)]
		[DbPrimaryKey]
		public int Id { get; set; }

		[DbColumn("table_id")]
		[DbUniqueKey("uk_table_id_role_id")]
		public int TableId { get; set; }

		[DbColumn("role_id")]
		[DbUniqueKey("uk_table_id_role_id")]
		public int RoleId { get; set; }

		[DbColumn("right")]
		public RightType Right { get; set; }

		public object Clone()
		{
			return new TableRoleMapping
			{
				TableId = TableId,
				Id = Id,
				Right = Right,
				RoleId = RoleId
			};
		}
	}
}
