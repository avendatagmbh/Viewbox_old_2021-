using System;
using System.Globalization;
using System.Windows.Data;

namespace TransDATA.Converter {
    internal class StringToNullableDecimalConverter : IValueConverter {
        #region IValueConverter Members
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            if (value == null) return "";
            string strval = value.ToString();
            decimal decval = decimal.Parse(strval);
            return decval.ToString("#,0.##########", new CultureInfo("de-DE"));
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
            try {
                string strval = value.ToString().Replace(" ", "");
                if (string.IsNullOrEmpty(strval)) return null;
                decimal decval = decimal.Parse(strval, new CultureInfo("de-DE"));
                return decval;
            } catch (Exception) {
                return null;
            }
        }
        #endregion
    }

    internal class StringToDecimalConverter : IValueConverter {
        #region IValueConverter Members
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            if (value == null) return "";
            string strval = value.ToString();
            decimal decval = decimal.Parse(strval);

            return decval.ToString("#,0.##########", new CultureInfo("de-DE"));
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
            try {
                string strval = value.ToString().Replace(" ", "");
                if (string.IsNullOrEmpty(strval)) return 0;
                decimal decval = decimal.Parse(strval, new CultureInfo("de-DE"));
                return decval;
            } catch (Exception) {
                return 0;
            }
        }
        #endregion
    }

    internal class StringToNullableMonetaryConverter : IValueConverter {
        #region IValueConverter Members
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            if (value == null) return "";
            string strval = value.ToString();
            decimal decval = decimal.Parse(strval);
            return decval.ToString("#,0.00", new CultureInfo("de-DE"));
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
            try {
                string strval = value.ToString().Replace(" ", "");
                if (string.IsNullOrEmpty(strval)) return null;
                decimal decval = decimal.Parse(strval, new CultureInfo("de-DE"));
                return decval;
            } catch (Exception) {
                return null;
            }
        }
        #endregion
    }

    internal class StringToMonetaryConverter : IValueConverter {
        #region IValueConverter Members
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            if (value == null) return "";
            string strval = value.ToString();
            decimal decval = decimal.Parse(strval);

            return decval.ToString("#,0.00", new CultureInfo("de-DE"));
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
            try {
                string strval = value.ToString().Replace(" ", "");
                if (string.IsNullOrEmpty(strval)) return 0;
                decimal decval = decimal.Parse(strval, new CultureInfo("de-DE"));
                return decval;
            } catch (Exception) {
                return 0;
            }
        }
        #endregion
    }
}