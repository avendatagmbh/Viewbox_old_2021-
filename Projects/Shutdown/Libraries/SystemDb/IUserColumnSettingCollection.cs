using System.Collections;
using System.Collections.Generic;

namespace SystemDb
{
	public interface IUserColumnSettingCollection : IEnumerable<IUserColumnSettings>, IEnumerable
	{
		IUserColumnSettings this[IUser user, IColumn column] { get; }
	}
}
