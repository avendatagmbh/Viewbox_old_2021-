using System;
using System.Collections;
using System.Collections.Generic;

namespace SystemDb
{
	public interface IUserCategoryRights : IEnumerable<Tuple<IUser, ICategory, RightType>>, IEnumerable
	{
		RightType this[IUser user, ICategory cat] { get; }
	}
}
