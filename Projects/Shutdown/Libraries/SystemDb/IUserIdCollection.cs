using System.Collections;
using System.Collections.Generic;

namespace SystemDb
{
	public interface IUserIdCollection : IEnumerable<IUser>, IEnumerable
	{
		int Count { get; }

		IUser this[int id] { get; }

		void Add(IUser user);

		void Remove(IUser user);
	}
}
