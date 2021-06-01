using System;
using DbAccess.Attributes;

namespace SystemDb.Internal
{
	[DbTable("server_log")]
	public class ServerLog
	{
		[DbColumn("id")]
		[DbPrimaryKey]
		public int Id { get; set; }

		[DbColumn("datetime")]
		public DateTime DateTime { get; set; }

		[DbColumn("comment")]
		public string Comment { get; set; }
	}
}
