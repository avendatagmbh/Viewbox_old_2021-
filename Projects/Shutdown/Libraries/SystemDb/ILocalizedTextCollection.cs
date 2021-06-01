using System;
using System.Collections;
using System.Collections.Generic;

namespace SystemDb
{
	public interface ILocalizedTextCollection : ICloneable, IEnumerable<KeyValuePair<string, string>>, IEnumerable
	{
		string this[ILanguage language] { get; set; }
	}
}
