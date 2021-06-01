using System;
using DbAccess.Attributes;

namespace SystemDb.Internal
{
	[DbTable("category_roles", ForceInnoDb = true)]
	internal class CategoryRoleMapping : ICloneable
	{
		[DbColumn("id")]
		[DbPrimaryKey]
		public int Id { get; set; }

		[DbColumn("category_id")]
		[DbUniqueKey("uk_category_id_role_id")]
		public int CategoryId { get; set; }

		[DbColumn("role_id")]
		[DbUniqueKey("uk_category_id_role_id")]
		public int RoleId { get; set; }

		[DbColumn("right")]
		public RightType Right { get; set; }

		public object Clone()
		{
			return new CategoryRoleMapping
			{
				CategoryId = CategoryId,
				Id = Id,
				Right = Right,
				RoleId = RoleId
			};
		}
	}
}
