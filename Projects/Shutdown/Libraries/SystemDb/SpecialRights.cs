using System;

namespace SystemDb
{
	[Flags]
	public enum SpecialRights
	{
		None = 0x0,
		Super = 0x1,
		Grant = 0x2
	}
}
