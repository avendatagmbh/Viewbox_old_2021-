namespace SystemDb
{
	public interface ICredential
	{
		int Id { get; }

		string Name { get; set; }

		SpecialRights Flags { get; set; }

		CredentialType Type { get; }
	}
}
