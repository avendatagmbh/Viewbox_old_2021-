using System.Collections.Generic;
using System.Linq;

namespace SystemDb.Internal
{
	internal class TableCrudCollection : Dictionary<int, List<ITableCrud>>, ITableCrudCollection
	{
		public List<ITableCrud> this[ITableObject tableObject]
		{
			get
			{
				if (!ContainsKey(tableObject.Id))
				{
					return null;
				}
				return base[tableObject.Id];
			}
		}

		public ITableCrud GetCrud(int tableId, int id)
		{
			if (ContainsKey(tableId))
			{
				return base[tableId].FirstOrDefault((ITableCrud w) => w.Id == id);
			}
			return null;
		}

		public ITableCrud GetCrudForTable(int tableId)
		{
			return base.Values.FirstOrDefault((List<ITableCrud> w) => w.Any((ITableCrud v) => v.Table.Id == tableId))?.FirstOrDefault((ITableCrud w) => w.Table.Id == tableId);
		}

		public bool ContainsTable(ITableObject tableObject)
		{
			return ContainsKey(tableObject.Id);
		}

		public bool ContainsTable(int id)
		{
			return ContainsKey(id);
		}

		public void Add(ITableCrud tableCrud)
		{
			if (ContainsKey(tableCrud.OnTable.Id))
			{
				base[tableCrud.OnTable.Id].Add(tableCrud);
				return;
			}
			Add(tableCrud.OnTable.Id, new List<ITableCrud> { tableCrud });
		}
	}
}
