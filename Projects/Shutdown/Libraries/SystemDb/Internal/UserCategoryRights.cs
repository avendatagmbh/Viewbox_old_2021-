using System;
using System.Collections;
using System.Collections.Generic;

namespace SystemDb.Internal
{
	internal class UserCategoryRights : CredentialRightDictionary<IUser, ICategory>, IUserCategoryRights, IEnumerable<Tuple<IUser, ICategory, RightType>>, IEnumerable
	{
		public CategoryUserMapping Mapping(IUser user, ICategory cat)
		{
			return new CategoryUserMapping
			{
				Id = GetMappingId(user, cat),
				UserId = user.Id,
				CategoryId = cat.Id,
				Right = base[user, cat]
			};
		}
	}
}
