using System;
using DbAccess.Attributes;

namespace SystemDb.Internal
{
	[DbTable("user_table_column_width_settings", ForceInnoDb = true)]
	internal class UserTableColumnWidthsSettings : IUserTableColumnWidthsSettings, ICloneable
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

		[DbColumn("column_widths", Length = 100000)]
		public string ColumnWidths { get; set; }

		public object Clone()
		{
			return new UserTableColumnWidthsSettings
			{
				TableObject = TableObject,
				TableId = TableId,
				Id = Id,
				ColumnWidths = ColumnWidths,
				User = User,
				UserId = UserId
			};
		}
	}
}
