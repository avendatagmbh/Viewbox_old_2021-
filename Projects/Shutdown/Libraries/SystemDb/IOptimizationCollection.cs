using System.Collections;
using System.Collections.Generic;

namespace SystemDb
{
	public interface IOptimizationCollection : IEnumerable<IOptimization>, IEnumerable
	{
		int Count { get; }

		IOptimization this[int id] { get; }

		int HighestLevel { get; }
	}
}
