using System.Collections;
using System.Collections.Generic;

namespace SystemDb.Internal
{
	internal class UserCollection : List<IUser>, IUserIdCollection, IEnumerable<IUser>, IEnumerable
	{
		public UserCollection(IEnumerable<IUser> collection)
			: base(collection)
		{
		}

		public new void Remove(IUser user)
		{
			base.Remove(user);
		}
	}
}
