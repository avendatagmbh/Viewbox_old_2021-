// --------------------------------------------------------------------------------
// author: Mirko Dibbert
// since:  2011-09-26
// copyright 2011 AvenDATA GmbH
// --------------------------------------------------------------------------------
using System.Globalization;
using System.Threading;

namespace Business.Structures {
    public class Language {
        internal Language(string cultureId, string displayNameDe, string displayNameEn) {
            DisplayNameDe = displayNameDe;
            DisplayNameEn = displayNameEn;
            Culture = CultureInfo.CreateSpecificCulture(cultureId);
        }

        private string DisplayNameDe { get; set; }
        private string DisplayNameEn { get; set; }

        public string DisplayName { get { return Thread.CurrentThread.CurrentUICulture.Name == "de-DE" ? DisplayNameDe : DisplayNameEn; } }
        public CultureInfo Culture { get; private set; }
    }
}