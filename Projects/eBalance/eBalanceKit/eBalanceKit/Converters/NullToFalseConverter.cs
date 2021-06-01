using System;
using System.Windows.Data;

namespace eBalanceKit.Converters {
    public class NullToFalseConverter : IValueConverter {

        public NullToFalseConverter() {
            this.NullToFalse = false;
        }

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture) {
            if (value == null) return NullToFalse;
            return !NullToFalse;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture) {
            throw new NotImplementedException();
        }

        public bool NullToFalse { get; set; }
    }
}
