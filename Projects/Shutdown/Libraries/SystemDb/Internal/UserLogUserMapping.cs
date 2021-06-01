using System;
using DbAccess.Attributes;

namespace SystemDb.Internal
{
	[DbTable("userlog_users", ForceInnoDb = true)]
	internal class UserLogUserMapping : ICloneable
	{
		[DbColumn("id")]
		[DbPrimaryKey]
		public int Id { get; set; }

		[DbColumn("userlog_id")]
		[DbUniqueKey("uk_userlog_user")]
		public int UserLogId { get; set; }

		[DbColumn("user_id")]
		[DbUniqueKey("uk_userlog_user")]
		public int UserId { get; set; }

		[DbColumn("right")]
		public RightType Right { get; set; }

		public object Clone()
		{
			return new UserLogUserMapping
			{
				UserLogId = UserLogId,
				Id = Id,
				Right = Right,
				UserId = UserId
			};
		}
	}
}
