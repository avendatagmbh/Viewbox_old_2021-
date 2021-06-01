namespace SystemDb
{
	public interface IUserOptimizationSettings
	{
		int Id { get; }

		IUser User { get; }

		IOptimization Optimization { get; }
	}
}
