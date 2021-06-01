namespace SystemDb
{
	internal interface IEmptyDistinctColumn
	{
		int TableId { get; }

		string IndexValue { get; }

		string SplitValue { get; }

		string SortValue { get; }

		int ColumnId { get; }

		bool IsEmpty { get; }

		bool OneDistinct { get; }
	}
}
