using System.Collections;
using System.Collections.Generic;
using SystemDb.Internal;

namespace SystemDb
{
	public interface IPasswordCollection : IEnumerable<IPassword>, IEnumerable
	{
		void Insert(int index, IPassword pwd);

		void Remove(Password oldestPwd);
	}
}
