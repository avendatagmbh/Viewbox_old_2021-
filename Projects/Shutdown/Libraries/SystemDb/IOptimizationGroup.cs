namespace SystemDb
{
	public interface IOptimizationGroup
	{
		int Id { get; }

		ILocalizedTextCollection Names { get; }

		OptimizationType Type { get; }
	}
}
