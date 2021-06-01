using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace SystemDb.Internal
{
	internal class UserLastIssueSettingsCollection : IUserLastIssueSettingsCollection, IEnumerable<IUserLastIssueSettings>, IEnumerable
	{
		private readonly ConcurrentDictionary<int, IUserLastIssueSettings> _dic = new ConcurrentDictionary<int, IUserLastIssueSettings>();

		public IUserLastIssueSettings this[IUser user]
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

		public IEnumerator<IUserLastIssueSettings> GetEnumerator()
		{
			return _dic.Values.GetEnumerator();
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}

		public void Add(IUserLastIssueSettings settings)
		{
			if (settings != null && !_dic.TryAdd(settings.User.Id, settings))
			{
				IUserLastIssueSettings history = _dic[settings.User.Id];
				_dic.TryUpdate(settings.User.Id, settings, history);
			}
		}

		public void Remove(IUserLastIssueSettings settings)
		{
			if (settings != null)
			{
				_dic.TryRemove(settings.User.Id, out var _);
			}
		}
	}
}
