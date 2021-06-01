using System;
using System.Collections;
using System.Collections.Generic;

namespace SystemDb
{
	public interface IOptimizationRights : IEnumerable<Tuple<IOptimization, RightType>>, IEnumerable
	{
		RightType this[IOptimization opt] { get; }
	}
}
