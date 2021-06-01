using System;
using DbAccess.Attributes;

namespace SystemDb.Internal
{
	[DbTable("optimization_roles", ForceInnoDb = true)]
	internal class OptimizationRoleMapping : ICloneable
	{
		[DbColumn("id", AutoIncrement = true)]
		[DbPrimaryKey]
		public int Id { get; set; }

		[DbUniqueKey("uk_optimization_id_role_id")]
		[DbColumn("optimization_id")]
		public int OptimizationId { get; set; }

		[DbUniqueKey("uk_optimization_id_role_id")]
		[DbColumn("role_id")]
		public int RoleId { get; set; }

		[DbColumn("visible")]
		public bool Visible { get; set; }

		public object Clone()
		{
			return new OptimizationRoleMapping
			{
				OptimizationId = OptimizationId,
				Id = Id,
				Visible = Visible,
				RoleId = RoleId
			};
		}
	}
}
