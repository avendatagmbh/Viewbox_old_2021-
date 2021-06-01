using System;
using System.Windows.Data;
using Base.Localisation;
using ViewAssistantBusiness;

namespace ViewAssistant.Converters
{
    class SelectedProfileToWindowTitleConverter : IValueConverter
    {
        #region IValueConverter Members

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value == null || !(value is ProfileConfigModel))
                return ResourcesCommon.DlgMainCaption;
            return String.Format("{0} - {1}", ResourcesCommon.DlgMainCaption, (value as ProfileConfigModel).Name);
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return null;
        }

        #endregion
    }
}
