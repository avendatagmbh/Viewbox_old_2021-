using System.Collections;
using System.Collections.Generic;

namespace SystemDb
{
	public interface ILanguageCollection : IEnumerable<ILanguage>, IEnumerable
	{
		int Count { get; }

		ILanguage this[string countryCode] { get; }
	}
}
