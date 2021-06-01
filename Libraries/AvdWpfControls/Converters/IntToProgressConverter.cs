using System;
using System.Globalization;
using System.Windows.Data;

namespace AvdWpfControls.Converters
{
    public class IntToProgressConverter : IValueConverter
    {
        #region IValueConverter Members

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            int intValue = 0;
            if (value != null)
                Int32.TryParse(value.ToString(), out intValue);
            return String.Format("{0} %", intValue);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}