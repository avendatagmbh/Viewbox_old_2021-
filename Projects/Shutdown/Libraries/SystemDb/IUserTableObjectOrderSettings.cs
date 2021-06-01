namespace SystemDb
{
	public interface IUserTableObjectOrderSettings
	{
		int Id { get; }

		IUser User { get; }

		TableType Type { get; }

		string TableObjectOrder { get; }
	}
}
