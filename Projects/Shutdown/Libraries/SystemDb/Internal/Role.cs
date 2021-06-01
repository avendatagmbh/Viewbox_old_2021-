using System;
using DbAccess.Attributes;

namespace SystemDb.Internal
{
	[DbTable("roles", ForceInnoDb = true)]
	public class Role : IRole, ICredential, ICloneable
	{
		private UserIdCollection _users = new UserIdCollection();

		private RoleSettingsCollection _roleSettings = new RoleSettingsCollection();

		[DbColumn("can_export")]
		public ExportRights CanExport { private get; set; }

		[DbColumn("role_id")]
		[DbPrimaryKey]
		public int Id { get; set; }

		[DbColumn("name", Length = 128)]
		public string Name { get; set; }

		public CredentialType Type => CredentialType.Role;

		[DbColumn("flags")]
		public SpecialRights Flags { get; set; }

		public bool AllowedExport => CanExport == ExportRights.Enabled;

		public IUserIdCollection Users => _users;

		public bool IsSuper => Flags.HasFlag(SpecialRights.Super);

		public bool CanGrant
		{
			get
			{
				if (!IsSuper)
				{
					return Flags.HasFlag(SpecialRights.Grant);
				}
				return true;
			}
		}

		public IRoleSettingsCollection Settings
		{
			get
			{
				return _roleSettings;
			}
			set
			{
				_roleSettings = (RoleSettingsCollection)value;
			}
		}

		public object Clone()
		{
			return new Role
			{
				Flags = Flags,
				Id = Id,
				Name = Name,
				_users = _users
			};
		}

		public override bool Equals(object obj)
		{
			if (obj == null)
			{
				return false;
			}
			ICredential p = obj as ICredential;
			if (p == null)
			{
				return false;
			}
			if (Type == p.Type)
			{
				return Id == p.Id;
			}
			return false;
		}

		public override int GetHashCode()
		{
			return Id;
		}
	}
}
