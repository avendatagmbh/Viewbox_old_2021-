// --------------------------------------------------------------------------------
// author: Marcus Gerlach
// since: 2012-02-13
// copyright 2012 AvenDATA GmbH
// --------------------------------------------------------------------------------
using System.Windows.Data;
namespace TransDATA.Converter {
    public class PercentageConverter : IValueConverter {

        public object Convert(object value, System.Type targetType, object parameter, System.Globalization.CultureInfo culture) {
            return System.Convert.ToDouble(value) *
               System.Convert.ToDouble(parameter);

        }

        public object ConvertBack(object value, System.Type targetType, object parameter, System.Globalization.CultureInfo culture) {
            throw new System.NotImplementedException();
        }
    }
}