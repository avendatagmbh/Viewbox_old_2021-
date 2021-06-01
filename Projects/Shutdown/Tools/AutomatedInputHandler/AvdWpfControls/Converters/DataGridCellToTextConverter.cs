using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Data;
using System.Data;
using System.Collections.ObjectModel;
using System.Windows;
using System.Collections;

namespace AvdWpfControls.Converters
{
    public class DataGridCellToTextConverter : IMultiValueConverter
    {
        #region IMultiValueConverter Members

        public object Convert(object[] values, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            int colIndex = (int)values[0];
            if (colIndex > -1)
            {
                if (values[1] != DependencyProperty.UnsetValue)
                {
                    IEnumerable<object> row = (IEnumerable<object>)values[1];
                    List<object> cellList = row.ToList();

                    if(colIndex >= cellList.Count)
                        return "UnsetValue";

                    object cell = cellList[colIndex];
                    string cellValue = (string)cell.GetType().GetProperty("DisplayString").GetValue(cell, null);
                    return cellValue;
                }
            }

            return "UnsetValue";
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}
