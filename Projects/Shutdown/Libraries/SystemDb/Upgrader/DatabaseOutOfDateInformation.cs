namespace SystemDb.Upgrader
{
	public class DatabaseOutOfDateInformation : DatabaseBaseOutOfDateInformation
	{
		public string InstalledDbVersion { get; set; }

		public string CurrentDbVersion { get; set; }

		public DatabaseOutOfDateInformation(string installed)
		{
			InstalledDbVersion = installed;
			CurrentDbVersion = VersionInfo.CurrentDbVersion;
		}
	}
}
