using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Windows.Data;

namespace TransDATA.Converter
{
    public class MyDebugConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return ConvertBack(value, targetType, parameter, culture);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) 
        {
            string typeStr="";
            try {
                if (value == null) {
                    typeStr = "NULL";
                }
                else {
                    typeStr = value.GetType().ToString();
                }
            } catch (Exception) {
            }

            string valueStr = "";
            try
            {
                valueStr = value.ToString();
            }
            catch (Exception)
            {
            }

            string paramStr = "";
            try {
                paramStr = parameter.ToString();
            }
            catch (Exception)
            {
            }

            try
            {
                System.Diagnostics.Debug.WriteLine("{0}\t{1}\t{2}", paramStr, typeStr, valueStr);
            }
            catch (Exception)
            {
            }

            if (paramStr == "CtlSelectDatabase") // || typeStr == "NULL"
            {
                //System.Diagnostics.Debugger.Break();
            }

            return value;
        }
    }
}
