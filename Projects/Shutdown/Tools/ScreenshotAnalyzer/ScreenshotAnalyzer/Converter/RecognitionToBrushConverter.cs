using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Data;
using System.Windows.Media;
using AvdCommon.DataGridHelper;
using ScreenshotAnalyzerBusiness.Structures.Results;

namespace ScreenshotAnalyzer.Converter {
    internal class RecognitionToBrushConverter : IValueConverter {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture) {
            ;// return System.Convert.ToInt32(value) != 0;
            DataRow row = (DataRow) value;
            ResultRowEntry rowEntry = (ResultRowEntry) row.RowEntries[System.Convert.ToInt32(parameter)];
            if (rowEntry.DisplayString != rowEntry.EditedText) return Brushes.Yellow;
            return Brushes.White;
        }

        public object ConvertBack(object value, Type targetType, object parameter,
                                  System.Globalization.CultureInfo culture) {
            throw new NotImplementedException();
        }
    }
}
