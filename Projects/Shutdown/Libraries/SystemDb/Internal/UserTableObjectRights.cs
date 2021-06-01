using System;
using System.Collections;
using System.Collections.Generic;

namespace SystemDb.Internal
{
	internal class UserTableObjectRights : CredentialRightDictionary<IUser, ITableObject>, IUserTableObjectRights, IEnumerable<Tuple<IUser, ITableObject, RightType>>, IEnumerable
	{
		public TableUserMapping Mapping(IUser user, ITableObject obj)
		{
			return new TableUserMapping
			{
				Id = GetMappingId(user, obj),
				UserId = user.Id,
				TableId = obj.Id,
				Right = base[user, obj]
			};
		}
	}
}
