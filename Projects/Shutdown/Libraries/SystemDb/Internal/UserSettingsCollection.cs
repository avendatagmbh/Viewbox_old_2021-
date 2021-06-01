using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace SystemDb.Internal
{
	public class UserSettingsCollection : List<IUserSetting>, IUserSettingsCollection, IEnumerable<IUserSetting>, IEnumerable
	{
		private readonly IUser _user;

		public string this[int userId, UserSettingsType settingsType]
		{
			get
			{
				return (from s in this
					where s.UserId == userId && s.Name == settingsType.ToString()
					select s.Value).FirstOrDefault();
			}
			set
			{
				IUserSetting setting = this.Where((IUserSetting s) => s.UserId == userId && s.Name == settingsType.ToString()).FirstOrDefault();
				if (setting != null)
				{
					setting.Value = value;
				}
			}
		}

		public IUserSettingsCollection this[IUser user] => new UserSettingsCollection(user, this.Where((IUserSetting s) => s.UserId == user.Id));

		public string this[UserSettingsType settingsType]
		{
			get
			{
				return this[settingsType, -1];
			}
			set
			{
				this[settingsType, -1] = value;
			}
		}

		public string this[UserSettingsType settingsType, int key]
		{
			get
			{
				string settingKey = ((key >= 0) ? (settingsType.ToString() + "_" + key) : settingsType.ToString());
				return (from s in this
					where s.Name == settingKey
					select s.Value).FirstOrDefault();
			}
			set
			{
				if (_user == null)
				{
					throw new InvalidOperationException("this property indexer can only be used after the instanciation of UserSettingsCollection with the IUser parameter");
				}
				string settingKey = ((key >= 0) ? (settingsType.ToString() + "_" + key) : settingsType.ToString());
				IUserSetting setting = this.Where((IUserSetting s) => s.Name == settingKey).FirstOrDefault();
				if (setting != null)
				{
					setting.Value = value;
				}
				else
				{
					setting = new UserSetting
					{
						Name = settingKey,
						UserId = _user.Id,
						Value = value
					};
				}
				if (this.SettingModifiedEvent != null)
				{
					this.SettingModifiedEvent(_user, settingKey, value);
				}
			}
		}

		public event Action<IUser, string, string> SettingModifiedEvent;

		public event Func<IUser, string, bool> SettingRemovedEvent;

		public UserSettingsCollection()
		{
		}

		public UserSettingsCollection(IEnumerable<IUserSetting> collection)
			: base(collection)
		{
		}

		public UserSettingsCollection(IUser user, IUserSettingsCollection collection)
			: base((IEnumerable<IUserSetting>)collection)
		{
			_user = user;
		}

		public UserSettingsCollection(IUser user, IEnumerable<IUserSetting> collection)
			: base(collection)
		{
			_user = user;
		}

		public void Add(int userId, UserSettingsType userSettingsType, string value)
		{
			Add(new UserSetting
			{
				Name = userSettingsType.ToString(),
				UserId = userId,
				Value = value
			});
		}

		public IUserSetting Get(int userId, string settingName)
		{
			return this.Where((IUserSetting s) => s.UserId == userId && s.Name == settingName).FirstOrDefault();
		}

		public bool Remove(UserSettingsType settingsType, int key)
		{
			if (_user == null)
			{
				throw new InvalidOperationException("Remove can only be used after the instanciation of UserSettingsCollection with the IUser parameter");
			}
			string settingKey = ((key >= 0) ? (settingsType.ToString() + "_" + key) : settingsType.ToString());
			IUserSetting setting = this.Where((IUserSetting s) => s.Name == settingKey).FirstOrDefault();
			if (setting != null && this.SettingRemovedEvent != null && this.SettingRemovedEvent(_user, settingKey))
			{
				Remove(setting);
			}
			return false;
		}
	}
}
