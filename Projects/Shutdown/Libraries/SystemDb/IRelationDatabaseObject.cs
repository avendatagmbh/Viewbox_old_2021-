namespace SystemDb
{
	public interface IRelationDatabaseObject
	{
		int Id { get; set; }

		int RelationId { get; set; }

		int ParentId { get; set; }

		int ChildId { get; set; }

		int Operator { get; set; }

		int FullLine { get; set; }

		int Type { get; set; }

		string ExtInfo { get; set; }

		string ColumnExtInfo { get; set; }

		bool UserDefined { get; set; }
	}
}
