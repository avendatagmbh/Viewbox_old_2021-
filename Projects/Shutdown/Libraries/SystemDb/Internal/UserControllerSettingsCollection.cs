using System.Collections.Concurrent;

namespace SystemDb.Internal
{
	internal class UserControllerSettingsCollection : IUserControllerSettingsCollection
	{
		private readonly ConcurrentDictionary<int, IUserControllerSettings> _dic = new ConcurrentDictionary<int, IUserControllerSettings>();

		public IUserControllerSettings this[int user]
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

		public IUserControllerSettings this[IUser user] => this[user.Id];

		public void Add(UserControllerSettings settings)
		{
			if (settings != null && !_dic.TryAdd(settings.User.Id, settings))
			{
				IUserControllerSettings history = _dic[settings.User.Id];
				_dic.TryUpdate(settings.User.Id, settings, history);
			}
		}

		public void Remove(UserControllerSettings settings)
		{
			if (settings != null)
			{
				_dic.TryRemove(settings.User.Id, out var _);
			}
		}
	}
}
