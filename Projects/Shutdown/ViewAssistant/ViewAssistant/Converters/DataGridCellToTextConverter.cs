// --------------------------------------------------------------------------------
// author: ???
// since: 2013-10-18
// copyright 2013 AvenDATA GmbH
// --------------------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Data;

namespace ViewAssistant.Converters {
    public class DataGridCellToTextConverter : IMultiValueConverter {
        #region IMultiValueConverter Members
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture) {
            var colIndex = (int) values[0];
            if (colIndex > -1) {
                if (values[1] != DependencyProperty.UnsetValue) {
                    var row = (IEnumerable<object>) values[1];
                    List<object> cellList = row.ToList();

                    if (colIndex >= cellList.Count)
                        return "UnsetValue";

                    object cell = cellList[colIndex];
                    var cellValue = (string) cell.GetType().GetProperty("DisplayString").GetValue(cell, null);
                    return cellValue;
                }
            }

            return "UnsetValue";
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture) { throw new NotImplementedException(); }
        #endregion
    }
}