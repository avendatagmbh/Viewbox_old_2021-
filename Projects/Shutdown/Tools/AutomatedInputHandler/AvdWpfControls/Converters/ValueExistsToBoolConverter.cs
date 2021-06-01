using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Data;

namespace AvdWpfControls.Converters {
    public class ValueExistsToBoolConverter :IValueConverter{
        public ValueExistsToBoolConverter() {
            ExistsToTrue = true;
        }

        public bool ExistsToTrue { get; set; }

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture) {
            if (value == null) return !ExistsToTrue;
            return ExistsToTrue;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture) {
            throw new NotImplementedException();
        }
    }
}
