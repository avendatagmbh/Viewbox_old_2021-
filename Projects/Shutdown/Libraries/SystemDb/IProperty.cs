using System;

namespace SystemDb
{
	public interface IProperty : ICloneable
	{
		int Id { get; }

		string Key { get; }

		string Value { get; set; }

		ILocalizedTextCollection Names { get; }

		ILocalizedTextCollection Descriptions { get; }

		PropertyType Type { get; }
	}
}
