using System;
using System.Collections;
using System.Collections.Generic;

namespace SystemDb
{
	public interface IUserTableObjectRights : IEnumerable<Tuple<IUser, ITableObject, RightType>>, IEnumerable
	{
		RightType this[IUser user, ITableObject obj] { get; }
	}
}
