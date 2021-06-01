using System;
using System.Collections.Generic;
using System.Windows.Data;

namespace ViewValidator.Converters {
    class DbNullToFontConverter : IValueConverter {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture) {
            int column = (int)parameter;
            List<ViewValidator.Models.Profile.PreviewData.Entry> entries = value as List<ViewValidator.Models.Profile.PreviewData.Entry>;
            if (entries != null && column < entries.Count && entries[column].DisplayString.ToLower() == "dbnull")
                return System.Windows.FontStyles.Oblique;
            return System.Windows.FontStyles.Normal;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture) {
            throw new NotImplementedException();
        }
    }
}
