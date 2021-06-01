using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Localisation {

    public interface ILanguage {
        string CountryCode { get; }
        string LanguageName { get; }
    }
}
