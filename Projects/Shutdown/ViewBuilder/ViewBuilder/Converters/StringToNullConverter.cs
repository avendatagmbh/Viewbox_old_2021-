using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Windows.Data;

namespace ViewBuilder.Converters {
    public class StringToNullConverter : IValueConverter {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            string stringValue = value as string;
            if (stringValue == null)
                return "NULL";
            return value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
            string stringValue = value as string;
            if (stringValue == "NULL")
                return null;
            return value;
        }
    }
}
