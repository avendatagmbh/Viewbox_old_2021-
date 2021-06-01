using System.Collections;
using System.Collections.Generic;

namespace SystemDb.Internal
{
	public interface IFullHistoryParameterValueCollection : IEnumerable<IHistoryParameterValue>, IEnumerable
	{
		int Count { get; }

		IHistoryParameterValue this[int id] { get; }
	}
}
