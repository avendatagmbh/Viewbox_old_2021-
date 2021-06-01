namespace SystemDb.Internal
{
	public interface IUserSetting
	{
		int Id { get; set; }

		string Name { get; set; }

		string Value { get; set; }

		int UserId { get; set; }
	}
}
