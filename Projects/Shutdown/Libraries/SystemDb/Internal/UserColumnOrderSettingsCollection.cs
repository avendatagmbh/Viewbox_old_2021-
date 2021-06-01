using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace SystemDb.Internal
{
	internal class UserColumnOrderSettingsCollection : IUserColumnOrderSettingsCollection, IEnumerable<IUserColumnOrderSettings>, IEnumerable
	{
		private readonly Dictionary<Tuple<int, int>, IUserColumnOrderSettings> _dic = new Dictionary<Tuple<int, int>, IUserColumnOrderSettings>();

		public IUserColumnOrderSettings this[int user, int tableObject]
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

		public IEnumerable<UserColumnOrderSettings> this[IUser user] => new List<UserColumnOrderSettings>(_dic.Where(delegate(KeyValuePair<Tuple<int, int>, IUserColumnOrderSettings> kv)
		{
			KeyValuePair<Tuple<int, int>, IUserColumnOrderSettings> keyValuePair2 = kv;
			return keyValuePair2.Key.Item1 == user.Id;
		}).Select(delegate(KeyValuePair<Tuple<int, int>, IUserColumnOrderSettings> kv)
		{
			KeyValuePair<Tuple<int, int>, IUserColumnOrderSettings> keyValuePair = kv;
			return keyValuePair.Value as UserColumnOrderSettings;
		}));

		public IUserColumnOrderSettings this[IUser user, ITableObject tableObject] => this[user.Id, tableObject.Id];

		public IEnumerator<IUserColumnOrderSettings> GetEnumerator()
		{
			return _dic.Values.GetEnumerator();
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}

		public void Add(UserColumnOrderSettings settings)
		{
			_dic[Tuple.Create(settings.User.Id, settings.TableObject.Id)] = settings;
		}

		public void Remove(IUserColumnOrderSettings settings)
		{
			if (settings != null)
			{
				_dic.Remove(Tuple.Create(settings.User.Id, settings.TableObject.Id));
			}
		}
	}
}
