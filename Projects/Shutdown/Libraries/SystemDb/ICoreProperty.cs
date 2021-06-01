using System;

namespace SystemDb
{
	public interface ICoreProperty : ICloneable
	{
		int Id { get; }

		string Key { get; }

		string Value { get; set; }

		PropertyType Type { get; }
	}
}
