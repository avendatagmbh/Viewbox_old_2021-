using System.Collections;
using System.Collections.Generic;

namespace SystemDb
{
	public interface ITableCollection : IDataObjectCollection<ITable>, IEnumerable<ITable>, IEnumerable
	{
	}
}
