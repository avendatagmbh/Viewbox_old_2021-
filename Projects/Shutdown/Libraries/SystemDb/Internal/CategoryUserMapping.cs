using System;
using DbAccess.Attributes;

namespace SystemDb.Internal
{
	[DbTable("category_users", ForceInnoDb = true)]
	internal class CategoryUserMapping : ICloneable
	{
		[DbColumn("id")]
		[DbPrimaryKey]
		public int Id { get; set; }

		[DbColumn("category_id")]
		[DbUniqueKey("uk_category_id_user_id")]
		public int CategoryId { get; set; }

		[DbColumn("user_id")]
		[DbUniqueKey("uk_category_id_user_id")]
		public int UserId { get; set; }

		[DbColumn("right")]
		public RightType Right { get; set; }

		public object Clone()
		{
			return new CategoryUserMapping
			{
				CategoryId = CategoryId,
				Id = Id,
				Right = Right,
				UserId = UserId
			};
		}
	}
}
