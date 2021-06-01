// --------------------------------------------------------------------------------
// author: ???
// since: 2012-03-09
// copyright 2012 AvenDATA GmbH
// --------------------------------------------------------------------------------
using System;
using System.Windows.Data;

namespace AvdWpfControls.Converters {
    public class ObjectToTypeStringConverter : IValueConverter {
        public object Convert(
            object value, Type targetType,
            object parameter, System.Globalization.CultureInfo culture) { return value == null ? null : value.GetType().Name; }

        public object ConvertBack(object value, Type targetType, object parameter,
                                  System.Globalization.CultureInfo culture) { throw new NotImplementedException(); }
    }
}