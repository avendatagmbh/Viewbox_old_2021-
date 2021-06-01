using System;
using System.Windows;
using System.Windows.Data;

namespace AvdWpfControls.Converters {
    public class EmptyStringToVisibilityConverter : IValueConverter {
        public bool EmptyToVisible { get; set; }

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture) {
            if (value == null || string.IsNullOrEmpty(value.ToString())) return EmptyToVisible ? Visibility.Visible : Visibility.Collapsed;
            return EmptyToVisible ? Visibility.Collapsed : Visibility.Visible;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture) {
            throw new NotImplementedException();
        }
    }
}
