using System;
using System.Windows.Data;
using System.Windows.Media;

namespace ViewAssistant.Converters
{
    public class ItemSelectedToColorConverter : IValueConverter
    {
        public Brush SelectedBrush { get; set; }
        public Brush DeselectedBrush { get; set; }

        #region IValueConverter Members

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return (bool)value ? SelectedBrush : DeselectedBrush;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}
