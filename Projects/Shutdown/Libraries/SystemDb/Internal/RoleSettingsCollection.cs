using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace SystemDb.Internal
{
	public class RoleSettingsCollection : List<IRoleSetting>, IRoleSettingsCollection, IEnumerable<IRoleSetting>, IEnumerable
	{
		private readonly IRole _role;

		public string this[int roleId, RoleSettingsType settingsType]
		{
			get
			{
				return (from s in this
					where s.RoleId == roleId && s.Name == settingsType.ToString()
					select s.Value).FirstOrDefault();
			}
			set
			{
				IRoleSetting setting = this.Where((IRoleSetting s) => s.RoleId == roleId && s.Name == settingsType.ToString()).FirstOrDefault();
				if (setting != null)
				{
					setting.Value = value;
				}
			}
		}

		public IRoleSettingsCollection this[IRole role] => new RoleSettingsCollection(role, this.Where((IRoleSetting s) => s.RoleId == role.Id));

		public string this[RoleSettingsType settingsType]
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

		public string this[RoleSettingsType settingsType, int key]
		{
			get
			{
				return this.FirstOrDefault((IRoleSetting s) => s.Name == settingsType.ToString() && s.RoleId == key).Value;
			}
			set
			{
				if (_role == null)
				{
					throw new InvalidOperationException("this property indexer can only be used after the instanciation of RoleSettingsCollection with the IRole parameter");
				}
				IRoleSetting setting = this.FirstOrDefault((IRoleSetting s) => s.Name == settingsType.ToString() && s.RoleId == key);
				if (setting != null)
				{
					setting.Value = value;
				}
				else
				{
					setting = new RoleSetting
					{
						Name = settingsType.ToString(),
						RoleId = _role.Id,
						Value = value
					};
				}
				if (this.SettingModifiedEvent != null)
				{
					this.SettingModifiedEvent(_role, settingsType, value);
				}
			}
		}

		public event Action<IRole, RoleSettingsType, string> SettingModifiedEvent;

		public event Func<IRole, RoleSettingsType, bool> SettingRemovedEvent;

		public RoleSettingsCollection()
		{
		}

		public RoleSettingsCollection(IEnumerable<IRoleSetting> collection)
			: base(collection)
		{
		}

		public RoleSettingsCollection(IRole role, IRoleSettingsCollection collection)
			: base((IEnumerable<IRoleSetting>)collection)
		{
			_role = role;
		}

		public RoleSettingsCollection(IRole role, IEnumerable<IRoleSetting> collection)
			: base(collection)
		{
			_role = role;
		}

		public void Add(int roleId, RoleSettingsType roleSettingsType, string value)
		{
			Add(new RoleSetting
			{
				Name = roleSettingsType.ToString(),
				RoleId = roleId,
				Value = value
			});
		}

		public IRoleSetting Get(RoleSettingsType settingsType, int roleId)
		{
			return this.FirstOrDefault((IRoleSetting s) => s.Name == settingsType.ToString() && s.RoleId == roleId);
		}

		public bool Remove(RoleSettingsType settingsType, int key)
		{
			if (_role == null)
			{
				throw new InvalidOperationException("Remove can only be used after the instanciation of RoleSettingsCollection with the IRole parameter");
			}
			IRoleSetting setting = this.FirstOrDefault((IRoleSetting s) => s.Name == settingsType.ToString() && s.RoleId == key);
			if (setting != null && this.SettingRemovedEvent != null && this.SettingRemovedEvent(_role, settingsType))
			{
				Remove(setting);
			}
			return false;
		}
	}
}
