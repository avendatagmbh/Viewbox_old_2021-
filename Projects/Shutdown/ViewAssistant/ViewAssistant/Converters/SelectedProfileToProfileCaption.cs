using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Data;
using Base.Localisation;
using ViewAssistantBusiness;

namespace ViewAssistant.Converters
{
    public class SelectedProfileToProfileCaption : IValueConverter
    {
        #region IValueConverter Members

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value == null || !(value is ProfileConfigModel))
                return ResourcesCommon.BtnProfilesCaption;
            else return (value as ProfileConfigModel).Name;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return null;
        }

        #endregion
    }
}
