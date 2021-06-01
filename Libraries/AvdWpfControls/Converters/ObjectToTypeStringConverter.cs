using System;
using System.Globalization;
using System.Windows.Data;

namespace AvdWpfControls.Converters
{
    public class ObjectToTypeStringConverter : IValueConverter
    {
        #region IValueConverter Members

        public object Convert(
            object value, Type targetType,
            object parameter, CultureInfo culture)
        {
            return value == null ? null : value.GetType().Name;
        }

        public object ConvertBack(object value, Type targetType, object parameter,
                                  CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}