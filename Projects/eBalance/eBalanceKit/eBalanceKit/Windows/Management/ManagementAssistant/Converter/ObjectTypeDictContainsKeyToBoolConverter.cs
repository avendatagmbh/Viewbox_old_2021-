// --------------------------------------------------------------------------------
// author: Sebastian Vetter
// since: 2012-07-06
// copyright 2012 AvenDATA GmbH
// --------------------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Windows.Data;

namespace eBalanceKit.Windows.Management.ManagementAssistant.Converter {
    public class ObjectTypeDictContainsKeyToBoolConverter : IValueConverter {

        public ObjectTypeDictContainsKeyToBoolConverter() { Invert = false; }
        
        public virtual object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            ObjectTypes objectType;

            if (parameter == null || value == null) {
                return GetResult(false);
            }

            if (Enum.TryParse(parameter.ToString(), true, out objectType)) {
                if (value is Dictionary<ObjectTypes, object>) {
                    return GetResult((value as Dictionary<ObjectTypes, object>).ContainsKey(objectType) && (value as Dictionary<ObjectTypes, object>)[objectType] != null);
                }
            }

            return GetResult(false);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) { return null; }

        public bool Invert { get; set; }
        
        protected bool GetResult(bool baseValue) {
            if (Invert) {
                return !baseValue;
            }
            return baseValue;
        }

    }
}