using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Data;
using System.Windows;
using Taxonomy;
using Taxonomy.Enums;

namespace eBalanceKit.Converters {
    
    /// <summary>
    /// 
    /// </summary>
    /// <author>Mirko Dibbert</author>
    /// <since>2011-06-07</since>
    internal class MandatoryTypeAccountBalancesToVisibilityConverter : IValueConverter {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture) {
            if (value == null) return Visibility.Collapsed;
            if (!(value is MandatoryType)) return Visibility.Collapsed;
            switch ((MandatoryType)value) {
                case MandatoryType.AccountBalance:
                    return Visibility.Visible;

                default:
                    return Visibility.Collapsed;
            }           
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture) {
            throw new NotImplementedException();
        }
    }
}
