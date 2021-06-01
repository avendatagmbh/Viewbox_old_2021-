using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace SystemDb.Internal
{
	internal class UserTableTransactionIdSettingsCollection : IUserTableTransactionIdSettingsCollection, IEnumerable<IUserTableTransactionIdSettings>, IEnumerable
	{
		private readonly Dictionary<Tuple<int, int>, IUserTableTransactionIdSettings> _dic = new Dictionary<Tuple<int, int>, IUserTableTransactionIdSettings>();

		public IUserTableTransactionIdSettings this[int tableId, int userId]
		{
			get
			{
				Tuple<int, int> key = new Tuple<int, int>(tableId, userId);
				if (!_dic.ContainsKey(key))
				{
					return null;
				}
				return _dic[key];
			}
		}

		public IEnumerable<UserTableTransactionIdSettings> this[IUser user] => new List<UserTableTransactionIdSettings>(_dic.Where(delegate(KeyValuePair<Tuple<int, int>, IUserTableTransactionIdSettings> kv)
		{
			KeyValuePair<Tuple<int, int>, IUserTableTransactionIdSettings> keyValuePair2 = kv;
			return keyValuePair2.Key.Item2 == user.Id;
		}).Select(delegate(KeyValuePair<Tuple<int, int>, IUserTableTransactionIdSettings> kv)
		{
			KeyValuePair<Tuple<int, int>, IUserTableTransactionIdSettings> keyValuePair = kv;
			return keyValuePair.Value as UserTableTransactionIdSettings;
		}));

		public IUserTableTransactionIdSettings this[ITableObject table, IUser user] => this[table.Id, user.Id];

		public IEnumerator<IUserTableTransactionIdSettings> GetEnumerator()
		{
			return _dic.Values.GetEnumerator();
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}

		public void Add(IUserTableTransactionIdSettings settings)
		{
			_dic[Tuple.Create(settings.Table.Id, settings.User.Id)] = settings;
		}

		public void Remove(IUserTableTransactionIdSettings settings)
		{
			_dic.Remove(Tuple.Create(settings.User.Id, settings.Table.Id));
		}
	}
}
