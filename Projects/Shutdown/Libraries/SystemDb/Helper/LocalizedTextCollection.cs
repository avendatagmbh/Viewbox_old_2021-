using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace SystemDb.Helper
{
	public class LocalizedTextCollection : Dictionary<string, string>, ILocalizedTextCollection, ICloneable, IEnumerable<KeyValuePair<string, string>>, IEnumerable
	{
		public string First
		{
			get
			{
				if (base.Keys.Count <= 0)
				{
					return null;
				}
				return base[base.Keys.First()];
			}
		}

		public string this[ILanguage language]
		{
			get
			{
				if (!ContainsKey(language.CountryCode))
				{
					return null;
				}
				return base[language.CountryCode];
			}
			set
			{
				base[language.CountryCode] = value;
			}
		}

		public object Clone()
		{
			LocalizedTextCollection coll = new LocalizedTextCollection();
			using Enumerator enumerator = GetEnumerator();
			while (enumerator.MoveNext())
			{
				KeyValuePair<string, string> text = enumerator.Current;
				coll.Add(text.Key, text.Value);
			}
			return coll;
		}

		public void Add(ILanguage language, string text)
		{
			try
			{
				base[language.CountryCode] = text;
			}
			catch (Exception)
			{
			}
		}
	}
}
