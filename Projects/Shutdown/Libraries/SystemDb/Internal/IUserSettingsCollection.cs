using System;
using System.Collections;
using System.Collections.Generic;

namespace SystemDb.Internal
{
	public interface IUserSettingsCollection : IEnumerable<IUserSetting>, IEnumerable
	{
		IUserSettingsCollection this[IUser user] { get; }

		string this[UserSettingsType settingsType] { get; set; }

		string this[UserSettingsType settingsType, int key] { get; set; }

		event Action<IUser, string, string> SettingModifiedEvent;

		event Func<IUser, string, bool> SettingRemovedEvent;

		void Add(int userId, UserSettingsType userSettingsType, string value);

		void Add(IUserSetting item);

		bool Remove(IUserSetting item);

		bool Remove(UserSettingsType settingsType, int key);

		IUserSetting Get(int userId, string settingName);
	}
}
