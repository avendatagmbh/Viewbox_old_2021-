using System;
using DbAccess.Attributes;

namespace SystemDb.Internal
{
	[DbTable("column_users", ForceInnoDb = true)]
	internal class ColumnUserMapping : ICloneable
	{
		[DbColumn("id")]
		[DbPrimaryKey]
		public int Id { get; set; }

		[DbColumn("column_id")]
		[DbUniqueKey("uk_column_id_user_id")]
		public int ColumnId { get; set; }

		[DbColumn("user_id")]
		[DbUniqueKey("uk_column_id_user_id")]
		public int UserId { get; set; }

		[DbColumn("right")]
		public RightType Right { get; set; }

		public object Clone()
		{
			return new ColumnUserMapping
			{
				ColumnId = ColumnId,
				Id = Id,
				Right = Right,
				UserId = UserId
			};
		}
	}
}
