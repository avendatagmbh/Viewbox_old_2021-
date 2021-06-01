using System.Collections;
using System.Collections.Generic;

namespace SystemDb
{
	public interface IColumnCollection : IDataObjectCollection<IColumn>, IEnumerable<IColumn>, IEnumerable
	{
		void Add(IColumn column);

		void RemoveById(int id);
	}
}
