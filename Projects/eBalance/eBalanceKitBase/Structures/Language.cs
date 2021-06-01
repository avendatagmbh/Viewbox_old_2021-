// --------------------------------------------------------------------------------
// author: Mirko Dibbert
// since:  2012-03-13
// copyright 2012 AvenDATA GmbH
// --------------------------------------------------------------------------------
using System.Globalization;
using System.Threading;

namespace eBalanceKitBase.Structures {
    public class Language {
        public Language(string cultureId, string displayNameDe, string displayNameEn) {
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