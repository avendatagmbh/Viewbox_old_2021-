namespace SystemDb
{
	public interface IUserUserLogSettings
	{
		int Id { get; }

		IUser User { get; }

		int UserLogId { get; }

		bool IsVisible { get; }
	}
}
