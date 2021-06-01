using System;
using System.Collections;
using System.Collections.Generic;

namespace SystemDb
{
	public interface IRoleOptimizationRights : IEnumerable<Tuple<IRole, IOptimization, RightType>>, IEnumerable
	{
		RightType this[IRole role, IOptimization opt] { get; set; }
	}
}
