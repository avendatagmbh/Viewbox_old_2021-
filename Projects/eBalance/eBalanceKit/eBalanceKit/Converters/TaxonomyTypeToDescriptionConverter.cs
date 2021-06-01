// --------------------------------------------------------------------------------
// author: Sebastian Vetter
// since: 2012-10-18
// copyright 2012 AvenDATA GmbH
// --------------------------------------------------------------------------------
using System;
using System.Windows.Data;
using eBalanceKitResources.Localisation;

namespace eBalanceKit.Converters {
    public class TaxonomyTypeToDescriptionConverter : IValueConverter {

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

            return eBalanceKitResources.Localisation.ResourcesTaxonomy.ResourceManager.GetString(
                "TaxonomyVizualizationDescription" + value.ToString());
            
            //switch ((XbrlElementValueTypes)value) {

            //    case XbrlElementValueTypes.Tuple:
            //        return null;

            //    case XbrlElementValueTypes.Abstract:
            //        return null;

            //    case XbrlElementValueTypes.Boolean:
            //        return null;

            //    case XbrlElementValueTypes.Date:
            //        return null;
            //    //return "/eBilanz-Kit;component/Resources/Kalender24.png";

            //    case XbrlElementValueTypes.Int:
            //        return null;

            //    case XbrlElementValueTypes.Monetary:
            //        return null;
            //    //return "/eBilanz-Kit;component/Resources/monetary.png";

            //    case XbrlElementValueTypes.Numeric:
            //        return null;

            //    case XbrlElementValueTypes.String:
            //        return null;
            //    //return "/eBilanz-Kit;component/Resources/Qute/Fonts16.png";

            //    default:
            //        throw new NotImplementedException();
            //}
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
