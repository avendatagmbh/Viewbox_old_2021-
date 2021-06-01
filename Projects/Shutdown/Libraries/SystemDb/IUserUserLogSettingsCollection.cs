using System.Collections;
using System.Collections.Generic;

namespace SystemDb
{
	public interface IUserUserLogSettingsCollection : IEnumerable<IUserUserLogSettings>, IEnumerable
	{
		IUserUserLogSettings this[IUser user, int userLogId] { get; }
	}
}
