using System;
using DbAccess.Attributes;

namespace SystemDb.Internal
{
	[DbTable("table_users", ForceInnoDb = true)]
	internal class TableUserMapping : ICloneable
	{
		[DbColumn("id")]
		[DbPrimaryKey]
		public int Id { get; set; }

		[DbColumn("table_id")]
		[DbUniqueKey("uk_table_user")]
		public int TableId { get; set; }

		[DbColumn("user_id")]
		[DbUniqueKey("uk_table_user")]
		public int UserId { get; set; }

		[DbColumn("right")]
		public RightType Right { get; set; }

		public object Clone()
		{
			return new TableUserMapping
			{
				TableId = TableId,
				Id = Id,
				Right = Right,
				UserId = UserId
			};
		}
	}
}
