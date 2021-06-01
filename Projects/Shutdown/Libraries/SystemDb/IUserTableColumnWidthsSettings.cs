namespace SystemDb
{
	public interface IUserTableColumnWidthsSettings
	{
		int Id { get; }

		IUser User { get; }

		ITableObject TableObject { get; }

		string ColumnWidths { get; }
	}
}
