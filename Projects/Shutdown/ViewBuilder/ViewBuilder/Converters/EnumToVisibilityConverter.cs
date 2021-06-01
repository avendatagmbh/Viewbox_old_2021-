using System;
using System.Windows;
using System.Windows.Data;

namespace ViewBuilder.Converters {

    public class EnumToVisibilityConverter : IValueConverter {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture) {
            string ParameterString = parameter as string;
            if (ParameterString == null) {
                return Visibility.Hidden;
            }

            if (Enum.IsDefined(value.GetType(), value) == false) {
                return Visibility.Hidden;
            }

            object paramvalue = Enum.Parse(value.GetType(), ParameterString);

            if (paramvalue.Equals(value)) {
                return Visibility.Visible;
            } else {
                return Visibility.Hidden;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture) {
            string ParameterString = parameter as string;
            if (ParameterString == null) {
                return DependencyProperty.UnsetValue;
            }

            return Enum.Parse(targetType, ParameterString);
        }
    }
}
