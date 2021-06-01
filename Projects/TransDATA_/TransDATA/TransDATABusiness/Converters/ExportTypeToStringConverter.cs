using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Data;
using TransDATABusiness.Enums;
using System.Globalization;

namespace TransDATABusiness.Converters {

    [ValueConversion(typeof(ExportType), typeof(string))]
     public class ExportTypeToStringConverter : IValueConverter {
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
             switch ((ExportType)value) {

                 case ExportType.Numeric:
                     return "Numerisch";

                 case ExportType.Alphanumeric:
                     return "Alphanumerisch";

                 case ExportType.Date:
                     return "Datum";
             }

             return string.Empty;

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
