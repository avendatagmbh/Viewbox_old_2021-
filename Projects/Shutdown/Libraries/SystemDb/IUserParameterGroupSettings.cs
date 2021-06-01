namespace SystemDb
{
	public interface IUserParameterGroupSettings
	{
		int Id { get; }

		IUser User { get; }

		IIssue Issue { get; }

		string ParameterGroup { get; }
	}
}
