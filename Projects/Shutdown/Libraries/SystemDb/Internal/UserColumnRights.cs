using System;
using System.Collections;
using System.Collections.Generic;

namespace SystemDb.Internal
{
	internal class UserColumnRights : CredentialRightDictionary<IUser, IColumn>, IUserColumnRights, IEnumerable<Tuple<IUser, IColumn, RightType>>, IEnumerable
	{
		public ColumnUserMapping Mapping(IUser user, IColumn col)
		{
			return new ColumnUserMapping
			{
				Id = GetMappingId(user, col),
				UserId = user.Id,
				ColumnId = col.Id,
				Right = base[user, col]
			};
		}
	}
}
