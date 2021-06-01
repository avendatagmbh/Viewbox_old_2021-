using System.Collections;
using System.Collections.Generic;

namespace SystemDb.Internal
{
	internal class UserNameCollection : Dictionary<string, IUser>, IUserNameCollection, IEnumerable<IUser>, IEnumerable
	{
		public new IUser this[string userName]
		{
			get
			{
				if (!ContainsKey(userName.ToLower()))
				{
					return null;
				}
				return base[userName.ToLower()];
			}
		}

		public void Add(IUser user)
		{
			Add(user.UserName, user);
			(user as User).UserRemoved += delegate(IUser u)
			{
				Remove(u.UserName);
			};
			(user as User).UserChanged += delegate(IUser u, string p, object o)
			{
				if (p == "UserName")
				{
					Remove(o as string);
					Add(u.UserName, u);
				}
			};
		}

		public new IEnumerator<IUser> GetEnumerator()
		{
			return base.Values.GetEnumerator();
		}
	}
}
