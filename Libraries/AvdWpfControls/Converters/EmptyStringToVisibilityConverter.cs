using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace AvdWpfControls.Converters
{
    public class EmptyStringToVisibilityConverter : IValueConverter
    {
        public bool EmptyToVisible { get; set; }

        #region IValueConverter Members

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null || string.IsNullOrEmpty(value.ToString()))
                return EmptyToVisible ? Visibility.Visible : Visibility.Collapsed;
            return EmptyToVisible ? Visibility.Collapsed : Visibility.Visible;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}