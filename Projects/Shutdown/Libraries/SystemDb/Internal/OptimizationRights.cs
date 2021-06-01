using System;
using System.Collections;
using System.Collections.Generic;

namespace SystemDb.Internal
{
	internal class OptimizationRights : RightDictionary<IOptimization>, IOptimizationRights, IEnumerable<Tuple<IOptimization, RightType>>, IEnumerable
	{
	}
}
