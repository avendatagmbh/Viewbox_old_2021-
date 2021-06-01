/**************************************************************************************************************
 * author               date            comment
 * ------------------------------------------------------------------------------------------------------------
 * Mirko Dibbert        2010-10-22      initial implementation
 *************************************************************************************************************/

using System;
using System.Windows;
using System.Windows.Data;

namespace ViewValidator.Converters {

    public class BoolToVisibilityConverter : IValueConverter {

        public BoolToVisibilityConverter() {
            this.VisibleValue = false;
            this.HiddenState = Visibility.Hidden;
        }

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture) {
            if (value is bool) {
                bool boolValue = (bool)value;
                if (boolValue == VisibleValue) return Visibility.Visible;
                return this.HiddenState;
            } else {
                bool? boolValue = (bool?)value;
                if (boolValue.HasValue && boolValue == VisibleValue) return Visibility.Visible;
                return this.HiddenState;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture) {
            throw new NotImplementedException();
        }

        public bool VisibleValue { get; set; }

        public Visibility HiddenState { get; set; }
    }
}
