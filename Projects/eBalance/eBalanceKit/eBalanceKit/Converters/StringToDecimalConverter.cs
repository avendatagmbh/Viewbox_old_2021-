using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Data;
using eBalanceKitBase.Structures;

namespace eBalanceKit.Converters {

    class StringToNullableDecimalConverter : IValueConverter {

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture) {
            if (value == null) return "";
            string strval = value.ToString();
            decimal decval = decimal.Parse(strval);
            return decval.ToString("#,0.##########", new System.Globalization.CultureInfo("de-DE"));
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture) {
            try {
                string strval = value.ToString().Replace(" ", "");
                if (string.IsNullOrEmpty(strval)) return null;
                decimal decval = decimal.Parse(strval, new System.Globalization.CultureInfo("de-DE"));
                return decval;
            } catch (Exception) {
                return null;
            }
        }
    }

    class StringToDecimalConverter : IValueConverter {

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture) {
            if (value == null) return "";
            string strval = value.ToString();
            decimal decval = decimal.Parse(strval);
            
            return decval.ToString("#,0.##########", new System.Globalization.CultureInfo("de-DE"));
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture) {
            try {
                string strval = value.ToString().Replace(" ", "");
                if (string.IsNullOrEmpty(strval)) return 0;
                decimal decval = decimal.Parse(strval, new System.Globalization.CultureInfo("de-DE"));
                return decval;
            } catch (Exception) {
                return 0;
            }
        }
    }

    class StringToNullableMonetaryConverter : IValueConverter {

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture) {
            if (value == null) return "";
            string strval = value.ToString();
            decimal decval = decimal.Parse(strval);
            return LocalisationUtils.DecimalToString(decval);
            //return decval.ToString("#,0.00", new System.Globalization.CultureInfo("de-DE"));
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture) {
            try {
                string strval = value.ToString().Replace(" ", "");
                if (string.IsNullOrEmpty(strval)) return null;
                decimal decval = decimal.Parse(strval, LocalisationUtils.GermanCulture);
                return decval;
            } catch (Exception) {
                return null;
            }
        }
    }

    class StringToMonetaryConverter : IValueConverter {

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture) {
            if (value == null) return "";
            string strval = value.ToString();
            decimal decval = decimal.Parse(strval);

            return decval.ToString("#,0.00", new System.Globalization.CultureInfo("de-DE"));
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture) {
            try {
                string strval = value.ToString().Replace(" ", "");
                if (string.IsNullOrEmpty(strval)) return 0;
                decimal decval = decimal.Parse(strval, new System.Globalization.CultureInfo("de-DE"));
                return decval;
            } catch (Exception) {
                return 0;
            }
        }
    }
}
