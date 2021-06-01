using DbAccess.Attributes;

namespace SystemDb.Internal
{
	[DbTable("user_last_issue_settings", ForceInnoDb = true)]
	internal class UserLastIssueSettings : IUserLastIssueSettings
	{
		[DbColumn("user_id")]
		public int UserId { get; set; }

		[DbColumn("id")]
		[DbPrimaryKey]
		public int Id { get; set; }

		public IUser User { get; set; }

		[DbColumn("Last_issue_id")]
		public int LastUsedIssue { get; set; }

		public object Clone()
		{
			return new UserLastIssueSettings
			{
				Id = Id,
				LastUsedIssue = LastUsedIssue,
				User = User,
				UserId = UserId
			};
		}
	}
}
