/**************************************************************************************************************
 * author               date            comment
 * ------------------------------------------------------------------------------------------------------------
 * Mirko Dibbert        2010-10-30      initial implementation
 *************************************************************************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Data;
using System.Windows;

namespace ViewBuilder.Converters {

    public class ProgressToVisibilityConverter : IValueConverter {

        /// <summary>
        /// Konvertiert einen Wert.
        /// </summary>
        /// <param name="value">Der von der Bindungsquelle erzeugte Wert.</param>
        /// <param name="targetType">Der Typ der Bindungsziel-Eigenschaft.</param>
        /// <param name="parameter">Der zu verwendende Konverterparameter.</param>
        /// <param name="culture">Die im Konverter zu verwendende Kultur.</param>
        /// <returns>
        /// Ein konvertierter Wert.Wenn die Methode null zurückgibt, wird der gültige NULL-Wert verwendet.
        /// </returns>
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture) {
            int progress = System.Convert.ToInt32(value);
            if (progress < 100) return Visibility.Visible;
            else return Visibility.Collapsed;
        }

        /// <summary>
        /// Konvertiert einen Wert.
        /// </summary>
        /// <param name="value">Der Wert, der vom Bindungsziel erzeugt wird.</param>
        /// <param name="targetType">Der Typ, in den konvertiert werden soll.</param>
        /// <param name="parameter">Der zu verwendende Konverterparameter.</param>
        /// <param name="culture">Die im Konverter zu verwendende Kultur.</param>
        /// <returns>
        /// Ein konvertierter Wert.Wenn die Methode null zurückgibt, wird der gültige NULL-Wert verwendet.
        /// </returns>
        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture) {
            throw new NotImplementedException();
        }
    }
}
