using System;
using SystemDb.Helper;
using SystemDb.Internal;

namespace SystemDb
{
	public interface IUser : ICredential, ICloneable
	{
		string UserName { get; set; }

		string Email { get; set; }

		string Password { get; set; }

		bool FirstLogin { get; set; }

		DateTime PasswordCreationDate { get; set; }

		short PasswordTrials { get; set; }

		int DisplayRowCount { get; set; }

		IRoleCollection Roles { get; }

		IPasswordCollection PasswordHistory { get; set; }

		IUserSettingsCollection Settings { get; set; }

		bool IsSuper { get; }

		bool IsLogRead { get; set; }

		string SessionId { get; set; }

		bool CanGrant { get; }

		bool IsADUser { get; }

		bool AllowedExport { get; }

		string Domain { get; }

		string CurrentLanguage { get; set; }

		RightHelper UserRightHelper { get; set; }

		bool CheckPassword(string password);

		string UserTable(string systemName);
	}
}
