using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;

namespace eBalanceKitBase.Structures {
    public static class LocalisationUtils {
        private static CultureInfo _culture = CultureInfo.CreateSpecificCulture("de-DE");
        public static CultureInfo GermanCulture { get { return _culture; } }

        public static string DecimalToString(decimal value) { return value.ToString("#,0.00", _culture); }
        public static string CurrencyToString(decimal value, string currency = "€") { return value.ToString("#,0.00", _culture) + " " + currency; }

        public static string DecimalToString(decimal? value) { return value.HasValue ? DecimalToString(value.Value) : "-"; }
        public static string CurrencyToString(decimal? value, string currency = "€") { return value.HasValue ? CurrencyToString(value.Value, currency) : "-"; }
    }
}
