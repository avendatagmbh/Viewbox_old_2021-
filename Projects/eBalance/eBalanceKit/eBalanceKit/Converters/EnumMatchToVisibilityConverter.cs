using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Data;
using System.Globalization;
using System.Windows;

namespace eBalanceKit.Converters {
    
    public class EnumMatchToVisibilityConverter : IValueConverter {
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
    }
}
