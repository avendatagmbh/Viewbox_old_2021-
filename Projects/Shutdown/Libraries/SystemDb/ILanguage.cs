using System.Globalization;

namespace SystemDb
{
	public interface ILanguage
	{
		string CountryCode { get; }

		string LanguageName { get; }

		string LanguageMotto { get; }

		CultureInfo CultureInfo { get; }
	}
}
