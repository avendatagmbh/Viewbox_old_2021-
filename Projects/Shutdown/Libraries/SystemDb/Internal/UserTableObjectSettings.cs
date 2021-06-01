using System;
using DbAccess.Attributes;

namespace SystemDb.Internal
{
	[DbTable("user_table_settings", ForceInnoDb = true)]
	internal class UserTableObjectSettings : IUserTableObjectSettings, ICloneable
	{
		[DbColumn("table_id")]
		public int TableId { get; set; }

		[DbColumn("user_id")]
		public int UserId { get; set; }

		[DbColumn("id")]
		[DbPrimaryKey]
		public int Id { get; set; }

		public ITableObject TableObject { get; set; }

		public IUser User { get; set; }

		[DbColumn("visible")]
		public bool IsVisible { get; set; }

		public object Clone()
		{
			return new UserTableObjectSettings
			{
				TableObject = TableObject,
				TableId = TableId,
				Id = Id,
				IsVisible = IsVisible,
				User = User,
				UserId = UserId
			};
		}
	}
}
