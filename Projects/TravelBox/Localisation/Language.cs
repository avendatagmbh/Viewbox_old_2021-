using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Localisation {
    
    internal class Language : ILanguage {

        internal Language(string countryCode, string languageName) {
            CountryCode = countryCode;
            LanguageName = languageName;
        }

        public string CountryCode { get; private set; }
        public string LanguageName { get; private set;  }
    }
}
