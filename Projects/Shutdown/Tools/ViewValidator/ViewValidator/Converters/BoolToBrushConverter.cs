/**************************************************************************************************************
 * author               date            comment
 * ------------------------------------------------------------------------------------------------------------
 * Mirko Dibbert        2010-10-30      initial implementation
 *************************************************************************************************************/

using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace ViewValidator.Converters {
   
    [ValueConversion(typeof(bool), typeof(Brush))]
    public class BoolToBrushConverter : IValueConverter {
        
        /// <summary>
        /// Gets or sets the true color.
        /// </summary>
        /// <value>The true color.</value>
        public Brush True { get; set; }

        /// <summary>
        /// Gets or sets the false color.
        /// </summary>
        /// <value>The false color.</value>
        public Brush False { get; set; }

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
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {            
            if (targetType != typeof(Brush)) return null;
            return ((bool)value) ? True : False;
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
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
            throw new NotImplementedException();
        }
    }
}
