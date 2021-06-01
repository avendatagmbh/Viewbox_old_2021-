using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace ViewAssistant.Converters
{
    public class BoolToVisibilityConverter : IValueConverter
    {
        public BoolToVisibilityConverter()
        {
            VisibleValue = false;
            HiddenState = Visibility.Hidden;
        }

        public bool VisibleValue { get; set; }

        public Visibility HiddenState { get; set; }

        #region IValueConverter Members
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool)
            {
                var boolValue = (bool)value;
                if (boolValue == VisibleValue) return Visibility.Visible;
                return HiddenState;
            }
            else
            {
                var boolValue = (bool?)value;
                if (boolValue.HasValue && boolValue == VisibleValue) return Visibility.Visible;
                return HiddenState;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
        #endregion
    }
}