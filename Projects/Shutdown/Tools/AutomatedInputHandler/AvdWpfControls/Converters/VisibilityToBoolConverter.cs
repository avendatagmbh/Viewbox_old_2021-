// --------------------------------------------------------------------------------
// author: Sebastian Vetter
// since: 2012-05-31
// copyright 2012 AvenDATA GmbH
// --------------------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Data;
using System.Windows;

namespace AvdWpfControls.Converters {
    public class VisibilityToBoolConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter,
                              System.Globalization.CultureInfo culture)
        {
            try
            {
                Visibility visibilityToConvert = (Visibility)value;
                if (   (visibilityToConvert == Visibility.Collapsed)
                    || (visibilityToConvert == Visibility.Hidden)    )
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            catch
            {
                return false;
            }
        }


        public object ConvertBack(object value, Type targetType, object parameter,
                                  System.Globalization.CultureInfo culture)
        {

            try
            {
                bool boolToConvert = (bool)value;
                if (boolToConvert)
                {
                    return Visibility.Collapsed;
                }
                else
                {
                    return Visibility.Visible;
                }
            }
            catch
            {
                return Visibility.Collapsed;
            }
        }

    }
}