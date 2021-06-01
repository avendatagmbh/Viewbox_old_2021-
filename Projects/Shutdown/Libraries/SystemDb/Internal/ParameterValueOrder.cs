using DbAccess.Attributes;

namespace SystemDb.Internal
{
	[DbTable("user_issue_storedprocedure_order_settings", ForceInnoDb = true)]
	internal class ParameterValueOrder : IParameterValueOrder
	{
		[DbColumn("id")]
		[DbPrimaryKey]
		public int Id { get; set; }

		[DbColumn("user_id")]
		public int UserId { get; set; }

		[DbColumn("parameter_id")]
		public int ParameterId { get; set; }

		[DbColumn("collection_id")]
		public int CollectionId { get; set; }

		[DbColumn("collections_ids", Length = 1000)]
		public string CollectionsIds { get; set; }
	}
}
