using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace AvdWpfControls.Converters
{
    public class EnumMatchToVisibilityConverter : IValueConverter
    {
        #region IValueConverter Members

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string parameterString = parameter as string;
            if (parameterString == null)
            {
                return Visibility.Collapsed;
            }
            if (Enum.IsDefined(value.GetType(), value) == false)
            {
                return DependencyProperty.UnsetValue;
            }
            object paramvalue = Enum.Parse(value.GetType(), parameterString);
            if (paramvalue.Equals(value))
            {
                return Visibility.Visible;
            }
            else
            {
                return Visibility.Collapsed;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}