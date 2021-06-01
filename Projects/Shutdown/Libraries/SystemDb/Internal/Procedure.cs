using DbAccess.Attributes;

namespace SystemDb.Internal
{
	[DbTable("procedure", ForceInnoDb = true)]
	internal class Procedure
	{
		[DbColumn("table_id")]
		public int IssueId { get; set; }

		public IIssue Issue { get; set; }

		[DbColumn("command")]
		public string Command { get; set; }
	}
}
