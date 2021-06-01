using System;
using System.Windows.Data;
using System.Windows;

namespace eBalanceKit.Converters {

    public class BoolToVisibilityConverter : IValueConverter {

        public BoolToVisibilityConverter() {
            VisibleValue = false;
            HiddenState = Visibility.Hidden;
        }

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture) {
            if (value is bool) {
                bool boolValue = (bool)value;
                return boolValue == VisibleValue ? Visibility.Visible : HiddenState;
            } else {
                bool? boolValue = (bool?)value;
                if (boolValue.HasValue && boolValue == VisibleValue) return Visibility.Visible;
                return HiddenState;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture) {
            throw new NotImplementedException();
        }

        public bool VisibleValue { get; set; }

        public Visibility HiddenState { get; set; }
    }
}
