using System;
using System.Collections;
using System.Collections.Generic;

namespace SystemDb.Internal
{
	internal class RoleColumnRights : CredentialRightDictionary<IRole, IColumn>, IRoleColumnRights, IEnumerable<Tuple<IRole, IColumn, RightType>>, IEnumerable
	{
		public ColumnRoleMapping Mapping(IRole role, IColumn col)
		{
			return new ColumnRoleMapping
			{
				Id = GetMappingId(role, col),
				RoleId = role.Id,
				ColumnId = col.Id,
				Right = base[role, col]
			};
		}
	}
}
