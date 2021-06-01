using System;
using System.Collections;
using System.Collections.Generic;

namespace SystemDb.Internal
{
	internal class ColumnRights : RightDictionary<IColumn>, IColumnRights, IEnumerable<Tuple<IColumn, RightType>>, IEnumerable
	{
	}
}
