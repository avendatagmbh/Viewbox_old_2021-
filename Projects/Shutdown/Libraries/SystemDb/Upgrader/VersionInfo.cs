using System;
using System.Collections.Generic;
using System.Linq;

namespace SystemDb.Upgrader
{
	public static class VersionInfo
	{
		private static readonly List<string> _dbVersionHistory = new List<string>
		{
			"1.1", "1.2", "1.3", "1.4", "1.5", "1.6", "1.7", "1.8", "1.9", "1.9.1",
			"1.9.2", "1.9.3", "1.9.4", "1.9.5", "1.9.6", "1.9.7", "1.9.8", "1.9.9", "2.0.0", "2.0.1",
			"2.0.2", "2.0.3", "2.0.4", "2.0.5", "2.0.6", "2.0.7"
		};

		public static string CurrentDbVersion => _dbVersionHistory.Last();

		public static List<string> DbVersionHistory => _dbVersionHistory;

		public static int VersionToInt(string version)
		{
			return Convert.ToInt32(version.Replace(".", ""));
		}

		public static bool ProgramVersionToOld(string dbVersion)
		{
			return VersionToInt(dbVersion) > VersionToInt(CurrentDbVersion);
		}

		public static bool NewerDbVersionExists(string version)
		{
			return VersionToInt(CurrentDbVersion) > VersionToInt(version);
		}
	}
}
