using System.Collections;
using System.Collections.Generic;

namespace SystemDb
{
	public interface ISchemeCollection : IEnumerable<IScheme>, IEnumerable
	{
		int Count { get; }

		IScheme this[int id] { get; }
	}
}
