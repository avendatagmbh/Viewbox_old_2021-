using DbAccess.Attributes;

namespace SystemDb.Internal
{
	[DbTable("file_systems", ForceInnoDb = true)]
	public class FileSys : IFileSys
	{
		[DbColumn("id")]
		public int Id { get; set; }

		[DbColumn("database")]
		public string Database { get; set; }
	}
}
