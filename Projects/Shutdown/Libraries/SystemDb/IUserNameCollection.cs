using System.Collections;
using System.Collections.Generic;

namespace SystemDb
{
	public interface IUserNameCollection : IEnumerable<IUser>, IEnumerable
	{
		int Count { get; }

		IUser this[string userName] { get; }

		void Add(IUser user);
	}
}
