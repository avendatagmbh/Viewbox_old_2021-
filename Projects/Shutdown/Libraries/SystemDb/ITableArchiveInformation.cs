namespace SystemDb
{
	public interface ITableArchiveInformation
	{
		int TableId { get; }

		string CreateStatement { get; }
	}
}
