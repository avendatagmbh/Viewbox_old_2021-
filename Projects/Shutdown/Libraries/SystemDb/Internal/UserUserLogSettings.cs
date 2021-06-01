using System;
using DbAccess.Attributes;

namespace SystemDb.Internal
{
	[DbTable("user_userlog_settings", ForceInnoDb = true)]
	internal class UserUserLogSettings : IUserUserLogSettings, ICloneable
	{
		[DbColumn("user_id")]
		public int UserId { get; set; }

		[DbColumn("id")]
		[DbPrimaryKey]
		public int Id { get; set; }

		public IUser User { get; set; }

		[DbColumn("userlog_id")]
		public int UserLogId { get; set; }

		[DbColumn("visible")]
		public bool IsVisible { get; set; }

		public object Clone()
		{
			return new UserUserLogSettings
			{
				UserLogId = UserLogId,
				Id = Id,
				IsVisible = IsVisible,
				User = User,
				UserId = UserId
			};
		}
	}
}
