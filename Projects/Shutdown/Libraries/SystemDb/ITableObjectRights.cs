using System;
using System.Collections;
using System.Collections.Generic;

namespace SystemDb
{
	public interface ITableObjectRights : IEnumerable<Tuple<ITableObject, RightType>>, IEnumerable
	{
		RightType this[ITableObject obj] { get; }
	}
}
