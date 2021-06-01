using System;
using System.Globalization;
using System.Windows.Data;

namespace ViewAssistant.Converters
{
    internal class EmptyStringToNotSetStringConverter : IValueConverter
    {
        #region IValueConverter Members
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null || value.ToString() == string.Empty)
                return Base.Localisation.ResourcesCommon.Common_ValueNotSet;

            return value.ToString();
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value;
        }
        #endregion
    }
}
