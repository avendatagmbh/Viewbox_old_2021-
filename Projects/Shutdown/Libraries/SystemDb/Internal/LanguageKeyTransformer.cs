using System;
using System.Collections.Generic;

namespace SystemDb.Internal
{
	public static class LanguageKeyTransformer
	{
		private static readonly Dictionary<string, string> keyValues = new Dictionary<string, string>
		{
			{ "sr", "0" },
			{ "zh", "1" },
			{ "th", "2" },
			{ "ko", "3" },
			{ "ro", "4" },
			{ "sl", "5" },
			{ "hr", "6" },
			{ "ms", "7" },
			{ "uk", "8" },
			{ "et", "9" },
			{ "ar", "A" },
			{ "he", "B" },
			{ "cs", "C" },
			{ "de", "D" },
			{ "en", "E" },
			{ "fr", "F" },
			{ "el", "G" },
			{ "hu", "H" },
			{ "it", "I" },
			{ "ja", "J" },
			{ "da", "K" },
			{ "pl", "L" },
			{ "zf", "M" },
			{ "nl", "N" },
			{ "no", "O" },
			{ "pt", "P" },
			{ "sk", "Q" },
			{ "ru", "R" },
			{ "es", "S" },
			{ "tr", "T" },
			{ "fi", "U" },
			{ "sv", "V" },
			{ "bg", "W" },
			{ "lt", "X" },
			{ "lv", "Y" },
			{ "z1", "Z" },
			{ "af", "a" },
			{ "id", "i" }
		};

		public static string Transformer(string key)
		{
			try
			{
				if (key != null && key.Length == 1)
				{
					return key;
				}
				return keyValues[key];
			}
			catch (Exception)
			{
				return string.Empty;
			}
		}
	}
}
