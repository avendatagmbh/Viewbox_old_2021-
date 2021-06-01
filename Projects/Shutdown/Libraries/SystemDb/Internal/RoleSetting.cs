using System;
using DbAccess.Attributes;

namespace SystemDb.Internal
{
	[DbTable("role_settings", ForceInnoDb = true)]
	public class RoleSetting : IRoleSetting, ICloneable
	{
		[DbColumn("id")]
		[DbPrimaryKey]
		public int Id { get; set; }

		[DbColumn("name", Length = 64)]
		public string Name { get; set; }

		[DbColumn("value", Length = 256)]
		public string Value { get; set; }

		[DbColumn("role_id")]
		public int RoleId { get; set; }

		public object Clone()
		{
			return MemberwiseClone();
		}
	}
}
