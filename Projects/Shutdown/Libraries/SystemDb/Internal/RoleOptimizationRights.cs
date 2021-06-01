using System;
using System.Collections;
using System.Collections.Generic;

namespace SystemDb.Internal
{
	internal class RoleOptimizationRights : CredentialRightDictionary<IRole, IOptimization>, IRoleOptimizationRights, IEnumerable<Tuple<IRole, IOptimization, RightType>>, IEnumerable
	{
		public OptimizationRoleMapping Mapping(IRole role, IOptimization opt)
		{
			return new OptimizationRoleMapping
			{
				Id = GetMappingId(role, opt),
				RoleId = role.Id,
				OptimizationId = opt.Id,
				Visible = (base[role, opt] == RightType.Read)
			};
		}
	}
}
