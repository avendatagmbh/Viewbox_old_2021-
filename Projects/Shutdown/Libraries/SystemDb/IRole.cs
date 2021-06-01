using System;
using SystemDb.Internal;

namespace SystemDb
{
	public interface IRole : ICredential, ICloneable
	{
		IUserIdCollection Users { get; }

		bool IsSuper { get; }

		bool CanGrant { get; }

		bool AllowedExport { get; }

		IRoleSettingsCollection Settings { get; set; }
	}
}
