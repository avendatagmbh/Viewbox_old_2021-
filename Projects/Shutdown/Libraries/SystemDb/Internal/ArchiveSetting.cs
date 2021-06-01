using DbAccess.Attributes;

namespace SystemDb.Internal
{
	[DbTable("archive_settings", ForceInnoDb = true)]
	internal class ArchiveSetting : IArchiveSetting
	{
		[DbColumn("id")]
		[DbPrimaryKey]
		public int Id { get; set; }

		[DbColumn("key", Length = 128)]
		public string Key { get; set; }

		[DbColumn("value")]
		public string Value { get; set; }

		[DbColumn("table_id")]
		public int ArchiveId { get; set; }

		[DbColumn("ordinal")]
		public int Ordinal { get; set; }
	}
}
