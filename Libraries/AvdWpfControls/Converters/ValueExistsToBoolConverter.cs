using System;
using System.Globalization;
using System.Windows.Data;

namespace AvdWpfControls.Converters
{
    public class ValueExistsToBoolConverter : IValueConverter
    {
        public ValueExistsToBoolConverter()
        {
            ExistsToTrue = true;
        }

        public bool ExistsToTrue { get; set; }

        #region IValueConverter Members

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null) return !ExistsToTrue;
            return ExistsToTrue;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}