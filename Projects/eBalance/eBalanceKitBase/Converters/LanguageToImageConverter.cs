// --------------------------------------------------------------------------------
// author: Mirko Dibbert
// since:  2012-03-13
// copyright 2011 AvenDATA GmbH
// --------------------------------------------------------------------------------
using System.Windows.Data;
using eBalanceKitBase.Structures;

namespace eBalanceKitBase.Converters {
    public class LanguageToImageConverter : IValueConverter {

        public object Convert(object value, System.Type targetType, object parameter, System.Globalization.CultureInfo culture) {
            var language = value as Language;
            if (language == null) return null;
            switch (language.Culture.Name) {
                case "de-DE":
                    return "/eBalanceKitResources;component/Resources/DE.png";
                case "en-US":
                    return "/eBalanceKitResources;component/Resources/EN.png";
            }
            return null;
        }

        public object ConvertBack(object value, System.Type targetType, object parameter, System.Globalization.CultureInfo culture) {
            throw new System.NotImplementedException();
        }
    }
}