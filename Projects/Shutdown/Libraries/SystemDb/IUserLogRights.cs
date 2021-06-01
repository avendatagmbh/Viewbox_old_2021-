using System;
using System.Collections;
using System.Collections.Generic;

namespace SystemDb
{
	public interface IUserLogRights : IEnumerable<Tuple<IUser, IUser, RightType>>, IEnumerable
	{
		RightType this[IUser userLog, IUser user] { get; }
	}
}
