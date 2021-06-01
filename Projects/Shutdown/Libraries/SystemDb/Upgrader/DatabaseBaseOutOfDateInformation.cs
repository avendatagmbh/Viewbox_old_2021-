namespace SystemDb.Upgrader
{
	public interface DatabaseBaseOutOfDateInformation
	{
		string InstalledDbVersion { get; set; }

		string CurrentDbVersion { get; set; }
	}
}
