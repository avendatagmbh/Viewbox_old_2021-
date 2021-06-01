using System.Collections;
using System.Collections.Generic;

namespace SystemDb
{
	public interface IUserTableTransactionIdSettingsCollection : IEnumerable<IUserTableTransactionIdSettings>, IEnumerable
	{
		IUserTableTransactionIdSettings this[ITableObject table, IUser user] { get; }
	}
}
