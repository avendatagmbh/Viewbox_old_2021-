using System;
using DbAccess.Attributes;

namespace SystemDb.Internal
{
	[DbTable("user_settings", ForceInnoDb = true)]
	public class UserSetting : IUserSetting, ICloneable
	{
		[DbColumn("id")]
		[DbPrimaryKey]
		public int Id { get; set; }

		[DbColumn("name", Length = 64)]
		public string Name { get; set; }

		[DbColumn("value", Length = 256)]
		public string Value { get; set; }

		[DbColumn("user_id")]
		public int UserId { get; set; }

		public object Clone()
		{
			return MemberwiseClone();
		}
	}
}
