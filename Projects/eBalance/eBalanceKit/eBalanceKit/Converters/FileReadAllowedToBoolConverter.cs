// --------------------------------------------------------------------------------
// author: Sebastian Vetter
// since: 2012-03-12
// copyright 2012 AvenDATA GmbH
// --------------------------------------------------------------------------------
using System;
using System.Globalization;
using System.IO;
using System.Windows.Data;

namespace eBalanceKit.Converters {
    public class FileReadAllowedToBoolConverter : IValueConverter {

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            if (value == null) return false;

            if (string.IsNullOrWhiteSpace(value.ToString())) {
                return false;
            }
            
            if (!File.Exists(value.ToString()))
                return false;

            try {
                File.OpenRead(value.ToString());
                return true;
            }
            catch (Exception) {
                return false;
            }

        }

        /// <summary>
        /// Konvertiert einen Wert. 
        /// </summary>
        /// <returns>
        /// Ein konvertierter Wert.Wenn die Methode null zurückgibt, wird der gültige NULL-Wert verwendet.
        /// </returns>
        /// <param name="value">Der Wert, der vom Bindungsziel erzeugt wird.</param><param name="targetType">Der Typ, in den konvertiert werden soll.</param><param name="parameter">Der zu verwendende Konverterparameter.</param><param name="culture">Die im Konverter zu verwendende Kultur.</param>
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) { return null; }
    }
}