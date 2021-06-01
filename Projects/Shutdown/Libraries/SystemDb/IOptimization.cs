using System;

namespace SystemDb
{
	public interface IOptimization : ICloneable
	{
		int Id { get; }

		string Value { get; }

		IOptimization Parent { get; }

		IOptimizationCollection Children { get; }

		ILocalizedTextCollection Descriptions { get; }

		IOptimizationGroup Group { get; }

		int Level { get; }

		string FindValue(OptimizationType type);
	}
}
