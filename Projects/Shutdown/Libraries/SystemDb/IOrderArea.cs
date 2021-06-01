namespace SystemDb
{
	public interface IOrderArea
	{
		int Id { get; }

		int TableId { get; }

		string LanguageValue { get; }

		string IndexValue { get; }

		string SplitValue { get; }

		string SortValue { get; }

		long Start { get; }

		long End { get; }
	}
}
