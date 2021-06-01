/**************************************************************************************************************
 * author               date            comment
 * ------------------------------------------------------------------------------------------------------------
 * Mirko Dibbert        2010-10-22      initial implementation
 *************************************************************************************************************/

using System;
using System.Windows;
using System.Windows.Data;

namespace ViewValidator.Converters {

    public class EnumToVisibilityConverter : IValueConverter {

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
            string ParameterString = parameter as string;
            if (ParameterString == null) {
                return Visibility.Hidden;
            }

            if (Enum.IsDefined(value.GetType(), value) == false) {
                return Visibility.Hidden;
            }

            object paramvalue = Enum.Parse(value.GetType(), ParameterString);

            if (paramvalue.Equals(value)) {
                return Visibility.Visible;
            } else {
                return Visibility.Hidden;
            }
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
            string ParameterString = parameter as string;
            if (ParameterString == null) {
                return DependencyProperty.UnsetValue;
            }

            return Enum.Parse(targetType, ParameterString);
        }
    }
}
