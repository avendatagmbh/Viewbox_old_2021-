using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace SystemDb.Internal
{
	internal class UserTableObjectOrderSettingsCollection : IUserTableObjectOrderSettingsCollection, IEnumerable<IUserTableObjectOrderSettings>, IEnumerable
	{
		private readonly Dictionary<Tuple<int, TableType>, IUserTableObjectOrderSettings> _dic = new Dictionary<Tuple<int, TableType>, IUserTableObjectOrderSettings>();

		public IUserTableObjectOrderSettings this[int user, TableType type]
		{
			get
			{
				Tuple<int, TableType> key = Tuple.Create(user, type);
				if (!_dic.ContainsKey(key))
				{
					return null;
				}
				return _dic[key];
			}
		}

		public IEnumerable<UserTableObjectOrderSettings> this[IUser user] => new List<UserTableObjectOrderSettings>(_dic.Where(delegate(KeyValuePair<Tuple<int, TableType>, IUserTableObjectOrderSettings> kv)
		{
			KeyValuePair<Tuple<int, TableType>, IUserTableObjectOrderSettings> keyValuePair2 = kv;
			return keyValuePair2.Key.Item1 == user.Id;
		}).Select(delegate(KeyValuePair<Tuple<int, TableType>, IUserTableObjectOrderSettings> kv)
		{
			KeyValuePair<Tuple<int, TableType>, IUserTableObjectOrderSettings> keyValuePair = kv;
			return keyValuePair.Value as UserTableObjectOrderSettings;
		}));

		public IUserTableObjectOrderSettings this[IUser user, TableType type] => this[user.Id, type];

		public IEnumerator<IUserTableObjectOrderSettings> GetEnumerator()
		{
			return _dic.Values.GetEnumerator();
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}

		public void Add(UserTableObjectOrderSettings settings)
		{
			_dic[Tuple.Create(settings.User.Id, settings.Type)] = settings;
		}

		public void Remove(IUserTableObjectOrderSettings settings)
		{
			_dic.Remove(Tuple.Create(settings.User.Id, settings.Type));
		}
	}
}
