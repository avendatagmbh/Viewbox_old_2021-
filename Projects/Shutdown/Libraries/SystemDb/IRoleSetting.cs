namespace SystemDb
{
	public interface IRoleSetting
	{
		int Id { get; set; }

		string Name { get; set; }

		string Value { get; set; }

		int RoleId { get; set; }
	}
}
