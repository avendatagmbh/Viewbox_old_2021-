using System;
using System.Collections;
using System.Collections.Generic;

namespace SystemDb.Internal
{
	internal class CategoryRights : RightDictionary<ICategory>, ICategoryRights, IEnumerable<Tuple<ICategory, RightType>>, IEnumerable
	{
	}
}
