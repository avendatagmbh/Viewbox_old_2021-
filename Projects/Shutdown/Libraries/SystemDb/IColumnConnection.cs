namespace SystemDb
{
	public interface IColumnConnection
	{
		IColumn Source { get; }

		IDataObject Target { get; }

		int Operator { get; }

		int FullLine { get; }

		RelationType RelationType { get; }

		string ExtInfo { get; }

		string ColumnExtInfo { get; }

		int RelationId { get; set; }

		bool UserDefined { get; set; }
	}
}
