namespace SystemDb
{
	public interface ITableCrudColumn
	{
		int Id { get; }

		ITableCrud TableCrud { get; }

		IColumn Column { get; }

		IColumn FromColumn { get; }

		int CalculateType { get; }
	}
}
