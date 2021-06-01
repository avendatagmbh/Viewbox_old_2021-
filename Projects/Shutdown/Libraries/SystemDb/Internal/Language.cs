using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using SystemDb.Resources;
using DbAccess.Attributes;

namespace SystemDb.Internal
{
	[DbTable("languages", ForceInnoDb = true)]
	public class Language : ILanguage
	{
		private static readonly Dictionary<string, Tuple<string, string>> _languageDescriptions;

		private static readonly Dictionary<string, CultureInfo> _languages;

		public static readonly Language DefaultLanguage;

		private CultureInfo cultureInfo;

		public static Dictionary<string, Tuple<string, string>> LanguageDescriptions => _languageDescriptions;

		public static Dictionary<string, CultureInfo> Languages => _languages;

		[DbColumn("id")]
		[DbPrimaryKey]
		public int Id { get; set; }

		[DbColumn("country_code", Length = 2)]
		public string CountryCode { get; set; }

		[DbColumn("language_name", Length = 128)]
		public string LanguageName { get; set; }

		[DbColumn("language_motto", Length = 1024)]
		public string LanguageMotto { get; set; }

		public CultureInfo CultureInfo
		{
			get
			{
				if (cultureInfo == null)
				{
					cultureInfo = new CultureInfo(CountryCode);
				}
				return cultureInfo;
			}
		}

		static Language()
		{
			_languageDescriptions = new Dictionary<string, Tuple<string, string>>();
			_languages = new Dictionary<string, CultureInfo>();
			CultureInfo language = new CultureInfo("en");
			_languageDescriptions.Add(language.IetfLanguageTag, new Tuple<string, string>(global::SystemDb.Resources.Resources.ResourceManager.GetString("languageName", language), global::SystemDb.Resources.Resources.ResourceManager.GetString("motto", language)));
			_languages.Add(language.IetfLanguageTag, language);
			language = new CultureInfo("de");
			_languageDescriptions.Add(language.IetfLanguageTag, new Tuple<string, string>(global::SystemDb.Resources.Resources.ResourceManager.GetString("languageName", language), global::SystemDb.Resources.Resources.ResourceManager.GetString("motto", language)));
			_languages.Add(language.IetfLanguageTag, language);
			language = new CultureInfo("es");
			_languageDescriptions.Add(language.IetfLanguageTag, new Tuple<string, string>(global::SystemDb.Resources.Resources.ResourceManager.GetString("languageName", language), global::SystemDb.Resources.Resources.ResourceManager.GetString("motto", language)));
			_languages.Add(language.IetfLanguageTag, language);
			language = new CultureInfo("fr");
			_languageDescriptions.Add(language.IetfLanguageTag, new Tuple<string, string>(global::SystemDb.Resources.Resources.ResourceManager.GetString("languageName", language), global::SystemDb.Resources.Resources.ResourceManager.GetString("motto", language)));
			_languages.Add(language.IetfLanguageTag, language);
			language = new CultureInfo("it");
			_languageDescriptions.Add(language.IetfLanguageTag, new Tuple<string, string>(global::SystemDb.Resources.Resources.ResourceManager.GetString("languageName", language), global::SystemDb.Resources.Resources.ResourceManager.GetString("motto", language)));
			_languages.Add(language.IetfLanguageTag, language);
			DefaultLanguage = new Language
			{
				CountryCode = LanguageDescriptions.First().Key,
				LanguageName = LanguageDescriptions.First().Value.Item1,
				LanguageMotto = LanguageDescriptions.First().Value.Item2
			};
		}
	}
}
