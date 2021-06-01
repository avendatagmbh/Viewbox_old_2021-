using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace AvdWpfControls.Converters
{
    public class VisibilityToBoolConverter : IValueConverter
    {
        #region IValueConverter Members

        public object Convert(object value, Type targetType, object parameter,
                              CultureInfo culture)
        {
            try
            {
                Visibility visibilityToConvert = (Visibility) value;
                if ((visibilityToConvert == Visibility.Collapsed)
                    || (visibilityToConvert == Visibility.Hidden))
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            catch
            {
                return false;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter,
                                  CultureInfo culture)
        {
            try
            {
                bool boolToConvert = (bool) value;
                if (boolToConvert)
                {
                    return Visibility.Collapsed;
                }
                else
                {
                    return Visibility.Visible;
                }
            }
            catch
            {
                return Visibility.Collapsed;
            }
        }

        #endregion
    }
}