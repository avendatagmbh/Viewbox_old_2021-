using System;
using System.Collections;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace eBalanceKit.Windows.Import {
    public class NotEmptyListToVisibilityConverter : IValueConverter {
        
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            if (value == null)
                return Visibility.Visible;
            else {
                ICollection list = value as ICollection;

                if (list != null) {
                    if (list.Count == 0)
                        return Visibility.Visible;
                    else
                        return Visibility.Collapsed;
                } else {
                    int count;
                    if (Int32.TryParse(value.ToString(), out count))
                        return count == 0 ? Visibility.Visible: Visibility.Collapsed ;
                    return Visibility.Visible;
                }
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) { throw new NotImplementedException(); }
    }
}

