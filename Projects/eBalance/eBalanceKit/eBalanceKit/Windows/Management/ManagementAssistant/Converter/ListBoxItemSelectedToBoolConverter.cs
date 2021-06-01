// --------------------------------------------------------------------------------
// author: Sebastian Vetter
// since: 2012-07-10
// copyright 2012 AvenDATA GmbH
// --------------------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Windows.Controls;
using System.Windows.Data;

namespace eBalanceKit.Windows.Management.ManagementAssistant.Converter {
    public class ListBoxItemSelectedToBoolConverter :IValueConverter {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            if (value is ListBox) {
                var lb = value as ListBox;
                if (lb.SelectedItem == parameter) {
                    return true;
                }
                if (parameter is int) {
                    return lb.SelectedIndex == (int) parameter;
                }
            }

            return value == parameter;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) { return null; }
    }
}