using System;
using System.Collections;
using System.Collections.Generic;

namespace SystemDb.Internal
{
	internal class UserLogRights : CredentialRightDictionary<IUser, IUser>, IUserLogRights, IEnumerable<Tuple<IUser, IUser, RightType>>, IEnumerable
	{
		public UserLogUserMapping Mapping(IUser userLog, IUser user)
		{
			return new UserLogUserMapping
			{
				Id = GetMappingId(userLog, user),
				UserLogId = userLog.Id,
				UserId = user.Id,
				Right = base[userLog, user]
			};
		}
	}
}
