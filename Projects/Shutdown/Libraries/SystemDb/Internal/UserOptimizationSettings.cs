using System;
using DbAccess.Attributes;

namespace SystemDb.Internal
{
	[DbTable("user_optimization_settings", ForceInnoDb = true)]
	internal class UserOptimizationSettings : IUserOptimizationSettings, ICloneable
	{
		[DbColumn("optimization_id")]
		public int OptimizationId { get; set; }

		[DbColumn("user_id")]
		public int UserId { get; set; }

		[DbColumn("id")]
		[DbPrimaryKey]
		public int Id { get; set; }

		public IOptimization Optimization { get; set; }

		public IUser User { get; set; }

		public object Clone()
		{
			return new UserOptimizationSettings
			{
				Id = Id,
				Optimization = Optimization,
				OptimizationId = OptimizationId,
				User = User,
				UserId = UserId
			};
		}
	}
}
