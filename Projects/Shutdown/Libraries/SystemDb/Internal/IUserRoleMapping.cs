using DbAccess.Attributes;

namespace SystemDb.Internal
{
	public interface IUserRoleMapping
	{
		[DbColumn("id")]
		[DbPrimaryKey]
		int Id { get; set; }

		[DbColumn("user_id")]
		[DbUniqueKey("uk_user_id_role_id")]
		int UserId { get; set; }

		[DbColumn("role_id")]
		[DbUniqueKey("uk_user_id_role_id")]
		int RoleId { get; set; }

		[DbColumn("ordinal")]
		int Ordinal { get; set; }

		object Clone();
	}
}
