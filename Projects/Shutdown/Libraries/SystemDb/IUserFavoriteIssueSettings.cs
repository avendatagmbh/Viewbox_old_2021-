namespace SystemDb
{
	public interface IUserFavoriteIssueSettings
	{
		int Id { get; }

		IUser User { get; }

		string FavoriteList { get; }
	}
}
