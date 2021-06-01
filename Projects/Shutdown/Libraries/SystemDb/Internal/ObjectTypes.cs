using System;
using DbAccess.Attributes;

namespace SystemDb.Internal
{
	[DbTable("object_types", ForceInnoDb = true)]
	internal class ObjectTypes : IObjectTypes, ICloneable
	{
		[DbColumn("id")]
		[DbPrimaryKey]
		public int Id { get; set; }

		[DbColumn("value")]
		public string Value { get; set; }

		public object Clone()
		{
			return MemberwiseClone();
		}
	}
}
