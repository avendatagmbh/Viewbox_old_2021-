using System;
using DbAccess.Attributes;

namespace SystemDb.Internal
{
	[DbTable("user_column_order_settings", ForceInnoDb = true)]
	internal class UserColumnOrderSettings : IUserColumnOrderSettings, ICloneable
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

		[DbColumn("column_order", Length = 10000)]
		public string ColumnOrder { get; set; }

		public object Clone()
		{
			return new UserColumnOrderSettings
			{
				TableObject = TableObject,
				TableId = TableId,
				Id = Id,
				ColumnOrder = ColumnOrder,
				User = User,
				UserId = UserId
			};
		}
	}
}
