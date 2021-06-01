using System.Collections;
using System.Collections.Generic;

namespace SystemDb
{
	public interface IUserTableColumnWidthsSettingsCollection : IEnumerable<IUserTableColumnWidthsSettings>, IEnumerable
	{
		IUserTableColumnWidthsSettings this[IUser user, ITableObject tableObject] { get; }
	}
}
