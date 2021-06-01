using System.Collections;
using System.Collections.Generic;

namespace SystemDb.Internal
{
	internal class FullColumnCollection : Dictionary<int, IColumn>, IFullColumnCollection, IEnumerable<IColumn>, IEnumerable
	{
		public new IColumn this[int id]
		{
			get
			{
				if (!ContainsKey(id))
				{
					return null;
				}
				return base[id];
			}
		}

		public void Add(IColumn column)
		{
			Add(column.Id, column);
		}

		public new IEnumerator<IColumn> GetEnumerator()
		{
			return base.Values.GetEnumerator();
		}
	}
}
