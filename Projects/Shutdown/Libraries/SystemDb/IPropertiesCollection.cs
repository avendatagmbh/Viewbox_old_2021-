using System.Collections;
using System.Collections.Generic;

namespace SystemDb
{
	public interface IPropertiesCollection : IEnumerable<IProperty>, IEnumerable
	{
		int Count { get; }

		IProperty this[int id] { get; }
	}
}
