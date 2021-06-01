using System.Collections;
using System.Collections.Generic;

namespace SystemDb
{
	public interface ICategoryCollection : IEnumerable<ICategory>, IEnumerable
	{
		int Count { get; }

		ICategory this[int id] { get; }

		void Add(ICategory category);
	}
}
