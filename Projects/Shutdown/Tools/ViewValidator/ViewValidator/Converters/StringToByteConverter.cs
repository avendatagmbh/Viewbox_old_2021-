using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Data;
using ViewValidatorLogic.Structures.Results;

namespace ViewValidator.Converters {
    class StringToByteConverter : IValueConverter {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture) {
            if (!(parameter is KeyValuePair<int, int>)) return "";
            KeyValuePair<int, int> param = (KeyValuePair<int, int>) parameter;
            int index = param.Key;
            int column = param.Value;
            RowDifference rowDifference = value as RowDifference;
            
            return StringToHex(rowDifference.Rows[index][column].ToString());
        }

        private string StringToHex(string hexstring) {
            var sb = new StringBuilder();
            foreach (char t in hexstring)
                sb.Append("0x" + System.Convert.ToInt32(t).ToString("x") + " ");
            return sb.ToString();
        }


        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture) {
            throw new NotImplementedException();
        }
    }
}
