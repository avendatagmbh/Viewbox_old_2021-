using System.Collections;
using System.Collections.Generic;

namespace SystemDb.Internal
{
	public interface IFullHistoryParameterValueFreeSelectionCollection : IEnumerable<IHistoryParameterValueFreeSelection>, IEnumerable
	{
		int Count { get; }

		IHistoryParameterValueFreeSelection this[int id] { get; }
	}
}
