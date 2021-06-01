using System;
using DbAccess.Attributes;

namespace SystemDb.Internal
{
	[DbTable("files", ForceInnoDb = true)]
	public class FileObject : IFileObject
	{
		[DbColumn("id")]
		public int Id { get; set; }

		[DbColumn("directory_id")]
		public int DirectoryId { get; set; }

		[DbColumn("file_name")]
		public string Name { get; set; }

		[DbColumn("file_size")]
		public int Size { get; set; }

		[DbColumn("file_date")]
		public DateTime Date { get; set; }
	}
}
