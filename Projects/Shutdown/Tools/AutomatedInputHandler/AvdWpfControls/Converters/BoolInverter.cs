// --------------------------------------------------------------------------------
// author: Mirko Dibbert
// since: 2012-02-29
// copyright 2012 AvenDATA GmbH
// --------------------------------------------------------------------------------
using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Markup;

namespace AvdWpfControls.Converters {
    public class BoolInverter : MarkupExtension, IValueConverter {
        public override object ProvideValue(IServiceProvider serviceProvider) {
            return this;
        }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            if (value is bool)
                return !(bool)value;
            return value;
        }
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
            return Convert(value, targetType, parameter, culture);
        }
    }
}