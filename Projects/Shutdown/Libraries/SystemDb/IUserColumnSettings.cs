namespace SystemDb
{
	public interface IUserColumnSettings
	{
		int Id { get; }

		IUser User { get; }

		IColumn Column { get; }

		bool IsVisible { get; }
	}
}
