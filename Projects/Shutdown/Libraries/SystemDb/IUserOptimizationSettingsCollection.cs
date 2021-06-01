namespace SystemDb
{
	public interface IUserOptimizationSettingsCollection
	{
		IUserOptimizationSettings this[IUser user] { get; }
	}
}
