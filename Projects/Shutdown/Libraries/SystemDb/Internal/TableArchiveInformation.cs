using DbAccess.Attributes;

namespace SystemDb.Internal
{
	[DbTable("table_archive_information")]
	public class TableArchiveInformation : ITableArchiveInformation
	{
		[DbColumn("id")]
		[DbPrimaryKey]
		public int Id { get; set; }

		[DbColumn("table_id")]
		public int TableId { get; set; }

		[DbColumn("createstatement", Length = 100000)]
		public string CreateStatement { get; set; }
	}
}
