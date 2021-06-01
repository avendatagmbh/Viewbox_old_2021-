using System.Collections;
using System.Collections.Generic;

namespace SystemDb.Internal
{
	internal class RoleCollection : Dictionary<int, Role>, IRoleCollection, IEnumerable<IRole>, IEnumerable
	{
		public new IRole this[int id]
		{
			get
			{
				if (!ContainsKey(id))
				{
					return null;
				}
				return base[id];
			}
		}

		public void Add(IRole role)
		{
			if (!ContainsKey(role.Id))
			{
				Add(role.Id, role as Role);
			}
		}

		public void Remove(IRole role)
		{
			Remove(role.Id);
		}

		public new IEnumerator<IRole> GetEnumerator()
		{
			return base.Values.GetEnumerator();
		}
	}
}
