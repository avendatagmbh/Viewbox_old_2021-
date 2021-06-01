using System;
using DbAccess.Attributes;

namespace SystemDb.Internal
{
	[DbTable("user_table_object_order_settings", ForceInnoDb = true)]
	internal class UserTableObjectOrderSettings : IUserTableObjectOrderSettings, ICloneable
	{
		[DbColumn("user_id")]
		public int UserId { get; set; }

		[DbColumn("id")]
		[DbPrimaryKey]
		public int Id { get; set; }

		[DbColumn("type")]
		public TableType Type { get; set; }

		public IUser User { get; set; }

		[DbColumn("table_object_order", Length = 100000)]
		public string TableObjectOrder { get; set; }

		public object Clone()
		{
			return new UserTableObjectOrderSettings
			{
				Type = Type,
				Id = Id,
				TableObjectOrder = TableObjectOrder,
				User = User,
				UserId = UserId
			};
		}
	}
}
