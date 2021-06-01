using System.Collections;
using System.Collections.Generic;

namespace SystemDb.Internal
{
	internal class UserIdCollection : Dictionary<int, IUser>, IUserIdCollection, IEnumerable<IUser>, IEnumerable
	{
		public new IUser this[int id]
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

		public void Add(IUser user)
		{
			if (!ContainsKey(user.Id))
			{
				Add(user.Id, user);
				(user as User).UserRemoved += delegate(IUser u)
				{
					Remove(u.Id);
				};
			}
		}

		public void Remove(IUser user)
		{
			Remove(user.Id);
		}

		public new IEnumerator<IUser> GetEnumerator()
		{
			return base.Values.GetEnumerator();
		}
	}
}
