namespace SystemDb
{
	public interface IUserControllerSettings
	{
		int Id { get; }

		IUser User { get; }

		string Controller { get; }
	}
}
