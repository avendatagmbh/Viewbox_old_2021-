using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using System.Windows.Data;

namespace AvdWpfControls.Converters
{
    public class TabSizeConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter,
            System.Globalization.CultureInfo culture)
        {
            if (values != null && values.Length > 0) {
                //TabControl tabControl = values[0] as TabControl;
                //if (tabControl != null) {
                //    double width = tabControl.ActualWidth/tabControl.Items.Count;
                //    //Subtract 1, otherwise we could overflow to two rows.
                //    return (width <= 1) ? 0 : (width - 1);
                //}
                TabControl tabControl = values[0] as TabControl;
                if (tabControl != null)
                {
                    foreach (var item in tabControl.Items) {
                        AvdTabItem tabItem = item as AvdTabItem;
                        if (tabItem != null) {
                            return tabItem.TabWidth;
                        }
                    }
                }
            }
            return null;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter,
            System.Globalization.CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}
