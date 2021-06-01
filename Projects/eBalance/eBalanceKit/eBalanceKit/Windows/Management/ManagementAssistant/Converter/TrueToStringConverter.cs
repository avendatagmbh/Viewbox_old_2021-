// --------------------------------------------------------------------------------
// author: Sebastian Vetter
// since: 2012-07-19
// copyright 2012 AvenDATA GmbH
// --------------------------------------------------------------------------------
using System;
using System.Globalization;
using System.Windows.Data;

namespace eBalanceKit.Windows.Management.ManagementAssistant.Converter {
    public class TrueToStringConverter : IValueConverter {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) { bool v;
            if (value == null || !bool.TryParse(value.ToString(), out v)) {
                return string.Empty;
            }

            if (parameter == null) {
                return "Please set the ConverterParameter to the string,";
            }

            if (v) {
                return parameter.ToString();
            }
            return string.Empty;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) { return null; }
    }
}