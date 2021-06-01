using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace SystemDb.Internal
{
	internal class UserUserLogSettingsCollection : IUserUserLogSettingsCollection, IEnumerable<IUserUserLogSettings>, IEnumerable
	{
		private readonly Dictionary<Tuple<int, int>, IUserUserLogSettings> _dic = new Dictionary<Tuple<int, int>, IUserUserLogSettings>();

		public IUserUserLogSettings this[int user, int userLog]
		{
			get
			{
				Tuple<int, int> key = Tuple.Create(user, userLog);
				if (!_dic.ContainsKey(key))
				{
					return null;
				}
				return _dic[key];
			}
		}

		public IEnumerable<UserUserLogSettings> this[IUser user] => new List<UserUserLogSettings>(_dic.Where(delegate(KeyValuePair<Tuple<int, int>, IUserUserLogSettings> kv)
		{
			KeyValuePair<Tuple<int, int>, IUserUserLogSettings> keyValuePair2 = kv;
			return keyValuePair2.Key.Item1 == user.Id;
		}).Select(delegate(KeyValuePair<Tuple<int, int>, IUserUserLogSettings> kv)
		{
			KeyValuePair<Tuple<int, int>, IUserUserLogSettings> keyValuePair = kv;
			return keyValuePair.Value as UserUserLogSettings;
		}));

		public IUserUserLogSettings this[IUser user, int userLogId] => this[user.Id, userLogId];

		public IEnumerator<IUserUserLogSettings> GetEnumerator()
		{
			return _dic.Values.GetEnumerator();
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}

		public void Add(UserUserLogSettings settings)
		{
			_dic[Tuple.Create(settings.User.Id, settings.UserLogId)] = settings;
		}

		public void Remove(IUserUserLogSettings settings)
		{
			_dic.Remove(Tuple.Create(settings.User.Id, settings.UserLogId));
		}
	}
}
