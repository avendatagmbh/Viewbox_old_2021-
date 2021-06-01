using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Data;
using System.Windows.Media;
using DbSearchLogic.Structures.TableRelated;

namespace DbSearch.Converter {
    class StatusToBackgroundConverter : IValueConverter {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture) {
            if (targetType != typeof(Brush))
                throw new InvalidOperationException("The target must be a brush");
            switch((RowEntryStatus) value) {
                case RowEntryStatus.Used:
                    return Brushes.White;
                case RowEntryStatus.NullOrEmpty:
                    return Brushes.Gainsboro;
                case RowEntryStatus.Duplicate:
                    return Brushes.Gainsboro;
                default:
                    throw new ArgumentOutOfRangeException("Unknown row entry status");
            }
            
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture) {
            throw new NotSupportedException();
        }
    }
}
