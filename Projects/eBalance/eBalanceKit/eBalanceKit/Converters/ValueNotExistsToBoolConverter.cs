using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Data;
using System.Windows;

namespace eBalanceKit.Converters {

    public class ValueNotExistsToBoolConverter : IValueConverter {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture) {
            if (value == null) return true;
            if (value is Dictionary<object,object> && parameter != null) {
                return !(value as Dictionary<object, object>).ContainsKey(parameter);
            }
            return false;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture) {
            throw new NotImplementedException();
        }
    }
}
