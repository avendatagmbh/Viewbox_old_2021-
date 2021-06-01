using System;

namespace Utils
{
    public class SapLanguageCodeHelper
    {
        public static string GetSapLanguageCode(string lang)
        {
            string sapLanguageCode = null;
            switch (lang.ToLower())
            {
                case "en":
                    sapLanguageCode = "E";
                    break;
                case "de":
                    sapLanguageCode = "D";
                    break;
                case "es":
                    sapLanguageCode = "S";
                    break;
                case "fr":
                    sapLanguageCode = "F";
                    break;
                case "it":
                    sapLanguageCode = "I";
                    break;
                default:
                    throw new NotImplementedException("Language code " + lang + " is not implemented!");
            }
            return sapLanguageCode;
        }

        public static string GetViewBuilderLanguageCode(string lang)
        {
            string languageCode = null;
            switch (lang.ToUpper())
            {
                case "E":
                    languageCode = "en";
                    break;
                case "D":
                    languageCode = "de";
                    break;
                case "S":
                    languageCode = "es";
                    break;
                case "F":
                    languageCode = "fr";
                    break;
                case "I":
                    languageCode = "it";
                    break;
                default:
                    throw new NotImplementedException("Language code " + lang + " is not implemented!");
            }
            return languageCode;
        }
    }
}