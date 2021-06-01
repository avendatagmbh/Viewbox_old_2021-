namespace SystemDb
{
	public interface IUserTableObjectSettings
	{
		int Id { get; }

		IUser User { get; }

		ITableObject TableObject { get; }

		bool IsVisible { get; }
	}
}
