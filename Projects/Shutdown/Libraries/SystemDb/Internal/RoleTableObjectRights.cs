using System;
using System.Collections;
using System.Collections.Generic;

namespace SystemDb.Internal
{
	internal class RoleTableObjectRights : CredentialRightDictionary<IRole, ITableObject>, IRoleTableObjectRights, IEnumerable<Tuple<IRole, ITableObject, RightType>>, IEnumerable
	{
		public TableRoleMapping Mapping(IRole role, ITableObject obj)
		{
			return new TableRoleMapping
			{
				Id = GetMappingId(role, obj),
				RoleId = role.Id,
				TableId = obj.Id,
				Right = base[role, obj]
			};
		}
	}
}
