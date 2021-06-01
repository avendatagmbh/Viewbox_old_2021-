using System;
using DbAccess.Attributes;

namespace SystemDb.Internal
{
	[DbTable("user_property_settings", ForceInnoDb = true)]
	internal class UserPropertySettings : IUserPropertySettings, ICloneable
	{
		[DbColumn("property_id")]
		public int PropertyId { get; set; }

		[DbColumn("user_id")]
		public int UserId { get; set; }

		[DbColumn("id")]
		[DbPrimaryKey]
		public int Id { get; set; }

		public IProperty Property { get; set; }

		public IUser User { get; set; }

		[DbColumn("value")]
		public string Value { get; set; }

		public object Clone()
		{
			return new UserPropertySettings
			{
				Property = Property,
				PropertyId = PropertyId,
				Id = Id,
				Value = Value,
				User = User,
				UserId = UserId
			};
		}
	}
}
