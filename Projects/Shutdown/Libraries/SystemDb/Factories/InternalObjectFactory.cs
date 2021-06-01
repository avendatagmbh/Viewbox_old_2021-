using SystemDb.Internal;

namespace SystemDb.Factories
{
	public static class InternalObjectFactory
	{
		public static IRelationDatabaseObject CreateRelation(int sourceColumnId, int targetColumnId, int relationId)
		{
			return new Relation
			{
				ParentId = sourceColumnId,
				ChildId = targetColumnId,
				RelationId = relationId
			};
		}
	}
}
