using System;
using System.Collections;
using System.Collections.Generic;

namespace SystemDb.Internal
{
	internal class RoleCategoryRights : CredentialRightDictionary<IRole, ICategory>, IRoleCategoryRights, IEnumerable<Tuple<IRole, ICategory, RightType>>, IEnumerable
	{
		public CategoryRoleMapping Mapping(IRole role, ICategory cat)
		{
			return new CategoryRoleMapping
			{
				Id = GetMappingId(role, cat),
				RoleId = role.Id,
				CategoryId = cat.Id,
				Right = base[role, cat]
			};
		}
	}
}
