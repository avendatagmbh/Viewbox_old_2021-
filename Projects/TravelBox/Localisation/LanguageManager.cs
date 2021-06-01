using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Globalization;

namespace Localisation {

    /// <summary>
    /// This class provides some static language properties.
    /// </summary>
    /// <author>Mirko Dibbert</author>
    /// <since>2011-07-28</since>
    public static class LanguageManager {

        static LanguageManager() {
            _languages = new Dictionary<string, ILanguage>();
            foreach (var lang in new ILanguage[]{

                /**************************************************/
                /* add new languages here
                /* ensure to add the resouce files foreach new language, 
                 * otherwhise the fallback language would be used
                /**************************************************/
                new Language("de", "German"),
                new Language("en", "English")
            
            }) { _languages.Add(lang.CountryCode, lang); }
        }

        public static IEnumerable<ILanguage> Languages { get { return _languages.Values; } }
        private static Dictionary<string, ILanguage> _languages;

        /// <summary>
        /// Returs the fallback language, which is used if the resource file does not contain the specified resource.
        /// </summary>
        public static ILanguage FallbackLanguage { get { return _languages["en"]; } }
    }
}
