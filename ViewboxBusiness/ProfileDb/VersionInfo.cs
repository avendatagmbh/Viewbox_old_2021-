using System;
using System.Collections.Generic;

namespace ViewboxBusiness.ProfileDb
{
	public class VersionInfo
	{
		private static VersionInfo _instance;

		public string CurrentDbVersion = "1.0.5";

		private readonly List<string> _dbVersionHistory = new List<string> { "1.0.1", "1.0.2", "1.0.3", "1.0.4", "1.0.5" };

		public static VersionInfo Instance => _instance ?? (_instance = new VersionInfo());

		public List<string> DbVersionHistory => _dbVersionHistory;

		private VersionInfo()
		{
		}

		public static double VersionToDouble(string version)
		{
			string[] array = version.Split('.');
			double result = 0.0;
			double pow = 1.0;
			string[] array2 = array;
			foreach (string number in array2)
			{
				result += Convert.ToDouble(number) * pow;
				pow /= Convert.ToDouble(number.Length * 10);
			}
			return result;
		}

		public bool NewerDbVersionExists(string version)
		{
			return VersionToDouble(DbVersionHistory[DbVersionHistory.Count - 1]) > VersionToDouble(version);
		}

		public string GetNextDbVersion(string version)
		{
			foreach (string t in DbVersionHistory)
			{
				if (VersionToDouble(t) > VersionToDouble(version))
				{
					return t;
				}
			}
			return "";
		}

		public string GetLastDbVersion(string version)
		{
			for (int i = DbVersionHistory.Count - 1; i >= 0; i--)
			{
				if (VersionToDouble(DbVersionHistory[i]) <= VersionToDouble(version))
				{
					return DbVersionHistory[i];
				}
			}
			return "";
		}

		public string GetPreviousDbVersion(string version)
		{
			for (int i = DbVersionHistory.Count - 1; i >= 0; i--)
			{
				if (VersionToDouble(DbVersionHistory[i]) < VersionToDouble(version))
				{
					return DbVersionHistory[i];
				}
			}
			return string.Empty;
		}
	}
}
