using System;
using DbAccess.Attributes;

namespace SystemDb.Internal
{
	[DbTable("object_type_relations", ForceInnoDb = true)]
	public class ObjectTypeRelations : IObjectTypeRelations, ICloneable
	{
		[DbColumn("id")]
		[DbPrimaryKey]
		public int Id { get; set; }

		[DbColumn("ref_id")]
		public int Ref_Id { get; set; }

		[DbColumn("object_id")]
		public int Object_Id { get; set; }

		public object Clone()
		{
			return MemberwiseClone();
		}
	}
}
