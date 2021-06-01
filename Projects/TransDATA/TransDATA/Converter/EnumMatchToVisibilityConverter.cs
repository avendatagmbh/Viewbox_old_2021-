using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace TransDATA.Converter {
    public class EnumMatchToVisibilityConverter : IValueConverter {
        #region IValueConverter Members
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            if (value == null || parameter == null) return Visibility.Collapsed;
            string checkValue = value.ToString();
            string targetValue = parameter.ToString();

            if (checkValue.Equals(targetValue, StringComparison.InvariantCultureIgnoreCase)) return Visibility.Visible;
            else return Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
            // method not used since converter is only used for one way bindings
            throw new NotImplementedException();
        }
        #endregion
    }
}