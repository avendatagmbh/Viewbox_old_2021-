using System;
using DbAccess.Attributes;

namespace SystemDb.Internal
{
	[DbTable("column_roles", ForceInnoDb = true)]
	internal class ColumnRoleMapping : ICloneable
	{
		[DbColumn("id")]
		[DbPrimaryKey]
		public int Id { get; set; }

		[DbColumn("column_id")]
		[DbUniqueKey("uk_column_id_role_id")]
		public int ColumnId { get; set; }

		[DbColumn("role_id")]
		[DbUniqueKey("uk_column_id_role_id")]
		public int RoleId { get; set; }

		[DbColumn("right")]
		public RightType Right { get; set; }

		public object Clone()
		{
			return new ColumnRoleMapping
			{
				ColumnId = ColumnId,
				Id = Id,
				Right = Right,
				RoleId = RoleId
			};
		}
	}
}
