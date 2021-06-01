using System;
using System.Collections.Generic;
using System.Linq;

namespace SystemDb.Internal
{
	internal class UserPropertySettingCollection : IUserPropertySettingCollection
	{
		private readonly Dictionary<Tuple<int, int>, IUserPropertySettings> _dic = new Dictionary<Tuple<int, int>, IUserPropertySettings>();

		public IUserPropertySettings this[int user, int property]
		{
			get
			{
				Tuple<int, int> key = Tuple.Create(user, property);
				if (!_dic.ContainsKey(key))
				{
					return null;
				}
				return _dic[key];
			}
		}

		public IEnumerable<UserPropertySettings> this[IUser user] => new List<UserPropertySettings>(_dic.Where(delegate(KeyValuePair<Tuple<int, int>, IUserPropertySettings> kv)
		{
			KeyValuePair<Tuple<int, int>, IUserPropertySettings> keyValuePair2 = kv;
			return keyValuePair2.Key.Item1 == user.Id;
		}).Select(delegate(KeyValuePair<Tuple<int, int>, IUserPropertySettings> kv)
		{
			KeyValuePair<Tuple<int, int>, IUserPropertySettings> keyValuePair = kv;
			return keyValuePair.Value as UserPropertySettings;
		}));

		public IEnumerable<UserPropertySettings> GetAllSettings => new List<UserPropertySettings>(_dic.Select(delegate(KeyValuePair<Tuple<int, int>, IUserPropertySettings> kv)
		{
			KeyValuePair<Tuple<int, int>, IUserPropertySettings> keyValuePair = kv;
			return keyValuePair.Value as UserPropertySettings;
		}));

		public IUserPropertySettings this[IUser user, IProperty property] => this[user.Id, property.Id];

		public void Add(UserPropertySettings settings)
		{
			if (settings.User != null)
			{
				_dic[Tuple.Create(settings.User.Id, settings.Property.Id)] = settings;
			}
		}

		public void Remove(UserPropertySettings settings)
		{
			_dic.Remove(Tuple.Create(settings.User.Id, settings.Property.Id));
		}

		public void Clear()
		{
			_dic.Clear();
		}
	}
}
