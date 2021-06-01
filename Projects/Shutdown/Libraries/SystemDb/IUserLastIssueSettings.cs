namespace SystemDb
{
	public interface IUserLastIssueSettings
	{
		int Id { get; }

		IUser User { get; }

		int LastUsedIssue { get; }
	}
}
