using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Data;
using System.Windows.Media;

namespace DbSearch.Converter {
    class IsVisibleToBackgroundConverter : IValueConverter {

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture) {
            bool visible = System.Convert.ToBoolean(value);
            if (visible) return Brushes.White;
            byte c = 0;
            return new SolidColorBrush(new Color(){R = c, B=c, G=c, A=100});
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture) {
            throw new NotImplementedException();
        }
    }
}
