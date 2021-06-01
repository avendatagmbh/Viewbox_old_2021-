using System;
using System.Collections;
using System.Collections.Generic;

namespace SystemDb
{
	public interface IUserOptimizationRights : IEnumerable<Tuple<IUser, IOptimization, RightType>>, IEnumerable
	{
		RightType this[IUser user, IOptimization opt] { get; }
	}
}
