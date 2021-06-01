/**************************************************************************************************************
 * author               date            comment
 * ------------------------------------------------------------------------------------------------------------
 * Mirko Dibbert        2010-10-22      initial implementation
 *************************************************************************************************************/

using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;

namespace AvdWpfControls.Converters {

    [ValueConversion(typeof(bool), typeof(Brush))]
    public class BoolToBrushConverter : IValueConverter {

        public Brush True { get; set; }

        public Brush False { get; set; }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            if (targetType != typeof(Brush)) return null;
            return ((bool)value) ? True : False;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
            throw new NotImplementedException();
        }
    }
}
