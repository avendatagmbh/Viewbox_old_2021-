using System;

namespace SystemDb
{
	public interface IScheme : ICloneable
	{
		int Id { get; }

		string Partial { get; }

		ILocalizedTextCollection Descriptions { get; }

		void SetDescription(string description, ILanguage language);
	}
}
