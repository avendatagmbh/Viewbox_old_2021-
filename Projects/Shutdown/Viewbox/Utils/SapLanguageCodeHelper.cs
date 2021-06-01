using System;

namespace Utils
{
	public class SapLanguageCodeHelper
	{
		public static string GetSapLanguageCode(string lang)
		{
			return lang.ToLower() switch
			{
				"en" => "E", 
				"de" => "D", 
				"es" => "S", 
				"fr" => "F", 
				"it" => "I", 
				_ => throw new NotImplementedException("Language code " + lang + " is not implemented!"), 
			};
		}

		public static string GetViewBuilderLanguageCode(string lang)
		{
			return lang.ToUpper() switch
			{
				"E" => "en", 
				"D" => "de", 
				"S" => "es", 
				"F" => "fr", 
				"I" => "it", 
				_ => throw new NotImplementedException("Language code " + lang + " is not implemented!"), 
			};
		}
	}
}
