using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace SystemDb.Internal
{
	internal class UserColumnSettingCollection : IUserColumnSettingCollection, IEnumerable<IUserColumnSettings>, IEnumerable
	{
		private readonly Dictionary<Tuple<int, int>, IUserColumnSettings> _dic = new Dictionary<Tuple<int, int>, IUserColumnSettings>();

		public IUserColumnSettings this[int user, int column]
		{
			get
			{
				Tuple<int, int> key = Tuple.Create(user, column);
				if (!_dic.ContainsKey(key))
				{
					return null;
				}
				return _dic[key];
			}
		}

		public IEnumerable<UserColumnSettings> this[IUser user] => new List<UserColumnSettings>(_dic.Where(delegate(KeyValuePair<Tuple<int, int>, IUserColumnSettings> kv)
		{
			KeyValuePair<Tuple<int, int>, IUserColumnSettings> keyValuePair2 = kv;
			return keyValuePair2.Key.Item1 == user.Id;
		}).Select(delegate(KeyValuePair<Tuple<int, int>, IUserColumnSettings> kv)
		{
			KeyValuePair<Tuple<int, int>, IUserColumnSettings> keyValuePair = kv;
			return keyValuePair.Value as UserColumnSettings;
		}));

		public IUserColumnSettings this[IUser user, IColumn column] => this[user.Id, column.Id];

		public IEnumerator<IUserColumnSettings> GetEnumerator()
		{
			return _dic.Values.GetEnumerator();
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}

		public void Add(UserColumnSettings settings)
		{
			_dic[Tuple.Create(settings.User.Id, settings.Column.Id)] = settings;
		}

		public void Remove(IUserColumnSettings settings)
		{
			_dic.Remove(Tuple.Create(settings.User.Id, settings.Column.Id));
		}
	}
}
