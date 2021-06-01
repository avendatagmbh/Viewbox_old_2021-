// --------------------------------------------------------------------------------
// author: Sebastian Vetter
// since: 2012-11-29
// copyright 2012 AvenDATA GmbH
// --------------------------------------------------------------------------------
using System;
using System.Windows.Data;

namespace eBalanceKit.Converters {
    public class DateComparisonToBoolConverter : IValueConverter {
        public DateComparisonToBoolConverter() {
            Default = true;
            DateMustBeBefore = false;
        }

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture) {
            DateTime dateValue;
            DateTime dateParam;
            if (parameter == null || !DateTime.TryParse(parameter.ToString(), out dateParam)) {
                dateParam = DateTime.Now;
            }

            if (value == null || !DateTime.TryParse(value.ToString(), out dateValue)) {
                return Default;
            }

            var res = Compare(dateValue, dateParam);
            
            // dateValue is before dateParam
            if (res >= 0) {
                return DateMustBeBefore ? true : false;
            }

            if (res <= 0) {
                return DateMustBeBefore ? false : true;
            }

            return Default;
        }

        private int Compare(DateTime value, DateTime param) {
            if (ExactComprison) {
                return DateTime.Compare(value, param);
            }
            var vDate = new DateTime(value.Year, value.Month, value.Day);
            var pData = new DateTime(param.Year, param.Month, param.Day);

            return DateTime.Compare(vDate, pData);
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture) {
            throw new NotImplementedException();
        }

        public bool DateMustBeBefore { get; set; }
        public bool Default { get; set; }
        public bool ExactComprison { get; set; }
 
    }
}