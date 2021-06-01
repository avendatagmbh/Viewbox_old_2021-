using System;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using SystemDb.Helper;
using DbAccess.Attributes;

namespace SystemDb.Internal
{
	[DbTable("users", ForceInnoDb = true)]
	public class User : IUser, ICredential, ICloneable
	{
		public delegate void UserChangedHandler(IUser user, string property, object old_value);

		public delegate void UserRemovedHandler(IUser user);

		private string _password;

		private PasswordCollection _passwordHistory = new PasswordCollection();

		private RoleCollection _roles = new RoleCollection();

		private string _userName;

		private UserSettingsCollection _userSettings;

		[DbColumn("password", Length = 64)]
		public string PasswordHash { get; set; }

		[DbColumn("can_export")]
		public ExportRights CanExport { private get; set; }

		[DbColumn("id")]
		[DbPrimaryKey]
		public int Id { get; set; }

		[DbColumn("user_name", Length = 128)]
		[DbUniqueKey]
		public string UserName
		{
			get
			{
				return _userName;
			}
			set
			{
				if (_userName != value.ToLower())
				{
					string old = _userName;
					_userName = value.ToLower();
					if (_password != null)
					{
						Password = Password;
					}
					if (this.UserChanged != null)
					{
						this.UserChanged(this, "UserName", old);
					}
				}
			}
		}

		[DbColumn("name", Length = 128)]
		public string Name { get; set; }

		[DbColumn("email", Length = 128)]
		public string Email { get; set; }

		[DbColumn("password_creation_date")]
		public DateTime PasswordCreationDate { get; set; }

		[DbColumn("password_trials")]
		public short PasswordTrials { get; set; }

		public string SessionId { get; set; }

		public string CurrentLanguage { get; set; }

		public string Password
		{
			get
			{
				if (_password == null)
				{
					throw new InvalidOperationException("Currently only hash is known");
				}
				return _password;
			}
			set
			{
				if (value == null)
				{
					throw new ArgumentNullException();
				}
				PasswordHash = ComputePasswordHash(value, UserName);
				_password = value;
			}
		}

		public CredentialType Type => CredentialType.User;

		[DbColumn("first_login")]
		public bool FirstLogin { get; set; }

		[DbColumn("flags")]
		public SpecialRights Flags { get; set; }

		public IRoleCollection Roles => _roles;

		public IPasswordCollection PasswordHistory
		{
			get
			{
				return _passwordHistory;
			}
			set
			{
				_passwordHistory = value as PasswordCollection;
			}
		}

		public IUserSettingsCollection Settings
		{
			get
			{
				return _userSettings;
			}
			set
			{
				_userSettings = (UserSettingsCollection)value;
			}
		}

		public bool AllowedExport
		{
			get
			{
				if (CanExport != ExportRights.Enabled)
				{
					return Roles.Any((IRole r) => r.AllowedExport);
				}
				return true;
			}
		}

		public bool IsSuper
		{
			get
			{
				if (!Flags.HasFlag(SpecialRights.Super))
				{
					return Roles.Any((IRole r) => r.Flags.HasFlag(SpecialRights.Super));
				}
				return true;
			}
		}

		public bool CanGrant
		{
			get
			{
				if (!IsSuper && !Flags.HasFlag(SpecialRights.Grant))
				{
					return Roles.Any((IRole r) => r.Flags.HasFlag(SpecialRights.Grant));
				}
				return true;
			}
		}

		public bool IsLogRead { get; set; }

		[DbColumn("is_ad_user")]
		public bool IsADUser { get; set; }

		[DbColumn("display_row_count")]
		public int DisplayRowCount { get; set; }

		[DbColumn("domain")]
		public string Domain { get; set; }

		public RightHelper UserRightHelper { get; set; }

		public event UserRemovedHandler UserRemoved;

		public event UserChangedHandler UserChanged;

		public string UserTable(string systemName)
		{
			return "viewbox_user_table_" + Id + "_" + systemName;
		}

		public bool CheckPassword(string password)
		{
			if (!string.IsNullOrEmpty(password) && PasswordHash == ComputePasswordHash(password, UserName))
			{
				_password = password;
				return true;
			}
			return false;
		}

		private string ComputePasswordHash(string password, string salt)
		{
			return BitConverter.ToString(new SHA1CryptoServiceProvider().ComputeHash(Encoding.ASCII.GetBytes(password + salt))).Replace("-", "");
		}

		public void Remove()
		{
			if (this.UserRemoved != null)
			{
				this.UserRemoved(this);
			}
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

		public object Clone()
		{
			return new User
			{
				UserName = UserName,
				Email = Email,
				Flags = Flags,
				Id = Id,
				Name = Name,
				Password = Password,
				PasswordHash = PasswordHash,
				_roles = _roles,
				IsADUser = IsADUser,
				CanExport = CanExport,
				DisplayRowCount = DisplayRowCount,
				Domain = Domain
			};
		}
	}
}
