// --------------------------------------------------------------------------------
// author: Mirko Dibbert
// since:  2011-09-26
// copyright 2011 AvenDATA GmbH
// --------------------------------------------------------------------------------
using System.Windows.Data;
using Business.Structures;

namespace TransDATA.Converter {
    public class LanguageToImageConverter : IValueConverter {

        public object Convert(object value, System.Type targetType, object parameter, System.Globalization.CultureInfo culture) {
            var language = value as Language;
            if (language == null) return null;
            switch (language.Culture.Name) {
                case "de-DE":
                    return "/TransDATA;component/Resources/DE.png";
                case "en-US":
                    return "/TransDATA;component/Resources/EN.png";
            }
            return null;
        }

        public object ConvertBack(object value, System.Type targetType, object parameter, System.Globalization.CultureInfo culture) {
            throw new System.NotImplementedException();
        }
    }
}