using System.Collections;
using System.Collections.Generic;

namespace SystemDb
{
	public interface IUserParameterGroupSettingsCollection : IEnumerable<IUserParameterGroupSettings>, IEnumerable
	{
		IEnumerable<IUserParameterGroupSettings> this[IUser user, IIssue issue] { get; }
	}
}
