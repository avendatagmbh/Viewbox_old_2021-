namespace SystemDb
{
	public interface IUserPropertySettings
	{
		int Id { get; }

		IUser User { get; }

		IProperty Property { get; }

		string Value { get; }
	}
}
