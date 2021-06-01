using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Data;

namespace AvdWpfControls.Converters {
    public class CountToVisibilityConverter : IValueConverter {

        public CountToVisibilityConverter() {
            IsZeroState = Visibility.Collapsed;
            IsNotZeroState = Visibility.Visible;
        }

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture) {
            if (value is int) {
                int intValue = (int)value;
                if (intValue == 0) return IsZeroState;
                return IsNotZeroState;
            }
            return IsZeroState;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture) {
            throw new NotImplementedException();
        }

        public Visibility IsZeroState { get; set; }
        public Visibility IsNotZeroState { get; set; }
    }
}
