// --------------------------------------------------------------------------------
// author: Sebastian Vetter
// since: 2012-07-17
// copyright 2012 AvenDATA GmbH
// --------------------------------------------------------------------------------
using System;
using System.Globalization;
using System.Windows.Data;

namespace eBalanceKit.Windows.Management.ManagementAssistant.Converter {
    public class IsEqualToBoolConverter : IValueConverter {

        public IsEqualToBoolConverter() { Invert = false; }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            if (value == parameter) {
                return GetValue(true);
            }
            if (value.Equals(parameter)) {
                return GetValue(true);
            }
            if (value != null && parameter != null) {
                return GetValue(string.Equals(value.ToString(), parameter.ToString()));
            }

            return false;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) { return null; }

        public bool Invert { get; set; }

        private bool GetValue(bool baseValue) {
            if (Invert) {
                return !baseValue;
            }
            return baseValue;
        }
    }
}