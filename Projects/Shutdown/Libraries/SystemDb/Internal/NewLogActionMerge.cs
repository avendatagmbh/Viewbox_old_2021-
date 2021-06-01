using System;
using DbAccess.Attributes;

namespace SystemDb.Internal
{
	[DbTable("new_log_action")]
	public class NewLogActionMerge : INewLogActionMerge
	{
		[DbColumn("id")]
		[DbPrimaryKey]
		public int Id { get; set; }

		[DbColumn("user_id")]
		public int UserId { get; set; }

		[DbColumn("timestamp")]
		public DateTime Timestamp { get; set; }

		[DbColumn("action_controller")]
		public int ActionController { get; set; }

		[DbColumn("query_string", Length = 10000)]
		public string QueryString { get; set; }

		[DbColumn("parent_id")]
		public int Parentid { get; set; }
	}
}
