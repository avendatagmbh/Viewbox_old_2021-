using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Data;

namespace eBalanceKit.Converters {

    public class StringToColDescriptionConverter : IValueConverter {
        
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture) {

            string strval = value as string;
            if (string.IsNullOrEmpty(strval)) return "noch Keine Spalte ausgewählt";
            else return strval;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture) {
            throw new NotImplementedException();
        }
    }
}
