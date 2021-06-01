using System;
using System.Windows.Data;
using System.Globalization;

namespace eBalanceKit.Converters {

    public class EnumMatchToBooleanConverter : IValueConverter {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            if (Nullable.GetUnderlyingType(targetType) != null) {
                if (value == null || parameter == null) return false;
                string checkValue = value.ToString();
                string targetValue = parameter.ToString();
                return checkValue.Equals(targetValue, StringComparison.InvariantCultureIgnoreCase);                
            } else {
                if (value == null || parameter == null) return false;
                string checkValue = value.ToString();
                string targetValue = parameter.ToString();
                return checkValue.Equals(targetValue, StringComparison.InvariantCultureIgnoreCase);
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
            if (Nullable.GetUnderlyingType(targetType) != null) {
                var underlyingType = Nullable.GetUnderlyingType(targetType);
                if (value == null || parameter == null) return null;
                bool useValue = (bool)value;
                if (!useValue) return null;
                string targetValue = parameter.ToString();
                if (useValue) return Enum.Parse(underlyingType, targetValue);
                return null;
            } else {
                if (value == null || parameter == null) return null;
                bool useValue = (bool)value;
                string targetValue = parameter.ToString();
                if (useValue) return Enum.Parse(targetType, targetValue);
                return null;
            }            
        }
    }

    public class EnumMatchToBooleanConverter1 : IValueConverter {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            if (value == null || parameter == null) return false;
            string checkValue = value.ToString();
            string targetValue = parameter.ToString();
            return checkValue.Equals(targetValue, StringComparison.InvariantCultureIgnoreCase);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
            if (value == null || parameter == null) return null;
            bool useValue = (bool)value;
            string targetValue = parameter.ToString();
            if (useValue) return Enum.Parse(targetType, targetValue);
            return null;
        }
    }
}
