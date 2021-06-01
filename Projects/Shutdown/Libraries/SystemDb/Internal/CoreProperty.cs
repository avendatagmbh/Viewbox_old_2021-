using System;
using DbAccess.Attributes;

namespace SystemDb.Internal
{
	[DbTable("core_properties", ForceInnoDb = true)]
	internal class CoreProperty : ICoreProperty, ICloneable
	{
		[DbColumn("id")]
		[DbPrimaryKey]
		public int Id { get; set; }

		[DbColumn("key", Length = 128)]
		public string Key { get; set; }

		[DbColumn("value")]
		public string Value { get; set; }

		[DbColumn("type")]
		public PropertyType Type { get; set; }

		public object Clone()
		{
			return new CoreProperty
			{
				Key = Key,
				Id = Id,
				Value = Value,
				Type = Type
			};
		}
	}
}
