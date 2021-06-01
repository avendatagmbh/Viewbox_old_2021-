using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media.Imaging;
using AV.Log;
using DbAccess.Structures;
using log4net;

namespace AvdCommon.Converter
{
    public class ColumnTypeToImageSourceConverter : IValueConverter
    {
        internal ILog _log = LogHelper.GetLogger();

        #region IValueConverter Members

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string source;
            switch ((DbColumnTypes) value)
            {
                case DbColumnTypes.DbNumeric:
                case DbColumnTypes.DbInt:
                case DbColumnTypes.DbBigInt:
                case DbColumnTypes.DbBool:
                    source = "/Resources/ColumnTypes/numeric.png";
                    break;
                case DbColumnTypes.DbText:
                case DbColumnTypes.DbLongText:
                    source = "/Resources/ColumnTypes/text.png";
                    break;
                case DbColumnTypes.DbDate:
                case DbColumnTypes.DbTime:
                case DbColumnTypes.DbDateTime:
                    source = "/Resources/ColumnTypes/Clock24.png";
                    break;
                    //case DbColumnTypes.DbBinary:
                    //    break;
                case DbColumnTypes.DbUnknown:
                    //LogManager.Warning("Unbekannter Datentyp gefunden");
                    _log.Log(LogLevelEnum.Warn, "Unbekannter Datentyp gefunden");
                    return null;
                default:
                    //LogManager.Error("Kein Bild für den Spaltentyp: " + value.ToString());
                    _log.Log(LogLevelEnum.Error, "Kein Bild für den Spaltentyp: " + value);
                    return null;
                    //throw new ArgumentOutOfRangeException("Kein Bild für den Spaltentyp: " + value.ToString());
            }
            //return new BitmapImage(new Uri(source));
            return new BitmapImage(new Uri("pack://application:,,,/AvdCommon;component" + source));
            //return new BitmapImage(new Uri("pack://application:,,," + source));
        }

        public object ConvertBack(object value, Type targetType, object parameter,
                                  CultureInfo culture)
        {
            throw new NotSupportedException();
        }

        #endregion
    }
}