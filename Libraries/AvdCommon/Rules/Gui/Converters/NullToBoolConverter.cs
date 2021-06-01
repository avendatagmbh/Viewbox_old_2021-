using System;
using System.Globalization;
using System.Windows.Data;

namespace AvdCommon.Rules.Gui.Converters
{
    public class NullToBoolConverter : IValueConverter
    {
        public NullToBoolConverter()
        {
            NullToTrue = false;
        }

        public bool NullToTrue { get; set; }

        #region IValueConverter Members

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null) return NullToTrue;
            return !NullToTrue;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}