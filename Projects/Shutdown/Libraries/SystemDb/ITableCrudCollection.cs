using System.Collections.Generic;

namespace SystemDb
{
	public interface ITableCrudCollection
	{
		int Count { get; }

		List<ITableCrud> this[ITableObject tableObject] { get; }

		ITableCrud GetCrudForTable(int tableId);

		ITableCrud GetCrud(int tableId, int id);

		bool ContainsTable(ITableObject tableObject);

		bool ContainsTable(int id);
	}
}
