using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace SystemDb.Internal
{
	internal class UserTableObjectSettingsCollection : IUserTableObjectSettingsCollection, IEnumerable<IUserTableObjectSettings>, IEnumerable
	{
		private readonly Dictionary<Tuple<int, int>, IUserTableObjectSettings> _dic = new Dictionary<Tuple<int, int>, IUserTableObjectSettings>();

		public IUserTableObjectSettings this[int user, int tableObject]
		{
			get
			{
				Tuple<int, int> key = Tuple.Create(user, tableObject);
				if (!_dic.ContainsKey(key))
				{
					return null;
				}
				return _dic[key];
			}
		}

		public IEnumerable<UserTableObjectSettings> this[IUser user] => new List<UserTableObjectSettings>(_dic.Where(delegate(KeyValuePair<Tuple<int, int>, IUserTableObjectSettings> kv)
		{
			KeyValuePair<Tuple<int, int>, IUserTableObjectSettings> keyValuePair2 = kv;
			return keyValuePair2.Key.Item1 == user.Id;
		}).Select(delegate(KeyValuePair<Tuple<int, int>, IUserTableObjectSettings> kv)
		{
			KeyValuePair<Tuple<int, int>, IUserTableObjectSettings> keyValuePair = kv;
			return keyValuePair.Value as UserTableObjectSettings;
		}));

		public IUserTableObjectSettings this[IUser user, ITableObject tableObject] => this[user.Id, tableObject.Id];

		public IEnumerator<IUserTableObjectSettings> GetEnumerator()
		{
			return _dic.Values.GetEnumerator();
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}

		public void Add(UserTableObjectSettings settings)
		{
			_dic[Tuple.Create(settings.User.Id, settings.TableObject.Id)] = settings;
		}

		public void Remove(IUserTableObjectSettings settings)
		{
			_dic.Remove(Tuple.Create(settings.User.Id, settings.TableObject.Id));
		}
	}
}
