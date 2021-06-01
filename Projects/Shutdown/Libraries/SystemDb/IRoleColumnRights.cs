using System;
using System.Collections;
using System.Collections.Generic;

namespace SystemDb
{
	public interface IRoleColumnRights : IEnumerable<Tuple<IRole, IColumn, RightType>>, IEnumerable
	{
		RightType this[IRole role, IColumn col] { get; }
	}
}
