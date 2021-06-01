using DbAccess.Attributes;

namespace SystemDb.Internal
{
	[DbTable("table_schemes", ForceInnoDb = true)]
	internal class TableObjectSchemeMapping
	{
		[DbColumn("id")]
		[DbPrimaryKey]
		public int Id { get; set; }

		[DbColumn("table_id")]
		public int TableObjectId { get; set; }

		[DbColumn("scheme_id")]
		public int SchemeId { get; set; }
	}
}
