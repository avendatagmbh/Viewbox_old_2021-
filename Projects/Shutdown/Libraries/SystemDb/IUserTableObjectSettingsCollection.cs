using System.Collections;
using System.Collections.Generic;

namespace SystemDb
{
	public interface IUserTableObjectSettingsCollection : IEnumerable<IUserTableObjectSettings>, IEnumerable
	{
		IUserTableObjectSettings this[IUser user, ITableObject tableObject] { get; }
	}
}
