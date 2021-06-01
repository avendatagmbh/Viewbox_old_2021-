namespace SystemDb
{
	public interface IUserColumnOrderSettings
	{
		int Id { get; }

		IUser User { get; }

		ITableObject TableObject { get; }

		string ColumnOrder { get; }
	}
}
