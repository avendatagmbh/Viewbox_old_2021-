using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Data;

namespace TransDATA.Converter {
    public class BoolInversionConverter : IValueConverter {
        public BoolInversionConverter() {
        }

        #region IValueConverter Members
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            return !((bool)value);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
            return !((bool)value);
        }
        #endregion
    }
}
