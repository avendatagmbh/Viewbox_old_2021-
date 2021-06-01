using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Data;

namespace eBalanceKit.Converters {
    public class ZeroToVisibilityConverter : IValueConverter {
        public ZeroToVisibilityConverter() {
            ZeroVisibility = Visibility.Collapsed;
            NonZeroVisibility = Visibility.Visible;
        }

        public Visibility ZeroVisibility { get; set; }
        public Visibility NonZeroVisibility { get; set; }

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture) {
            if(value == null)
                return ZeroVisibility;
            int valueInt;
            if (Int32.TryParse(value.ToString(), out valueInt)) return valueInt == 0 ? ZeroVisibility : NonZeroVisibility;
            return ZeroVisibility;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture) {
            throw new NotImplementedException();
        }
    }
}
