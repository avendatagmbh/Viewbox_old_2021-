using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Data;

namespace ViewValidator.Converters {
    class IsVisibleToOpacityConverter : IValueConverter {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture) {
            if(System.Convert.ToBoolean(value)) return 1.0;
            return 0.25;
        }

        public object ConvertBack(object value, Type targetType, object parameter,System.Globalization.CultureInfo culture) {
            throw new NotSupportedException();
        }
    }
}
