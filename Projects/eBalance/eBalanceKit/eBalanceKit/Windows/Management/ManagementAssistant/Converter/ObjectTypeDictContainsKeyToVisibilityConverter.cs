// --------------------------------------------------------------------------------
// author: Sebastian Vetter
// since: 2012-07-06
// copyright 2012 AvenDATA GmbH
// --------------------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace eBalanceKit.Windows.Management.ManagementAssistant.Converter {
    public class ObjectTypeDictContainsKeyToVisibilityConverter : ObjectTypeDictContainsKeyToBoolConverter {
        
        public override object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            ObjectTypes objectType;

            if (parameter == null || value == null) {
                return GetVisibilityResult(false);
            }

            if (Enum.TryParse(parameter.ToString(), true, out objectType)) {
                if (value is Dictionary<ObjectTypes, object>) {
                    return GetVisibilityResult((value as Dictionary<ObjectTypes, object>).ContainsKey(objectType));
                }
            }

            return GetVisibilityResult(false);
        }

        public new object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) { return null; }

        private Visibility GetVisibilityResult(bool baseValue) {
            // true = Visible
            // false = Collapsed
            return base.GetResult(baseValue) ? Visibility.Visible : Visibility.Collapsed;
        }

    }
}