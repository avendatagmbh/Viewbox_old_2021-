using System.Collections;
using System.Collections.Generic;

namespace SystemDb
{
	public interface ICorePropertiesCollection : IEnumerable<ICoreProperty>, IEnumerable
	{
		int Count { get; }

		ICoreProperty this[int id] { get; }
	}
}
