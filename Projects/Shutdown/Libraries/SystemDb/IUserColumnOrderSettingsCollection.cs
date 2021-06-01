using System.Collections;
using System.Collections.Generic;

namespace SystemDb
{
	public interface IUserColumnOrderSettingsCollection : IEnumerable<IUserColumnOrderSettings>, IEnumerable
	{
		IUserColumnOrderSettings this[IUser user, ITableObject tableObject] { get; }
	}
}
