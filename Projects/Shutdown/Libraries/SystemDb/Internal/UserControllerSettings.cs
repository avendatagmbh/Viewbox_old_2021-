using System;
using DbAccess.Attributes;

namespace SystemDb.Internal
{
	[DbTable("user_controller_settings", ForceInnoDb = true)]
	internal class UserControllerSettings : IUserControllerSettings, ICloneable
	{
		[DbColumn("user_id")]
		public int UserId { get; set; }

		[DbColumn("id")]
		[DbPrimaryKey]
		public int Id { get; set; }

		[DbColumn("controller")]
		public string Controller { get; set; }

		public IUser User { get; set; }

		public object Clone()
		{
			return new UserControllerSettings
			{
				Id = Id,
				Controller = Controller,
				User = User,
				UserId = UserId
			};
		}
	}
}
