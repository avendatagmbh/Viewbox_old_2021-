using System.Collections;
using System.Collections.Generic;

namespace SystemDb
{
	public interface IUserTableObjectOrderSettingsCollection : IEnumerable<IUserTableObjectOrderSettings>, IEnumerable
	{
		IUserTableObjectOrderSettings this[IUser user, TableType type] { get; }
	}
}
