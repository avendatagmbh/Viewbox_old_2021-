using System.Collections.Generic;
using System.Linq;

namespace SystemDb.Internal
{
	internal class UserOptimizationSettingsCollection : IUserOptimizationSettingsCollection
	{
		private readonly Dictionary<int, IUserOptimizationSettings> _dic = new Dictionary<int, IUserOptimizationSettings>();

		public IUserOptimizationSettings this[int user]
		{
			get
			{
				if (!_dic.ContainsKey(user))
				{
					return null;
				}
				return _dic[user];
			}
		}

		public IUserOptimizationSettings this[IUser user] => this[user.Id];

		public void Add(UserOptimizationSettings settings)
		{
			if (settings != null && settings.User != null && !_dic.ContainsKey(settings.User.Id))
			{
				_dic[settings.User.Id] = settings;
			}
		}

		public void Remove(UserOptimizationSettings settings)
		{
			_dic.Remove(settings.User.Id);
		}

		public int Count()
		{
			return _dic.Count;
		}

		public UserOptimizationSettings GetElementAtSettings(int k)
		{
			if (k < 0)
			{
				return null;
			}
			if (k >= _dic.Count)
			{
				return null;
			}
			if (_dic.Values.Count >= k)
			{
				return (UserOptimizationSettings)_dic.Values.ElementAt(k);
			}
			return null;
		}
	}
}
