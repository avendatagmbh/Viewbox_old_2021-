using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Data;
using System.Windows.Media;
using ViewValidator.Manager;
using ViewValidatorLogic.Interfaces;
using ViewValidatorLogic.Structures.Results;

namespace ViewValidator.Converters {
    class RuleEntryTypeToBrushConverter : IValueConverter {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture) {
            int column = (int)(parameter);
            RowDifference rowDifference = value as RowDifference;
            return ColorManager.BrushFromRowEntryType(rowDifference.Rows[0][column].RowEntryType);
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture) {
            throw new NotImplementedException();
        }
    }
}
