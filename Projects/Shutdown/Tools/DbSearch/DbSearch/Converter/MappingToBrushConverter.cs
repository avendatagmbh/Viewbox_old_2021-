using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Data;
using System.Windows.Media;
using DbSearch.Manager;
using DbSearchLogic.SearchCore.Result;
using DbSearchLogic.SearchCore.Structures.Db;

namespace DbSearch.Converter {
    class MappingToBrushConverter : IValueConverter {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture) {
            if (value == null) return Brushes.Transparent;
            if (value is TableInfo) return ColorManager<TableInfo>.GetBrush((TableInfo)value);
            return ColorManager<TableInfo>.GetBrush(((ColumnMapping)value).ResultTable);
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture) {
            throw new NotSupportedException();
        }
    }
}
