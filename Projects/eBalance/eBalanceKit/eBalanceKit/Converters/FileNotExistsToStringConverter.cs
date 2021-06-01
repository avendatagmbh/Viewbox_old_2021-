// --------------------------------------------------------------------------------
// author: Sebastian Vetter
// since: 2012-05-31
// copyright 2012 AvenDATA GmbH
// --------------------------------------------------------------------------------
using System;
using System.Windows.Data;

namespace eBalanceKit.Converters {
    /// <summary>
    /// Returns the file name if the file exists. 
    /// Otherwise it gives back FileNotExistingMessage (default: eBalanceKitResources.Localisation.ResourcesReconciliation.ReconciliationFileNotFound)
    /// or if given the parameter as string (placeholder {0} allways replaced by the file name).
    /// If the file path is null or String.Empty it returns String.Empty.
    /// </summary>
    public class FileNotExistsToStringConverter : IValueConverter {

        public FileNotExistsToStringConverter() { FileNotExistingMessage = eBalanceKitResources.Localisation.ResourcesReconciliation.ReconciliationFileNotFound; }

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture) {
            if (value == null) return string.Empty;

            if (!System.IO.File.Exists(value.ToString())) {
                return parameter == null
                           ? string.Format(FileNotExistingMessage, value)
                           : null;// string.Format(parameter.ToString(), value);
            }

            return null;// value.ToString();
        }

        public string FileNotExistingMessage { get; set; }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture) {
            throw new NotImplementedException();
        }
    }
}
