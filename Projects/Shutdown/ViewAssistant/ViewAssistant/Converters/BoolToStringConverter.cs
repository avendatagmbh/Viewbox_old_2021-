using System;
using System.Globalization;
using System.Windows.Data;

namespace ViewAssistant.Converters
{
    public class BoolToStringConverter : IValueConverter
    {
        public BoolToStringConverter()
        {
            WhenFalse = "";
            WhenTrue = "";
        }

        public string WhenTrue { get; set; }
        public string WhenFalse { get; set; }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool)
            {
                var boolValue = (bool)value;
                if (boolValue) return WhenTrue;
                return WhenFalse;
            }
            else
            {
                var boolValue = (bool?)value;
                if (boolValue.HasValue && boolValue.Value) return WhenTrue;
                return WhenFalse;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}