using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace SystemDb.Internal
{
	internal class UserTableColumnWidthsSettingsCollection : IUserTableColumnWidthsSettingsCollection, IEnumerable<IUserTableColumnWidthsSettings>, IEnumerable
	{
		private readonly Dictionary<Tuple<int, int>, IUserTableColumnWidthsSettings> _dic = new Dictionary<Tuple<int, int>, IUserTableColumnWidthsSettings>();

		public IUserTableColumnWidthsSettings this[int user, int tableObject]
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

		public IEnumerable<UserTableColumnWidthsSettings> this[IUser user] => new List<UserTableColumnWidthsSettings>(_dic.Where(delegate(KeyValuePair<Tuple<int, int>, IUserTableColumnWidthsSettings> kv)
		{
			KeyValuePair<Tuple<int, int>, IUserTableColumnWidthsSettings> keyValuePair2 = kv;
			return keyValuePair2.Key.Item1 == user.Id;
		}).Select(delegate(KeyValuePair<Tuple<int, int>, IUserTableColumnWidthsSettings> kv)
		{
			KeyValuePair<Tuple<int, int>, IUserTableColumnWidthsSettings> keyValuePair = kv;
			return keyValuePair.Value as UserTableColumnWidthsSettings;
		}));

		public IUserTableColumnWidthsSettings this[IUser user, ITableObject tableObject] => this[user.Id, tableObject.Id];

		public IEnumerator<IUserTableColumnWidthsSettings> GetEnumerator()
		{
			return _dic.Values.GetEnumerator();
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}

		public void Add(IUserTableColumnWidthsSettings settings)
		{
			_dic[Tuple.Create(settings.User.Id, settings.TableObject.Id)] = settings;
		}

		public void Remove(IUserTableColumnWidthsSettings settings)
		{
			_dic.Remove(Tuple.Create(settings.User.Id, settings.TableObject.Id));
		}
	}
}
