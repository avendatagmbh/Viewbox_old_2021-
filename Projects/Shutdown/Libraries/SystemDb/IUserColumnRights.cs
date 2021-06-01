using System;
using System.Collections;
using System.Collections.Generic;

namespace SystemDb
{
	public interface IUserColumnRights : IEnumerable<Tuple<IUser, IColumn, RightType>>, IEnumerable
	{
		RightType this[IUser user, IColumn col] { get; }
	}
}
