using System.Collections;
using System.Collections.Generic;

namespace SystemDb
{
	public interface IUserFavoriteIssueSettingsCollection : IEnumerable<IUserFavoriteIssueSettings>, IEnumerable
	{
		IUserFavoriteIssueSettings this[IUser user] { get; }
	}
}
