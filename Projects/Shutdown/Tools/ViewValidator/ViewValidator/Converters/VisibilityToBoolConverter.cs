using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Data;

namespace ViewValidator.Converters {
    public class VisibilityToBoolConverter : IValueConverter {

        public VisibilityToBoolConverter() {
            this.VisibleToTrue = true;
        }

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture) {
            
            if (value is Visibility) {
                Visibility state = (Visibility) value;

                if (state == Visibility.Visible) return VisibleToTrue;
                return !VisibleToTrue;
            }
            return false;
        }

        public object ConvertBack(object value, Type targetType, object parameter,
                                  System.Globalization.CultureInfo culture) {
            throw new NotImplementedException();
        }

        public bool VisibleToTrue { get; set; }
    }
}
