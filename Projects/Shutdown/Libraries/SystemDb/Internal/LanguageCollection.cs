using System.Collections;
using System.Collections.Generic;

namespace SystemDb.Internal
{
	internal class LanguageCollection : Dictionary<string, ILanguage>, ILanguageCollection, IEnumerable<ILanguage>, IEnumerable
	{
		public new ILanguage this[string countryCode]
		{
			get
			{
				if (!ContainsKey(countryCode))
				{
					return null;
				}
				return base[countryCode];
			}
		}

		public new IEnumerator<ILanguage> GetEnumerator()
		{
			return base.Values.GetEnumerator();
		}

		public void Add(Language language)
		{
			Add(language.CountryCode, language);
		}
	}



}
