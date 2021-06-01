using System;
using System.Globalization;
using System.Windows.Data;

namespace TransDATA.Converter {
    public class ValueNotExistsToBoolConverter : IValueConverter {
        #region IValueConverter Members
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            if (value == null) return true;
            return false;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
            throw new NotImplementedException();
        }
        #endregion
    }
}