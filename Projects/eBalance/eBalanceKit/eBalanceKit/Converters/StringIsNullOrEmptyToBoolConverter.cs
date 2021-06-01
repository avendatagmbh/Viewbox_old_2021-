// --------------------------------------------------------------------------------
// author: Sebastian Vetter
// since: 2012-07-10
// copyright 2012 AvenDATA GmbH
// --------------------------------------------------------------------------------
using System;
using System.Globalization;
using System.Windows.Data;

namespace eBalanceKit.Converters {
    public class StringIsNullOrEmptyToBoolConverter : IValueConverter {

        public StringIsNullOrEmptyToBoolConverter() { Inverted = false; }

        /// <summary>
        /// Konvertiert einen Wert. 
        /// </summary>
        /// <returns>
        /// Ein konvertierter Wert.Wenn die Methode null zurückgibt, wird der gültige NULL-Wert verwendet.
        /// </returns>
        /// <param name="value">Der von der Bindungsquelle erzeugte Wert.</param><param name="targetType">Der Typ der Bindungsziel-Eigenschaft.</param><param name="parameter">Der zu verwendende Konverterparameter.</param><param name="culture">Die im Konverter zu verwendende Kultur.</param>
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            return value == null ? GetResult(true) : GetResult(string.IsNullOrEmpty(value.ToString()));
        }

        /// <summary>
        /// Konvertiert einen Wert. 
        /// </summary>
        /// <returns>
        /// Ein konvertierter Wert.Wenn die Methode null zurückgibt, wird der gültige NULL-Wert verwendet.
        /// </returns>
        /// <param name="value">Der Wert, der vom Bindungsziel erzeugt wird.</param><param name="targetType">Der Typ, in den konvertiert werden soll.</param><param name="parameter">Der zu verwendende Konverterparameter.</param><param name="culture">Die im Konverter zu verwendende Kultur.</param>
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) { return null; }

        public bool Inverted { get; set; }

        private bool GetResult(bool baseValue) {
            if (Inverted) {
                return !baseValue;
            }
            return baseValue;
        }
    }
}