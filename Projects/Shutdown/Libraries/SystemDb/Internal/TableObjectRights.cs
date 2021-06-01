using System;
using System.Collections;
using System.Collections.Generic;

namespace SystemDb.Internal
{
	internal class TableObjectRights : RightDictionary<ITableObject>, ITableObjectRights, IEnumerable<Tuple<ITableObject, RightType>>, IEnumerable
	{
	}
}
