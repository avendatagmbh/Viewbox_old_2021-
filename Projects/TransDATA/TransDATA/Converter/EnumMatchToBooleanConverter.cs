using System;
using System.Globalization;
using System.Windows.Data;

namespace TransDATA.Converter {
    public class EnumMatchToBooleanConverter : IValueConverter {
        #region IValueConverter Members
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            if (value == null || parameter == null) return false;
            string checkValue = value.ToString();
            string targetValue = parameter.ToString();
            return checkValue.Equals(targetValue, StringComparison.InvariantCultureIgnoreCase);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
            if (value == null || parameter == null) return null;
            var useValue = (bool) value;
            string targetValue = parameter.ToString();
            if (useValue) return Enum.Parse(targetType, targetValue);
            return null;
        }
        #endregion
    }
}