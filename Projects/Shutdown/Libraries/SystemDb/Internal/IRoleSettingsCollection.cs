using System;
using System.Collections;
using System.Collections.Generic;

namespace SystemDb.Internal
{
	public interface IRoleSettingsCollection : IEnumerable<IRoleSetting>, IEnumerable
	{
		IRoleSettingsCollection this[IRole role] { get; }

		string this[RoleSettingsType settingsType] { get; set; }

		string this[RoleSettingsType settingsType, int key] { get; set; }

		event Action<IRole, RoleSettingsType, string> SettingModifiedEvent;

		event Func<IRole, RoleSettingsType, bool> SettingRemovedEvent;

		void Add(int roleId, RoleSettingsType userSettingsType, string value);

		void Add(IRoleSetting item);

		bool Remove(IRoleSetting item);

		bool Remove(RoleSettingsType settingsType, int key);

		IRoleSetting Get(RoleSettingsType settingsType, int roleId);
	}
}
