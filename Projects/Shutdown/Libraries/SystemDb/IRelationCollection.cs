using System.Collections;
using System.Collections.Generic;

namespace SystemDb
{
	public interface IRelationCollection : IEnumerable<IRelation>, IEnumerable
	{
		IEnumerable<IRelation> this[int colid] { get; }

		IEnumerable<IRelation> this[IColumn column] { get; }

		IEnumerable<IRelation> this[string colName, string tableName, string databaseName] { get; }
	}
}
