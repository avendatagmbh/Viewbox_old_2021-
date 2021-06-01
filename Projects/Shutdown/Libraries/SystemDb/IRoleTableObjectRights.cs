using System;
using System.Collections;
using System.Collections.Generic;

namespace SystemDb
{
	public interface IRoleTableObjectRights : IEnumerable<Tuple<IRole, ITableObject, RightType>>, IEnumerable
	{
		RightType this[IRole role, ITableObject obj] { get; set; }
	}
}
