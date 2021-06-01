using System;
using System.Collections;
using System.Collections.Generic;

namespace SystemDb
{
	public interface IRoleCategoryRights : IEnumerable<Tuple<IRole, ICategory, RightType>>, IEnumerable
	{
		RightType this[IRole role, ICategory cat] { get; }
	}
}
