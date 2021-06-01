using System;

namespace SystemDb
{
	public interface ICategory : ICloneable
	{
		int Id { get; }

		ILocalizedTextCollection Names { get; }

		ITableObjectCollection TableObjects { get; }

		int Ordinal { get; }

		void SetName(string name, ILanguage language);
	}
}
