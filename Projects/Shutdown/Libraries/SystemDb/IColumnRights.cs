using System;
using System.Collections;
using System.Collections.Generic;

namespace SystemDb
{
	public interface IColumnRights : IEnumerable<Tuple<IColumn, RightType>>, IEnumerable
	{
		RightType this[IColumn col] { get; }
	}
}
