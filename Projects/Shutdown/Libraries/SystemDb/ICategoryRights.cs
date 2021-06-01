using System;
using System.Collections;
using System.Collections.Generic;

namespace SystemDb
{
	public interface ICategoryRights : IEnumerable<Tuple<ICategory, RightType>>, IEnumerable
	{
		RightType this[ICategory cat] { get; }
	}
}
