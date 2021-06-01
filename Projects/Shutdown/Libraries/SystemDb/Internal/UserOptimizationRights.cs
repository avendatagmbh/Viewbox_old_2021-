using System;
using System.Collections;
using System.Collections.Generic;

namespace SystemDb.Internal
{
	internal class UserOptimizationRights : CredentialRightDictionary<IUser, IOptimization>, IUserOptimizationRights, IEnumerable<Tuple<IUser, IOptimization, RightType>>, IEnumerable
	{
		public OptimizationUserMapping Mapping(IUser user, IOptimization opt)
		{
			return new OptimizationUserMapping
			{
				Id = GetMappingId(user, opt),
				UserId = user.Id,
				OptimizationId = opt.Id,
				Visible = (base[user, opt] == RightType.Read)
			};
		}
	}
}
