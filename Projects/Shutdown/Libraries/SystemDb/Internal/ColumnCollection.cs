using System.Collections;
using System.Collections.Generic;

namespace SystemDb.Internal
{
	internal class ColumnCollection : DataObjectCollection<Column, IColumn>, IColumnCollection, IDataObjectCollection<IColumn>, IEnumerable<IColumn>, IEnumerable
	{
	}
}
