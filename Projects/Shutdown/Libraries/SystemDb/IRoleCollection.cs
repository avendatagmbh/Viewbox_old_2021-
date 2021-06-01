using System.Collections;
using System.Collections.Generic;

namespace SystemDb
{
	public interface IRoleCollection : IEnumerable<IRole>, IEnumerable
	{
		int Count { get; }

		IRole this[int id] { get; }

		void Add(IRole role);

		void Remove(IRole role);
	}
}
