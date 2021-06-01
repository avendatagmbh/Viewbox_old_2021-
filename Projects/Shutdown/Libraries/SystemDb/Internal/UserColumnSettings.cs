using System;
using DbAccess.Attributes;

namespace SystemDb.Internal
{
	[DbTable("user_column_settings", ForceInnoDb = true)]
	internal class UserColumnSettings : IUserColumnSettings, ICloneable
	{
		[DbColumn("column_id")]
		public int ColumnId { get; set; }

		[DbColumn("user_id")]
		public int UserId { get; set; }

		[DbColumn("id")]
		[DbPrimaryKey]
		public int Id { get; set; }

		public IColumn Column { get; set; }

		public IUser User { get; set; }

		[DbColumn("visible")]
		public bool IsVisible { get; set; }

		public object Clone()
		{
			return new UserColumnSettings
			{
				Column = Column,
				ColumnId = ColumnId,
				Id = Id,
				IsVisible = IsVisible,
				User = User,
				UserId = UserId
			};
		}
	}
}
