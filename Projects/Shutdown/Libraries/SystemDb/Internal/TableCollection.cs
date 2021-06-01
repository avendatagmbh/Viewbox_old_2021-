using System.Collections;
using System.Collections.Generic;

namespace SystemDb.Internal
{
	internal class TableCollection : DataObjectCollection<Table, ITable>, ITableCollection, IDataObjectCollection<ITable>, IEnumerable<ITable>, IEnumerable
	{
	}
}
