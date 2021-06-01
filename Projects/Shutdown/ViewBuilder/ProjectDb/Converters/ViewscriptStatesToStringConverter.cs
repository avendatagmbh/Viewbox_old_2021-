using System;
using System.Globalization;
using System.Windows.Data;
using ViewBuilderCommon;

namespace Project.Converters
{
    [ValueConversion(typeof (ViewscriptStates), typeof (string))]
    public class ViewscriptStatesToStringConverter : IValueConverter
    {
        #region IValueConverter Members

        /// <summary>
        ///   Konvertiert einen Wert.
        /// </summary>
        /// <param name="value"> Der von der Bindungsquelle erzeugte Wert. </param>
        /// <param name="targetType"> Der Typ der Bindungsziel-Eigenschaft. </param>
        /// <param name="parameter"> Der zu verwendende Konverterparameter. </param>
        /// <param name="culture"> Die im Konverter zu verwendende Kultur. </param>
        /// <returns> Ein konvertierter Wert.Wenn die Methode null zurückgibt, wird der gültige NULL-Wert verwendet. </returns>
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            switch ((ViewscriptStates) value)
            {
                case ViewscriptStates.Ready:
                    return "Neuer View";
                case ViewscriptStates.CreatingIndex:
                    return "Erzeuge Indizes";
                case ViewscriptStates.CreateIndexError:
                    return "Fehler bei der Indexerstellung";
                case ViewscriptStates.CreatingTable:
                    return "Erstelle View";
                case ViewscriptStates.CreateTableError:
                    return "Fehler bei der Viewerstellung";
                case ViewscriptStates.CopyingTable:
                    return "Kopiere View in Zieldatenbank";
                case ViewscriptStates.CopyError:
                    return "Fehler beim Kopieren";
                case ViewscriptStates.Completed:
                    return "Vieweinspielung erfolgreich";
                case ViewscriptStates.Warning:
                    return "Warnung";
                case ViewscriptStates.CheckingReportParameters:
                    return "Checking report parameters (static view)";
                case ViewscriptStates.CheckingProcedureParameters:
                    return "Checking procedure parameters (dynamic view)";
                case ViewscriptStates.CheckingWhereCondition:
                    return "Checking report parameters (static view)";
                case ViewscriptStates.GettingIndexInfo:
                    return "Getting index data for tables";
                case ViewscriptStates.GeneratingDistinctValues:
                    return "Generating distinct data for parameters";
                case ViewscriptStates.CheckingReportParametersError:
                    return "Checking report parameters (static view) failed";
                case ViewscriptStates.CheckingProcedureParametersError:
                    return "Checking procedure parameters (dynamic view) failed";
                case ViewscriptStates.CheckingWhereConditionError:
                    return "Checking report parameters (static view)";
                case ViewscriptStates.GettingIndexInfoError:
                    return "Getting index data for tables failed";
                case ViewscriptStates.GeneratingDistinctValuesError:
                    return "Generating distinct data for parameters failed";
            }
            return string.Empty;
        }

        /// <summary>
        ///   Konvertiert einen Wert.
        /// </summary>
        /// <param name="value"> Der Wert, der vom Bindungsziel erzeugt wird. </param>
        /// <param name="targetType"> Der Typ, in den konvertiert werden soll. </param>
        /// <param name="parameter"> Der zu verwendende Konverterparameter. </param>
        /// <param name="culture"> Die im Konverter zu verwendende Kultur. </param>
        /// <returns> Ein konvertierter Wert.Wenn die Methode null zurückgibt, wird der gültige NULL-Wert verwendet. </returns>
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}