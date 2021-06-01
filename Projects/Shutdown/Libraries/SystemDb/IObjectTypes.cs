using System;

namespace SystemDb
{
	public interface IObjectTypes : ICloneable
	{
		int Id { get; }

		string Value { get; }
	}
}
