namespace SystemDb
{
	public interface IArchiveSetting
	{
		int Id { get; }

		int ArchiveId { get; }

		string Key { get; }

		string Value { get; set; }

		int Ordinal { get; set; }
	}
}
