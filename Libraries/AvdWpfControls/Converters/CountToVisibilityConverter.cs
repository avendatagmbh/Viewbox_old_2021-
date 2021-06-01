using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace AvdWpfControls.Converters
{
    public class CountToVisibilityConverter : IValueConverter
    {
        public CountToVisibilityConverter()
        {
            IsZeroState = Visibility.Collapsed;
            IsNotZeroState = Visibility.Visible;
        }

        public Visibility IsZeroState { get; set; }
        public Visibility IsNotZeroState { get; set; }

        #region IValueConverter Members

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is int)
            {
                int intValue = (int) value;
                if (intValue == 0) return IsZeroState;
                return IsNotZeroState;
            }
            return IsZeroState;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}