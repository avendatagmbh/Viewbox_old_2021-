using System;
using System.Globalization;
using System.Windows.Data;

namespace ViewAssistant.Converters {
    public class BoolInversionConverter : IValueConverter {
        #region IValueConverter Members
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            return !((bool)value);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
            return !((bool)value);
        }
        #endregion
    }
}
