using System.Collections;
using System.Collections.Generic;

namespace SystemDb
{
	public interface IUserLastIssueSettingsCollection : IEnumerable<IUserLastIssueSettings>, IEnumerable
	{
		IUserLastIssueSettings this[IUser user] { get; }

		void Remove(IUserLastIssueSettings settings);
	}
}
