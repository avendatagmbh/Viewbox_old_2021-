using System.Collections;
using System.Collections.Generic;

namespace SystemDb
{
	public interface IFullColumnCollection : IEnumerable<IColumn>, IEnumerable
	{
		int Count { get; }

		IColumn this[int id] { get; }
	}
}
