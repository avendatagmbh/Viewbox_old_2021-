using DbAccess.Attributes;

namespace SystemDb.Internal
{
	[DbTable("parameter_collections", ForceInnoDb = true)]
	internal class ParameterCollectionMapping
	{
		[DbColumn("id", AutoIncrement = true)]
		[DbPrimaryKey]
		public int Id { get; set; }

		[DbColumn("parameter_id")]
		public int ParameterId { get; set; }

		[DbColumn("collection_id")]
		public int CollectionId { get; set; }
	}
}
