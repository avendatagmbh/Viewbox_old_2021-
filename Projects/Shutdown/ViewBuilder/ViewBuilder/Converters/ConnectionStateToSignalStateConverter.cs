/**************************************************************************************************************
 * author               date            comment
 * ------------------------------------------------------------------------------------------------------------
 * Mirko Dibbert        2010-10-23      initial implementation
 *************************************************************************************************************/

using System;
using System.Windows.Data;
using AvdWpfControls;
using DbAccess;

namespace ViewBuilder.Converters {

    public class ConnectionStateToSignalStateConverter : IValueConverter {
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
            ConnectionStates state = (ConnectionStates)value;
            if (state == ConnectionStates.Online) return SignalLightStates.Green;
            else if (state == ConnectionStates.Offline) return SignalLightStates.Red;
            else return SignalLightStates.Yellow;
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
            SignalLightStates state = (SignalLightStates)value;
            if (state == SignalLightStates.Green) return ConnectionStates.Online;
            else if (state == SignalLightStates.Red) return ConnectionStates.Offline;
            else return ConnectionStates.Connecting;
        }
    }
}
