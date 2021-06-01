using DbAccess.Attributes;

namespace SystemDb.Internal
{
	[DbTable("table_original_names", ForceInnoDb = true)]
	internal class TableOriginalName
	{
		[DbColumn("id")]
		[DbPrimaryKey]
		public int RefId { get; set; }

		[DbColumn("original_name", Length = 128, AllowDbNull = true)]
		public string OriginalName { get; set; }
	}
}
