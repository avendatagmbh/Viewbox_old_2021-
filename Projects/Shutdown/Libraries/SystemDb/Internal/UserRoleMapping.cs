using System;
using DbAccess.Attributes;

namespace SystemDb.Internal
{
	[DbTable("user_roles", ForceInnoDb = true)]
	public class UserRoleMapping : ICloneable, IUserRoleMapping
	{
		[DbColumn("id")]
		[DbPrimaryKey]
		public int Id { get; set; }

		[DbColumn("user_id")]
		[DbUniqueKey("uk_user_id_role_id")]
		public int UserId { get; set; }

		[DbColumn("role_id")]
		[DbUniqueKey("uk_user_id_role_id")]
		public int RoleId { get; set; }

		[DbColumn("ordinal")]
		public int Ordinal { get; set; }

		public object Clone()
		{
			return new UserRoleMapping
			{
				UserId = UserId,
				Id = Id,
				Ordinal = Ordinal,
				RoleId = RoleId
			};
		}
	}
}
