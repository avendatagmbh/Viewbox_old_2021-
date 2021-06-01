using System.Collections.Generic;

namespace SystemDb
{
	internal interface IUserLogReaderCollection
	{
		List<IUser> Users { get; }

		void Add(IUser user);
	}
}
