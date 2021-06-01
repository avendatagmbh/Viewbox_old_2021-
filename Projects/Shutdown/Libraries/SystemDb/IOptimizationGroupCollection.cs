using System.Collections;
using System.Collections.Generic;

namespace SystemDb
{
	public interface IOptimizationGroupCollection : IEnumerable<IOptimizationGroup>, IEnumerable
	{
		int Count { get; }

		IOptimizationGroup this[int id] { get; }
	}
}
