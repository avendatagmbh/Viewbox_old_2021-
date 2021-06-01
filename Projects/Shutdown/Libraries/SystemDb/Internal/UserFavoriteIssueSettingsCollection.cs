using System.Collections;
using System.Collections.Generic;

namespace SystemDb.Internal
{
	internal class UserFavoriteIssueSettingsCollection : IUserFavoriteIssueSettingsCollection, IEnumerable<IUserFavoriteIssueSettings>, IEnumerable
	{
		private readonly Dictionary<int, IUserFavoriteIssueSettings> _dic = new Dictionary<int, IUserFavoriteIssueSettings>();

		public IUserFavoriteIssueSettings this[IUser user]
		{
			get
			{
				if (!_dic.ContainsKey(user.Id))
				{
					return null;
				}
				return _dic[user.Id];
			}
		}

		public IEnumerator<IUserFavoriteIssueSettings> GetEnumerator()
		{
			return _dic.Values.GetEnumerator();
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}

		public void Add(IUserFavoriteIssueSettings settings)
		{
			_dic[settings.User.Id] = settings;
		}

		public void Remove(IUserFavoriteIssueSettings settings)
		{
			_dic.Remove(settings.User.Id);
		}
	}
}
