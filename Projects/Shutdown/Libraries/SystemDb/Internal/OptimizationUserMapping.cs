using System;
using DbAccess.Attributes;

namespace SystemDb.Internal
{
	[DbTable("optimization_users", ForceInnoDb = true)]
	internal class OptimizationUserMapping : ICloneable
	{
		[DbColumn("id")]
		[DbPrimaryKey]
		public int Id { get; set; }

		[DbUniqueKey("uk_optimization_id_user_id")]
		[DbColumn("optimization_id")]
		public int OptimizationId { get; set; }

		[DbUniqueKey("uk_optimization_id_user_id")]
		[DbColumn("user_id")]
		public int UserId { get; set; }

		[DbColumn("visible")]
		public bool Visible { get; set; }

		public object Clone()
		{
			return new OptimizationUserMapping
			{
				OptimizationId = OptimizationId,
				Id = Id,
				Visible = Visible,
				UserId = UserId
			};
		}
	}
}
