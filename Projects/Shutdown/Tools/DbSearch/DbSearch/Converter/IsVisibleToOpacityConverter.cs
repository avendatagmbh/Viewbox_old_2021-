using System;
using System.Windows.Data;

namespace DbSearch.Converter {
    class IsVisibleToOpacityConverter : IValueConverter {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture) {
            if(System.Convert.ToBoolean(value)) return 1.0;
            return 0.25;
        }

        public object ConvertBack(object value, Type targetType, object parameter,System.Globalization.CultureInfo culture) {
            throw new NotSupportedException();
        }
    }
}
